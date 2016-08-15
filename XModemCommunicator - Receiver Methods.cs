using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        public void InitializeReceiver(XModemProtocolReceiverOptions options) {
            if (State != XModemStates.Idle) { return; }
            State = XModemStates.Initializing;
            Reset();

            XModemProtocolReceiverOptions localOptions = (XModemProtocolReceiverOptions)options?.Clone() ?? new XModemProtocolReceiverOptions();
            Mode = localOptions.Mode;
            if (localOptions.MaxNumberOfInitializationBytesForCRC > localOptions.MaxNumberOfInitializationBytesInTotal)
                MaxNumberOfInitializationBytesInTotal = MaxNumberOfInitializationBytesForCRC = localOptions.MaxNumberOfInitializationBytesForCRC;
            else {
                MaxNumberOfInitializationBytesForCRC = localOptions.MaxNumberOfInitializationBytesForCRC;
                MaxNumberOfInitializationBytesInTotal = localOptions.MaxNumberOfInitializationBytesInTotal;
            }

            _initializationTimeOut = new System.Timers.Timer(localOptions.InitializationTimeout);
            _initializationTimeOut.Elapsed += (s, e) => {
                _initializationTimeOut.Stop();
                _initializationWaitHandle.Set();
            };

            Data = new List<byte>();

            Packets = new List<List<byte>>();

            _initializationWaitHandle.Set();

            try {
                OperationPending?.Invoke();
            }
            catch {
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                throw;
            }

            Port.Flush();
            State = XModemStates.ReceiverSendingInitializationByte;
            Task.Run(() => Receive());
        }

        private void Receive() {
            try {
                byte initializationByte = Mode == XModemMode.Checksum ? NAK : C;
                int checksumByteCount = Mode == XModemMode.Checksum ? 1 : 2;

                int fullPacketSize = 3 + ((int) PacketSize)  + checksumByteCount;

                while(true) {
                    bool handled = true;
                    if (_cancellationWaitHandle.WaitOne(0)) {
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.UserCancelled));
                    }

                    switch(State) {
                        case XModemStates.ReceiverSendingInitializationByte:
                            if (Port.BytesToRead != 0) {
                                Task.Delay(50).Wait();
                                if (Port.BytesToRead >= fullPacketSize) {
                                    State = XModemStates.ReceiverReceivingPackets;
                                    _initializationTimeOut.Stop();
                                }
                            }
                            if (_initializationWaitHandle.WaitOne(0)) {
                                if (initializationByte == C) {
                                    if (++_countOfCsSent > MaxNumberOfInitializationBytesForCRC) {
                                        Mode = XModemMode.Checksum;
                                        initializationByte = NAK;
                                        fullPacketSize = 132;
                                    }
                                }
                                if(++_numOfInitializationBytesSent > MaxNumberOfInitializationBytesInTotal) {
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                                }
                                Port.Write(initializationByte);
                                _initializationWaitHandle.Reset();
                                _initializationTimeOut.Start();
                            }
                            break;
                        case XModemStates.ReceiverReceivingPackets:
                            if (Port.BytesToRead == 0) continue;
                            int bytesToRead = Port.BytesToRead;
                            byte[] byteArray = new byte[bytesToRead];
                            Port.Read(byteArray, 0, bytesToRead);
                            _tempBuffer.AddRange(byteArray.ToList());
                            if (_tempBuffer.Count == 1 && _tempBuffer[0] == EOT) {
                                SendACK();
                                throw new XModemProtocolException(null);
                            }
                            List<byte> packet;
                            if (_tempBuffer.Count < fullPacketSize) {
                                handled = false;
                                continue;
                            }
                            else if (_tempBuffer.Count > fullPacketSize) {
                                packet = new List<byte>(_tempBuffer.GetRange(0, fullPacketSize));
                                _tempBuffer.RemoveRange(0, fullPacketSize);
                            }
                            else {
                                packet = _tempBuffer;
                            }

                            if(ValidatePacket(packet) == true) {
                                SendACK();
                            }
                            else {
                                if(SendNAK() == true) 
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                            }
                            break;
                    }

                    if (handled) _tempBuffer = new List<byte>();
                }
            }
            catch(XModemProtocolException ex) {
                if (ex.AbortArgs != null) 
                    Abort(ex.AbortArgs);
                else {
                    for (int i = Data.Count - 1; i > -1; i--) {
                        if (Data[i] == SUB)
                            Data.RemoveAt(i);
                        else break;
                    }
                    CompleteOperation();
                }
            }
        }

        private bool ValidatePacket(IEnumerable<byte> buffer) {
            List<byte> packet = buffer.ToList();
            byte header = Mode == XModemMode.OneK ? STX : SOH;
            if (header != packet[0]) return false;
            if(packet[1] != ((byte)_packetIndexToReceive)) {
                if(packet[1] != ((byte)_packetIndexToReceive - 1)) {
                    return false;
                }
                return true;
            }
            byte inversePacketNumber = (byte)(0xFF - _packetIndexToReceive);
            if (packet[2] != inversePacketNumber) return false;
            
            List<byte> payLoad = packet.GetRange(3, (int)PacketSize);
            if (Mode == XModemMode.Checksum) {
                byte simpleChecksum = (byte)(payLoad.Sum(n => n));
                if (simpleChecksum != packet[131]) return false;
            }
            else {
                // This will need to verify checksum with CRC.
            }
            Packets.Add(packet);
            Data.AddRange(payLoad);
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(_packetIndexToReceive, packet));
            _packetIndexToReceive++;
            return true;
        }

        private void SendACK() {
            Port.Write(ACK);
            ResetConsecutiveNAKs();
        }

        private bool SendNAK() {
            Port.Write(NAK);
            return IncrementConsecutiveNAKs();
        }
    }
}