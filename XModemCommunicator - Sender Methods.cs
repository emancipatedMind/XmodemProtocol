using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {
        /// <summary>
        /// Sender Method.
        /// Initialize session as sender.
        /// </summary>
        public void InitializeSender(XModemProtocolSenderOptions options) {
            // First check whether the object state is idle.
            // If so, change state to Initializing, and reset XModemCommunicator.
            if (State != XModemStates.Idle) { return; }
            State = XModemStates.Initializing;
            Reset();
            bool rebuildPackets = false;

            // Get a clone of the options passed in. This is a deep copy so that it cannot be changed by outside world.
            XModemProtocolSenderOptions localOptions = (XModemProtocolSenderOptions)options?.Clone() ?? new XModemProtocolSenderOptions();

            // Remove data in Buffer, and Filename. They cannot be used more than once.
            options.Buffer = null;
            options.Filename = null;

            // Set the common options.
            SetCommonOptions(localOptions);

            // Buffer is the first place to look for data. If it has data, get this data, and set flag indicating that 
            // packets need to be built.
            if (localOptions.Buffer != null) {
                Data = localOptions.Buffer.ToList();
                Filename = null;
                rebuildPackets = true;
            }

            // If user instead specifies filename, try to import the bytes from this file.
            // Any exceptions thrown will bubble to top.
            else if (localOptions.Filename != null ) {
                // Set Filename, try to read file passed in, and set flag indicating tht packets need to be built.
                Filename = localOptions.Filename;
                Data = File.ReadAllBytes(Filename).ToList();
                rebuildPackets = true;
            }

            // Check if Data is null. It is null if previous operation has not filled it.
            // A previous operation could be from past Send, past Receive, or from this current 
            // iteration of Send.
            // If it is null, abort.
            if (Data == null) {
                Abort(new AbortedEventArgs(XModemAbortReason.BufferEmpty));
                return;
            }

            // If Mode has been changed, then update this, and rebuild the packets.
            if (localOptions.Mode != Mode) {
                Mode = localOptions.Mode;
                rebuildPackets = true;
            }

            // If checks have indicated that packets need to be built, perform.
            if (rebuildPackets == true) BuildPackets();

            // If user has indicated they would like to have a timeout, set that up.
            if (localOptions.InitializationTimeout > 0) {
                _initializationTimeOut = new System.Timers.Timer(localOptions.InitializationTimeout);
                _initializationTimeOut.Elapsed += (s, e) => {
                    _initializationTimeOut.Stop();
                    _initializationWaitHandle.Set();
                };
            }

            // Fire event in case user has specified method to run before operation begins.
            // If this event throws error, abort before rethrowing error.
            try {
                OperationPending?.Invoke();
            }
            catch {
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                throw;
            }

            // Flush port, change state, start initializationTimeOut if not null, and perform send.
            Port.Flush();
            State = XModemStates.SenderAwaitingInitializationFromReceiver;
            _initializationTimeOut?.Start();
            Task.Run(() => Send());
        }

        /// <summary>
        /// Method that performs send operation.
        /// </summary>
        private void Send() {

            _tempBuffer = new List<byte>();

            // Infinite loop wrapped in try block. The only way out of infinite loop is with Exception.
            try {
                while (true) {

                    // If operation cancelled, break out of infinite loop with an exception.
                    if (_cancellationWaitHandle.WaitOne(0)) {
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.UserCancelled));
                    }

                    switch (State) {
                        case XModemStates.SenderAwaitingInitializationFromReceiver:

                            if (Port.BytesToRead != 0) {
                                // If bytes exist at port, stop initializationTimeOut, and read all bytes at port.
                                _initializationTimeOut?.Stop();
                                _tempBuffer.AddRange(Port.ReadAllBytes());
                            }
                            // Check to see if we have bytes from previous iteration. If so, move on.
                            else if (_tempBuffer.Count != 0) { }
                            // If we have no bytes to read, check to see if initializationTimeOut expired.
                            else if (_initializationWaitHandle.WaitOne(0)) {
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout));
                            }
                            // If none of these conditions exist, start from top of loop.
                            else continue;

                            // If a cancellation is detected, throw XModemProtocolException ending operation.
                            if (DetectCancellation(_tempBuffer)) {
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                            }

                            // If a C is detected, check if user is forcing the checksum mode.
                            // If so, do not respond. Simply break out, and clear _tempBuffer.
                            // If CRC, or 1k mode in use, exit if statement.
                            if (_tempBuffer.Last() == C) {
                                if (Mode == XModemMode.Checksum) {
                                    _initializationTimeOut?.Start();
                                    break;
                                }
                            }

                            // If receiver sends NAK, and mode is not being forced to checksum,
                            // change mode to checksum, and rebuild packets before
                            // exiting if statement.
                            else if (_tempBuffer.Last() == NAK) {
                                if (Mode != XModemMode.Checksum) {
                                    Mode = XModemMode.Checksum;
                                    BuildPackets();
                                }
                            }

                            // If a cancellation byte is detected, but wasn't enough for cancellation, 
                            // check to see if this condition has lasted for an extended number of loops,
                            // and if so, break from switch statement clearing _tempBuffer.
                            // If not, start initializationTimeOut, and restart from top.
                            else if (_tempBuffer.Contains(CAN)) {
                                if (IncrementConsecutiveLoopsWithInsufficientCAN()) {
                                    _initializationTimeOut?.Start();
                                    continue;
                                }
                                else {
                                    break;
                                }
                            }

                            // Response was not understood, so restart initializationTimeOut, and 
                            // clear _tempBuffer.
                            else {
                                _initializationTimeOut?.Start();
                                break;
                            }

                            // Once logic has exited if statement above without breaking, change state, and send first packet.
                            State = XModemStates.SenderPacketsBeingSent;
                            SendPacket();
                            break;

                        case XModemStates.SenderAwaitingFinalACKFromReceiver:
                        case XModemStates.SenderPacketsBeingSent:
                            if (Port.BytesToRead != 0) {
                                // If bytes exist at port, read all bytes at port.
                                _tempBuffer.AddRange(new List<byte> { (byte)Port.ReadByte() });
                            }
                            // Check to see if bytes were in previous iteration.
                            else if (_tempBuffer.Count != 0) { }
                            // Continue if no data to examine.
                            else continue;

                            // If cancellation detected, exit infinite loop using exception.
                            if (DetectCancellation(_tempBuffer)) {
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                            }

                            // If last byte is ACK, perform next action.
                            // If awaiting final ACK, break free with exception.
                            // If not, increment _packetToSend, and send next packet.
                            if(_tempBuffer.Last() == ACK) {
                                if (State == XModemStates.SenderAwaitingFinalACKFromReceiver) {
                                    throw new XModemProtocolException();
                                } 
                                _packetIndexToSend++;
                                SendPacket();
                            }

                            // If last byte is NAK, resend packet.
                            else if (_tempBuffer.Last() == NAK) {
                                SendPacket();
                            }

                            // If a cancellation byte is detected, but wasn't enough for cancellation, 
                            // check to see if this condition has lasted for an extended number of loops,
                            // and if so, resend last packet. 
                            else if (_tempBuffer.Contains(CAN)) {
                                if (IncrementConsecutiveLoopsWithInsufficientCAN())
                                    continue;
                                else {
                                    SendPacket();
                                    break;
                                }
                            }

                            break;
                    }

                    _tempBuffer = new List<byte>();
                    ResetConsecutiveLoopsWithInsufficientCAN();
                }
            }

            // Only Exception caught here is XModemProtocolException. All others will bubble to top.
            catch (XModemProtocolException ex) {
                // If AbortArgs was provided with value, means that this is an abort.
                if (ex.AbortArgs != null) 
                    Abort(ex.AbortArgs);
                // If not, operation completed successfully.
                else {
                    CompleteOperation();
                }
            }
        }

        /// <summary>
        /// A method to build packets.
        /// </summary>
        private void BuildPackets() {

            // If no data, can't build packets. Return.
            if (Data == null) return;
            Packets = new List<List<byte>>();
            
            // Loop to build packets.
            for(int packetNumber = 1, position = 0, packetSize = 128; position < Data.Count; packetNumber++, position += packetSize) {
                // Get header byte.
                byte header = SOH;
                // If mode is OneK, a different header must be used if sending 1k packets.
                if (Mode == XModemMode.OneK) {
                    // If data left to send is less than limit set, use 128 packet size.
                    if (Data.Count - ( position + packetSize) <= 384 ) {
                        packetSize = 128;
                        header = SOH;
                    }
                    // If data to send is enough, use STX, and 1024 packet size.
                    else {
                        header = STX;
                        packetSize = 1024;
                    }
                }

                // Build the beginning of the packet.
                List<byte> packet = new List<byte> {
                    header,
                    (byte) packetNumber,
                    (byte) (0xFF - ((byte)packetNumber)) 
                };

                List<byte> packetInfo;
                // If Data left is more than packetSize, just grab the next packetSize number of bytes.
                if (position + packetSize <= Data.Count)  {
                    packetInfo = Data.GetRange(position, packetSize);
                }
                // If not, fill in rest of packet with SUB bytes.
                else {
                    int restOfBytes = Data.Count - position;
                    int numbOfSUB = packetSize - restOfBytes;
                    packetInfo = Data.GetRange(position, restOfBytes).Concat(Enumerable.Repeat(SUB, numbOfSUB)).ToList();
                }
                // Build rest of packet, and add to Packets List.
                packet.AddRange(packetInfo);
                packet.AddRange(CheckSum(packetInfo));
                Packets.Add(packet);
            }
            // Fire method indicating packets have finished being built.
            PacketsBuilt?.Invoke(this, new PacketsBuiltEventArgs(Packets));

        }

        /// <summary>
        /// A method to detect whether Cancellation has been received.
        /// </summary>
        /// <param name="recv">List holding bytes received</param>
        /// <returns>Was cancellation detected?</returns>
        private bool DetectCancellation(IEnumerable<byte> recv) {

            // If NumCancellationBytesRequired is less than 1, just exit.
            if (CancellationBytesRequired < 1) return false;

            // LINQ to get indices of CAN bytes.
            // 1). If byte is CAN, record index, if not, make index -1.
            // 2). Remove all elements equal to -1,
            // 3). Place elements in ascending order.
            // 4). Convert to List<byte>. Only need to perform LINQ once.
            var indicesOfCAN = (recv.Select((r, i) => { if (r == CAN) return i; else return -1; }).Where(i => i > -1).OrderBy(i => i)).ToList();

            // If the number of CANs found are not at least equal to the number required, no point in checking further.
            if (indicesOfCAN.Count < CancellationBytesRequired) return false;

            // Check to see if any consecutive amount of CANs found are above set limit.
            // If so, return true indicating such.
            for (int i = 0, counter = 0, indx = indicesOfCAN[0]; i < indicesOfCAN.Count; i++) {
                int next = indicesOfCAN[i];
                if (indx == next) {
                    ++indx;
                    ++counter;
                }
                else {
                    indx = next + 1;
                    counter = 1;
                }

                if (counter >= CancellationBytesRequired) return true;    
            }

            // No eligible CAN sequence found.
            return false;
        }

        /// <summary>
        /// Send a packet.
        /// </summary>
        private void SendPacket() {
            List<byte> packet;
            bool fireEvent = true;
            int index = _packetIndexToSend;

            // Check to see if index is within range of Packets.
            // If so, get the packet at that index.
            if (index < Packets.Count)  {
                packet = Packets[index];
            }
            // If not, this indicates that all packets have been transmitted.
            // Set flag indicating not to fire PacketToSend event. Set packet to EOT, and 
            // change state.
            else {
                fireEvent = false;
                packet = new List<byte> { EOT };
                State = XModemStates.SenderAwaitingFinalACKFromReceiver;
            }
            // If fireEvent flag is set, well... fire event.
            if (fireEvent == true) {
                PacketToSend?.Invoke(this, new PacketToSendEventArgs(++index, packet)); 
            }

            // Write out packet.
            Port.Write(packet);
        }

    }
}