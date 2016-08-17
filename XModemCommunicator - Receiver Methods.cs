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

            if (localOptions.Mode == XModemMode.CRC)
                Mode = XModemMode.OneK;
            else
                Mode = localOptions.Mode;

            SetCommonOptions(localOptions);

            ReceiverTimeoutDuringPacketReception = localOptions.ReceiverTimeoutForPacketReception;

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

                _tempBuffer = new List<byte>();

                byte initializationByte = Mode == XModemMode.Checksum ? NAK : C;

                System.Threading.ManualResetEvent receiverTimeoutWaitHandle = new System.Threading.ManualResetEvent(false); 

                System.Timers.Timer receiverWatchDog = new System.Timers.Timer(ReceiverTimeoutDuringPacketReception);
                receiverWatchDog.Elapsed += (s, e) => {
                    receiverTimeoutWaitHandle.Set();
                };

                while(true) {
                    bool handled = true;
                    if (_cancellationWaitHandle.WaitOne(0)) {
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.UserCancelled));
                    }

                    switch(State) {
                        case XModemStates.ReceiverSendingInitializationByte:
                            if (Port.BytesToRead != 0) {
                                State = XModemStates.ReceiverReceivingPackets;
                                _initializationTimeOut.Stop();
                            }
                            else if (_initializationWaitHandle.WaitOne(0)) {
                                if (initializationByte == C) {
                                    if (++_countOfCsSent > MaxNumberOfInitializationBytesForCRC) {
                                        Mode = XModemMode.Checksum;
                                        initializationByte = NAK;
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
                            if (Port.BytesToRead != 0) {
                                receiverWatchDog.Stop();
                                _tempBuffer.AddRange(Port.ReadAllBytes());
                                if (_tempBuffer[0] == EOT) {
                                    SendACK();
                                    throw new XModemProtocolException(null);
                                }
                                if (DetectCancellation(_tempBuffer)) {
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                                } 
                                if (_tempBuffer.Count < 132) {
                                    handled = false;
                                    receiverWatchDog.Start();
                                    continue;
                                }
                                int packetSize;
                                List<byte> packet;
                                if (Mode == XModemMode.Checksum)
                                    packetSize = 132;
                                else {
                                    if (_tempBuffer[0] == SOH) {
                                        packetSize = 133;
                                    }
                                    else if (_tempBuffer[0] == STX) {
                                        packetSize = 1029;
                                    }
                                    else {
                                        Port.Flush();
                                        if(SendNAK() == true) 
                                            throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                        continue;
                                    }
                                } 

                                if (_tempBuffer.Count < packetSize) {
                                    handled = false;
                                    receiverWatchDog.Start();
                                    continue;
                                }
                                if (_tempBuffer.Count > packetSize) handled = false;

                                packet = new List<byte>(_tempBuffer.GetRange(0, packetSize));
                                _tempBuffer.RemoveRange(0, packetSize);

                                if(ValidatePacket(packet) == true) {
                                    SendACK();
                                }
                                else {
                                    if(SendNAK() == true) 
                                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                }
                            }
                            else {
                                if (receiverTimeoutWaitHandle.WaitOne(0)) {
                                    if(SendNAK() == true) 
                                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout));
                                    receiverTimeoutWaitHandle.Reset();
                                }
                            }
                            receiverWatchDog.Start();
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
            bool packetVerifed = true;
            try {
                Exception ex;
                packet = buffer.ToList();
                int payLoadSize;
                if (Mode != XModemMode.OneK) {
                    if (packet[0] != SOH) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verfied", false);
                        throw ex;

                    }
                    payLoadSize = 128;
                }
                else {
                    if (packet[0] == SOH)
                        payLoadSize = 128;
                    else if (packet[0] == STX)
                        payLoadSize = 1024;
                    else {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verfied", false);
                        throw ex;
                    }
                }
                if (packet[1] != ((byte)_packetIndexToReceive)) {
                    if (packet[1] != ((byte)_packetIndexToReceive - 1)) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verfied", false);
                        throw ex;
                    }
                    ex = new Exception();
                    ex.Data.Add("Verfied", true);
                    throw ex;
                }
                byte inversePacketNumber = (byte)(0xFF - _packetIndexToReceive);
                if (packet[2] != inversePacketNumber) {
                    ex = new XModemProtocolException();
                    ex.Data.Add("Verfied", false);
                    throw ex;
                }

                List<byte> payLoad = packet.GetRange(3, payLoadSize);
                if (Mode == XModemMode.Checksum) {
                    byte simpleChecksum = (byte)(payLoad.Sum(n => n));
                    if (simpleChecksum != packet[131]) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verfied", false);
                        throw ex;
                    }
                }
                else {
                    if (CheckSumValidator.ApproveMessage(packet.GetRange(3, packet.Count - 3)) == false) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verfied", false);
                        throw ex;
                    }
                }
                Packets.Add(packet);
                Data.AddRange(payLoad);
                _packetIndexToReceive++;
            }
            catch(XModemProtocolException ex) {
                packetVerifed = (bool) ex.Data["Verified"];
            } 

            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(_packetIndexToReceive, packet, packetVerifed));
            return packetVerifed;
        }

        /// <summary>
        /// Send ACK.
        /// </summary>
        private void SendACK() {
            Port.Write(ACK);
            ResetConsecutiveNAKs();
        }

        /// <summary>
        /// Send NAK, and increment consecutive NAK.
        /// </summary>
        private bool SendNAK() {
            Port.Write(NAK);
            return IncrementConsecutiveNAKs();
        }
    }
}