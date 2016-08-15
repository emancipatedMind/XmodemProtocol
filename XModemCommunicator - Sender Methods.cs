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
            if (State != XModemStates.Idle) { return; }
            State = XModemStates.Initializing;
            Reset();
            bool rebuildPackets = false;

            XModemProtocolSenderOptions localOptions = (XModemProtocolSenderOptions)options?.Clone() ?? new XModemProtocolSenderOptions();
            options.Buffer = null;
            options.Filename = null;

            if (localOptions.Buffer != null) {
                Data = localOptions.Buffer.ToList();
                rebuildPackets = true;
            }
            // If user doesn't specify filename, check whether packets already exist
            // in system to see if user is just passing received information on or 
            // repeating last send.
            else if (localOptions.Filename != null ) {
                // Setting Filename
                Filename = localOptions.Filename;
                // Try to read file passed in.
                Data = Encoding.ASCII.GetBytes(File.ReadAllText(Filename)).ToList();
                rebuildPackets = true;
            }

            if (Data == null) {
                Abort(new AbortedEventArgs(XModemAbortReason.BufferEmpty));
                return;
            }

            if (localOptions.Mode != Mode) {
                Mode = localOptions.Mode;
                rebuildPackets = true;
            }

            if (rebuildPackets == true) BuildPackets();

            if (localOptions.InitializationTimeout > 0) {
                _initializationTimeOut = new System.Timers.Timer(localOptions.InitializationTimeout);
                _initializationTimeOut.Elapsed += (s, e) => {
                    _initializationTimeOut.Stop();
                    _initializationWaitHandle.Set();
                };
                _initializationTimeOut.Start();
            }

            try {
                OperationPending?.Invoke();
            }
            catch {
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                throw;
            }

            Port.Flush();
            State = XModemStates.SenderAwaitingInitializationFromReceiver;
            Task.Run(() => Send());
        }

        private void Send() {

            bool canDetected;
            bool handled = true;
            bool firstPass = true;

            try {
                while (true) {
                    handled = true;

                    if (_cancellationWaitHandle.WaitOne(0)) {
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.UserCancelled));
                    }

                    if (firstPass) {
                        if (_initializationWaitHandle.WaitOne(0)) {
                            throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout));
                        }
                        if (Port.BytesToRead == 0) continue;
                        _tempBuffer.AddRange(Encoding.ASCII.GetBytes(Port.ReadExisting()).ToList());
                        firstPass = false;
                    }
                    else {
                        try {
                            _tempBuffer.AddRange(new List<byte> { (byte)Port.ReadByte() });
                        }
                        catch (TimeoutException) {
                            throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout));
                        }
                    }

                    int tempBufferCount = _tempBuffer.Count;

                    if (tempBufferCount == 0) continue;

                    canDetected = _tempBuffer.Contains(CAN);
                    switch(State) {
                        case XModemStates.SenderAwaitingInitializationFromReceiver:
                            _initializationTimeOut?.Stop();
                            if ( DetectCancellation(_tempBuffer)) {
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                            }
                            if (canDetected) {
                                handled = false;
                                break;
                            }

                            if (_tempBuffer.Last() == C) {
                                if (Mode == XModemMode.Checksum) {
                                    _initializationTimeOut?.Start();
                                    firstPass = true;
                                    break;
                                }
                            }
                            else if (_tempBuffer.Last() == NAK) {
                                if (Mode != XModemMode.Checksum) {
                                    Mode = XModemMode.Checksum;
                                    BuildPackets();
                                }
                            }
                            else {
                                _initializationTimeOut?.Start();
                                firstPass = true;
                                break;
                            }
                            State = XModemStates.SenderPacketsBeingSent;
                            SendPacket();
                            break;

                        case XModemStates.SenderPacketsBeingSent:
                            if(tempBufferCount == 1) {
                                if (_tempBuffer[0] == ACK) {
                                    _packetIndexToSend++;
                                    ResetConsecutiveNAKs();
                                }
                                else {
                                    if (IncrementConsecutiveNAKs() == true) {
                                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                    }
                                }
                                SendPacket();
                            }
                            else {
                                if (DetectCancellation(_tempBuffer)) {
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                                }
                                if (canDetected) handled = false;
                            }
                            break;
                        case XModemStates.SenderAwaitingFinalACKFromReceiver:
                            if(tempBufferCount == 1) {
                                if (_tempBuffer[0] == ACK) {
                                    throw new XModemProtocolException(null);
                                }
                                else {
                                    if (IncrementConsecutiveNAKs() == true) {
                                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                    }
                                    SendPacket();
                                }
                            }
                            else {
                                if (DetectCancellation(_tempBuffer)) {
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                                }
                                if (canDetected) handled = false;
                            }
                            break;
                    }

                    if (handled) _tempBuffer = new List<byte>();
                }
            }
            catch (XModemProtocolException ex) {
                if (ex.AbortArgs != null) 
                    Abort(ex.AbortArgs);
                else {
                    CompleteOperation();
                }
            }
        }

        private void BuildPackets() {
            lock(this) {
                if (Data == null) return;
                Packets = new List<List<byte>>();
                byte header = PacketSize == XModemPacketSizes.OneK ? STX : SOH;
                
                for(int packetNumber = 1, position = 0, packetSize = (int)PacketSize; position < Data.Count; packetNumber++, position += packetSize) {
                    List<byte> packet = new List<byte> {
                        header,
                        (byte) packetNumber,
                        (byte) (0xFF - ((byte)packetNumber)) 
                    };
                    List<byte> packetInfo;
                    if (position + packetSize <= Data.Count)  {
                        packetInfo = Data.GetRange(position, packetSize);
                    }
                    else {
                        int restOfBytes = Data.Count - position;
                        int numbOfSUB = packetSize - restOfBytes;
                        packetInfo = Data.GetRange(position, restOfBytes).Concat(Enumerable.Repeat(SUB, numbOfSUB)).ToList();
                    }
                    packet.AddRange(packetInfo);
                    packet.AddRange(CheckSum(packetInfo));
                    Packets.Add(packet);
                }
                PacketsBuilt?.Invoke(this, new PacketsBuiltEventArgs(Packets));

            }
        }

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

            return false;
        }

        private void SendPacket() {
            List<byte> packet;
            bool fireEvent = true;
            int index = _packetIndexToSend;
            lock(this) {
                if (index < Packets.Count)  {
                    packet = Packets[index];
                }
                else {
                    fireEvent = false;
                    packet = new List<byte> { EOT };
                    State = XModemStates.SenderAwaitingFinalACKFromReceiver;
                }
            }
            if (fireEvent == true) {
                PacketToSend?.Invoke(this, new PacketToSendEventArgs(++index, packet)); 
            }
            Port.Write(packet.ToArray());
        }


    }
}