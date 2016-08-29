using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {
        /// <summary>
        /// Sender Method.
        /// Initialize session as sender.
        /// </summary>
        /// <param name="options">The options to be used this session. Pass in null to use past options.</param>
        public void Send(XModemProtocolOptions options = null) {
            // First check whether the object state is idle.
            // If so, change state to Initializing, change Role, and reset XModemCommunicator.
            if (State != XModemStates.Idle)  return;
            State = XModemStates.Initializing;
            Role = XModemRole.Sender;
            Reset();

            if (options != null) {
                // Get set options passed in.
                XModemProtocolOptions localOptions = (XModemProtocolOptions)options?.Clone();
                SetCommonOptions(localOptions);
                Mode = localOptions.Mode;
                SenderInitializationTimeout = localOptions.SenderInitializationTimeout;
            }

            // Check if _data is null. It is null if previous operation has not filled it.
            // A previous operation could either be a past Receive, or passed in through the
            // Data property.
            // If it is null, abort.
            if (_data == null) {
                Abort(new AbortedEventArgs(XModemAbortReason.BufferEmpty), false);
                return;
            }

            // If checks have indicated that packets need to be built, perform.
            if (_rebuildPackets == true) BuildPackets();

            // If user has indicated they would like to have a timeout, set that up.
            if (SenderInitializationTimeout > 0) {
                _initializationTimeOut = new System.Timers.Timer(SenderInitializationTimeout);
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
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed), false);
                throw;
            }

            // Flush port, change state, start initializationTimeOut if not null, and perform send.
            Port.Flush();
            State = XModemStates.SenderAwaitingInitializationFromReceiver;
            SendOperation();
        }

        /// <summary>
        /// Method that performs send operation.
        /// </summary>
        private void SendOperation() {

            _tempBuffer = new List<byte>();
            _initializationTimeOut?.Start();

            // Infinite loop wrapped in try block. The only way out of infinite loop is with Exception.
            try {
                while (true) {

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
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                            }
                            // If none of these conditions exist, start from top of loop.
                            else continue;

                            // If a cancellation is detected, throw XModemProtocolException ending operation.
                            if (DetectCancellation(_tempBuffer)) {
                                State = XModemStates.Cancelled;
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
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

                        case XModemStates.PendingCompletion:
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
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
                            }

                            // If last byte is ACK, perform next action.
                            // If awaiting final ACK, break free with exception.
                            // If not, increment _packetToSend, and send next packet.
                            if(_tempBuffer.Last() == ACK) {
                                if (State == XModemStates.PendingCompletion) {
                                    throw new XModemProtocolException();
                                } 
                                _packetIndex++;
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
                        // If operation cancelled, break out of infinite loop with an exception.
                        case XModemStates.Cancelled:
                            throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Cancelled), true);
                    }

                    _tempBuffer = new List<byte>();
                    _consecutiveLoopsWithCANs = 0;
                }
            }

            // Only Exception caught here is XModemProtocolException.
            catch (XModemProtocolException ex) {
                // If AbortArgs was provided with value, means that this is an abort.
                if (ex.AbortArgs != null)
                    Abort(ex.AbortArgs, ex.SendCancel);
                // If not, operation completed successfully.
                else {
                    CompleteOperation();
                }
            }
            //  All others will bubble to top after abort.
            catch (Exception) {
                Abort(new AbortedEventArgs(XModemAbortReason.OperationFailed), false);
                throw;
            }
        }

        /// <summary>
        /// A method to explicitly build packets. This is used when the
        /// count of the packets is needed. Upon completion of the method, whether called implicitly,
        /// or explicitly, the PacketsBuilt event is called.
        /// ArgumentNullException is thrown if no data has been passed into instance.
        /// </summary>
        /// <returns>The count of the packets.</returns>
        public int BuildPackets() {

            // If no data, can't build packets. Return.
            if (_data == null) throw new ArgumentNullException("Data is null.");
            Packets = new List<List<byte>>();
            
            // Loop to build packets.
            for(int packetNumber = 1, position = 0, packetSize = 128; position < _data.Count; packetNumber++, position += packetSize) {
                // Get header byte.
                byte header = SOH;
                // If mode is OneK, a different header must be used if sending 1k packets.
                if (Mode == XModemMode.OneK) {
                    // If data left to send is less than limit set, use 128 packet size.
                    if (_data.Count - ( position + packetSize) <= 128 ) {
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
                if (position + packetSize <= _data.Count)  {
                    packetInfo = _data.GetRange(position, packetSize);
                }
                // If not, fill in rest of packet with SUB bytes.
                else {
                    int restOfBytes = _data.Count - position;
                    int numbOfSUB = packetSize - restOfBytes;
                    packetInfo = _data.GetRange(position, restOfBytes).Concat(Enumerable.Repeat(SUB, numbOfSUB)).ToList();
                }
                // Build rest of packet, and add to Packets List.
                packet.AddRange(packetInfo);
                packet.AddRange(CheckSum(packetInfo));
                Packets.Add(packet);
            }
            // Fire method indicating packets have finished being built.
            Task.Run(() => PacketsBuilt?.Invoke(this, new PacketsBuiltEventArgs(Packets)));
            _rebuildPackets = false;
            return Packets.Count;
        }

        /// <summary>
        /// Send a packet.
        /// </summary>
        private void SendPacket() {
            List<byte> packet;
            bool fireEvent = true;
            int index = _packetIndex;

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
                State = XModemStates.PendingCompletion;
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