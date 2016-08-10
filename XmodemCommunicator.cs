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
            if (State != States.Idle) throw new XModemProtocolException("Cannot initialize unless object state is idle.");

            Reset();

            XModemProtocolSenderOptions localOptions = (XModemProtocolSenderOptions)options?.Clone() ?? new XModemProtocolSenderOptions();

            // If user doesn't specify filename, check whether packets already exist
            // in system to see if user is just passing received information on or 
            // repeating last send.
            if (localOptions.Filename == null ) {
                if (_buffer == null) throw new XModemProtocolException("No data to send. Specify file to send.");
            }

            // Setting Filename
            Filename = localOptions.Filename;

            // Try to read file passed in.
            try {
                _buffer = Encoding.ASCII.GetBytes(File.ReadAllText(Filename)).ToList();
            }
            catch (Exception ex) {
                throw new XModemProtocolException("Please check file.", ex);
            }

            Mode = localOptions.Mode;

            // Try to attach handler to SerialPort.DataReceived event.
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
                Abort(false, new AbortedEventArgs());
            };

            // _initializationTimeOut.Start();

            State = States.SenderAwaitingInitialization;

            // Can use token to cancel operation.
            Task.Run(() => Send());
        }

        private void BuildPackets() {
            lock(this) {
                if (_buffer == null) return;
                Packets = new List<List<byte>>();
                byte header = PacketSize == PacketSizes.OneK ? STX : SOH;
                
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
                PacketCount = Packets.Count;
                Packets.Add(new List<byte> {EOT});

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
            Reset();
            Aborted?.Invoke(this, e);
        }

        private void Reset() {
            _tempBuffer = new List<byte>();
            _initializationTimeOut?.Dispose();
            ConsecutiveNAKLimitPassed = null;
            State = States.Idle;
            ResetConsecutiveNAKs();
            _cancellationToken = System.Threading.CancellationToken.None;
            PacketIndexToSend = 0;
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

        private void IncrementConsecutiveNAKs() {
            _consecutiveNAKs++;
            if (_consecutiveNAKs > NAKBytesRequired) {
                ConsecutiveNAKLimitPassed?.Invoke();
                _consecutiveNAKs = 0;
            }
        }

        private void ResetConsecutiveNAKs() => _consecutiveNAKs = 0;

        private void SendPacket() {
            int index = PacketIndexToSend;
            List<byte> packet;
            int packetCount;
            lock(this) {
                packet = Packets[index];
                packetCount = Packets.Count;
            }
            index++;
            PacketSent?.Invoke(this, new PacketSentEventArgs(index, packet)); 
            if (index == packetCount) State = States.SenderAwaitingFinalACK;
            Port.Write(packet.ToArray());
        }

    }
}