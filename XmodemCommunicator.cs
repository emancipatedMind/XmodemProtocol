using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        public XModemCommunicator(System.IO.Ports.SerialPort port) {
            Port = port;
        }

        /// <summary>
        /// Sender Method.
        /// Initialize session as sender.
        /// </summary>
        public void InitializeSender(XModemProtocolSenderOptions options) {
            // First check whether the object state is idle.
            if (State != XModemStates.Idle) throw new XModemProtocolException("Cannot initialize unless object state is idle.");

            Reset();

            XModemProtocolSenderOptions localOptions = (XModemProtocolSenderOptions)options?.Clone() ?? new XModemProtocolSenderOptions();

            if (localOptions.Buffer != null) {
                _buffer = localOptions.Buffer.ToList();
                options.Buffer = null;
                options.Filename = null;
            }
            // If user doesn't specify filename, check whether packets already exist
            // in system to see if user is just passing received information on or 
            // repeating last send.
            else if (localOptions.Filename != null ) {
                // Setting Filename
                Filename = localOptions.Filename;
                options.Filename = null;
                // Try to read file passed in.
                try {
                    _buffer = Encoding.ASCII.GetBytes(File.ReadAllText(Filename)).ToList();
                }
                catch (Exception) { }

            }

            if (_buffer == null) {
                Abort(true, new AbortedEventArgs(XModemAbortReason.NoBytesSupplied));
                return;
            }


            Mode = localOptions.Mode;
            BuildPackets();

            // Flush both buffers.
            try {
                if (!Port.IsOpen) Port.Open();
                Port.Flush();
                if (localOptions.Prompt != null)
                    Port.Write(localOptions.Prompt.ToArray());
            }
            catch (Exception ex) {
                throw new XModemProtocolException("Port not in usable state.", ex);
            }

            _initializationTimeOut = new System.Timers.Timer(10000);
            _initializationTimeOut.Elapsed += (s, e) => {
                _initializationTimeOut.Stop();
                _sendOperationWaitHandle.Reset();
                Abort(false, new AbortedEventArgs(XModemAbortReason.Timeout));
            };

            // _initializationTimeOut.Start();

            State = XModemStates.SenderAwaitingInitialization;

            // Can use token to cancel operation.
            Task.Run(() => Send());
        }

        private void BuildPackets() {
            lock(this) {
                if (_buffer == null) return;
                Packets = new List<List<byte>>();
                byte header = PacketSize == XModemPacketSizes.OneK ? STX : SOH;
                
                for(int packetNumber = 1, position = 0, packetSize = (int)PacketSize; position < _buffer.Count; packetNumber++, position += packetSize) {
                    List<byte> packet = new List<byte> {
                        header,
                        (byte) packetNumber,
                        (byte) (0xFF - ((byte)packetNumber)) 
                    };
                    List<byte> packetInfo;
                    try {
                        packetInfo = _buffer.GetRange(position, packetSize);
                    }
                    catch (ArgumentException) {
                        int restOfBytes = _buffer.Count - position;
                        int numbOfSUB = packetSize - restOfBytes;
                        packetInfo = _buffer.GetRange(position, restOfBytes).Concat(Enumerable.Repeat(SUB, numbOfSUB)).ToList();
                    }
                    packet.AddRange(packetInfo);
                    packet.AddRange(CheckSum(packetInfo));
                    Packets.Add(packet);
                }
                PacketsBuilt?.Invoke(this, new PacketsBuiltEventArgs(Packets));

            }
        }

        private List<byte> CheckSum(List<byte> packetInfo) {
            if (Mode == XModemMode.Checksum)
                return new List<byte> { (byte)packetInfo.Sum(b => b) };
            else
                // This needs to produce CRC checksum.
                return new List<byte>();
        }

        private void Abort(bool sendCAN, AbortedEventArgs e) {
            if (sendCAN) Port.Write(Enumerable.Repeat(CAN, CANSentDuringAbort).ToArray());
            _sendOperationWaitHandle.Reset();
            State = XModemStates.Idle;
            Aborted?.Invoke(this, e);
        }

        private void Reset() {
            _tempBuffer = new List<byte>();
            _initializationTimeOut?.Dispose();
            State = XModemStates.Idle;
            ResetConsecutiveNAKs();
            _cancellationWaitHandle.Reset();
            _packetIndexToSend = 0;
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

        private bool IncrementConsecutiveNAKs() {
            _consecutiveNAKs++;
            if (_consecutiveNAKs > NAKBytesRequired) {
                _consecutiveNAKs = 0;
                return true;
            }
            return false;
        }

        private void ResetConsecutiveNAKs() => _consecutiveNAKs = 0;

        private void SendPacket() {
            List<byte> packet;
            bool fireEvent = true;
            int index = _packetIndexToSend;
            lock(this) {
                try {
                    packet = Packets[index];
                }
                catch (ArgumentOutOfRangeException) {
                    fireEvent = false;
                    packet = new List<byte> { EOT };
                    State = XModemStates.SenderAwaitingFinalACK;
                }
            }
            if (fireEvent == true) PacketSent?.Invoke(this, new PacketSentEventArgs(++index, packet)); 
            try {
                Port.Write(packet.ToArray());
            }
            catch (Exception ex) {
                throw new XModemProtocolException("Port not in usable state.", ex);
            }

        }

        /// <summary>
        /// Method used to cancel operation. If State is idle, method does nothing.
        /// </summary>
        /// <returns>If instance was in position to be cancelled, returns true. Otherwise, false.</returns>
        public bool CancelOperation() {
            if (State == XModemStates.Idle) return false;
            _cancellationWaitHandle.Set();
            return true;
        }

    }
}