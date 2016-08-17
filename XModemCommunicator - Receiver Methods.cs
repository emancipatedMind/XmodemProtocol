using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Receiver Method.
        /// Initialize session as receiver.
        /// </summary>
        public void InitializeReceiver() {
            InitializeReceiver(new XModemProtocolReceiverOptions()); 
        }
        /// <summary>
        /// Receiver Method.
        /// Initialize session as receiver.
        /// </summary>
        /// <param name="options">The options to be used this session.</param>
        public void InitializeReceiver(XModemProtocolReceiverOptions options) {
            // If state is not idle, then return. 
            // If it is, change it to Initializing, and reset instance.
            if (State != XModemStates.Idle) return;
            State = XModemStates.Initializing;
            Reset();

            // Get a clone of the optiosn passed in. This is a deep copy that so that it cannot be changed by outside world.
            XModemProtocolReceiverOptions localOptions = (XModemProtocolReceiverOptions)options?.Clone() ?? new XModemProtocolReceiverOptions();

            // If user has asked for CRC, upgrade to OneK as it is completely compatible with CRC.
            if (localOptions.Mode == XModemMode.CRC)
                Mode = XModemMode.OneK;
            // Other than that, heed user wishes.
            else
                Mode = localOptions.Mode;

            // Set the common options.
            SetCommonOptions(localOptions);

            // Get the timeout between Packet Reception.
            ReceiverTimeoutDuringPacketReception = localOptions.ReceiverTimeoutForPacketReception;

            // Check to make that MaxNumberOfInitializationBytesForCRC is less than MaxNumberOfInitializationBytesInTotal.
            // If it is, assign MaxNumberOfInitializationBytesForCRC to both of the variables, and update these variables in object 
            // passed in.
            // If this is not the case, assign as per user settings.
            if (localOptions.MaxNumberOfInitializationBytesForCRC > localOptions.MaxNumberOfInitializationBytesInTotal) { 
                MaxNumberOfInitializationBytesInTotal = MaxNumberOfInitializationBytesForCRC = localOptions.MaxNumberOfInitializationBytesForCRC;
                options.MaxNumberOfInitializationBytesInTotal = MaxNumberOfInitializationBytesForCRC;
            }
            else {
                MaxNumberOfInitializationBytesForCRC = localOptions.MaxNumberOfInitializationBytesForCRC;
                MaxNumberOfInitializationBytesInTotal = localOptions.MaxNumberOfInitializationBytesInTotal;
            }

            // This object is used to control how often the initialization byte is sent to receiver.
            // Whenever initializationWaitHandle is set, the receiver will send an initialization byte
            // through, will start this timer, and reset initializationWaitHandle. Once this timer elapses, it will set
            // _initializationWaitHandle, and process will repeat.
            _initializationTimeOut = new System.Timers.Timer(localOptions.InitializationTimeout);
            _initializationTimeOut.Elapsed += (s, e) => {
                _initializationTimeOut.Stop();
                _initializationWaitHandle.Set();
            };
            _initializationWaitHandle.Set();

            // Reset these variables
            Data = new List<byte>();
            Packets = new List<List<byte>>();

            // Invoke OperationPending event. If exception is thrown, the operation is aborted, and the 
            // exception is rethrown. 
            try {
                OperationPending?.Invoke();
            }
            catch {
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                throw;
            }

            // Flush port, change state, and perform operation.
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
                                _tempBuffer.AddRange(Port.ReadAllBytes());
                            }
                            else if (_tempBuffer.Count != 0) { }
                            else if (receiverTimeoutWaitHandle.WaitOne(0)) {
                                if (SendNAK() == true)
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout));
                                receiverTimeoutWaitHandle.Reset();
                                continue;
                            }
                            else continue;
                            
                            receiverWatchDog.Stop();
                            receiverTimeoutWaitHandle.Reset();

                            int packetSize;
                            List<byte> packet;

                            if (_tempBuffer[0] == EOT) {
                                SendACK();
                                throw new XModemProtocolException(null);
                            }

                            if (DetectCancellation(_tempBuffer)) {
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                            } 

                            if (_tempBuffer.Count < 132) {
                                receiverWatchDog.Start();
                                continue;
                            }

                            if (Mode == XModemMode.Checksum)
                                packetSize = 132;
                            else if (_tempBuffer[0] == SOH) {
                                packetSize = 133;
                            }
                            else if (_tempBuffer[0] == STX) {
                                packetSize = 1029;
                            }
                            else {
                                Port.Flush();
                                if(SendNAK() == true) 
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                receiverWatchDog.Start();
                                break;
                            }

                            if (_tempBuffer.Count < packetSize) {
                                receiverWatchDog.Start();
                                continue;
                            }

                            packet = new List<byte>(_tempBuffer.GetRange(0, packetSize));
                            _tempBuffer.RemoveRange(0, packetSize);

                            if(ValidatePacket(packet, packetSize) == true) {
                                SendACK();
                            }
                            else if(SendNAK() == true) {
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                            }

                            receiverWatchDog.Start();
                            if (_tempBuffer.Count != 0) continue;
                            break;
                    }

                    _tempBuffer = new List<byte>();
                }
            }

            // Only Exception caught here is XModemProtocolException. All others will bubble to top.
            catch(XModemProtocolException ex) {
                // If AbortArgs was provided with value, means that this is an abort.
                if (ex.AbortArgs != null) 
                    Abort(ex.AbortArgs);
                // If not, operation completed successfully.
                else {
                    // Remove SUB bytes from data.
                    for (int i = Data.Count - 1; i > -1; i--) {
                        if (Data[i] == SUB)
                            Data.RemoveAt(i);
                        else break;
                    }
                    CompleteOperation();
                }
            }
        }

        private bool ValidatePacket(IEnumerable<byte> buffer, int payLoadSize) {
            List<byte> packet = buffer.ToList();
            bool packetVerifed = true;
            payLoadSize -= Mode == XModemMode.Checksum ? 4 : 5;
            try {
                Exception ex;
                packet = buffer.ToList();
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
            }
            catch(XModemProtocolException ex) {
                packetVerifed = (bool) ex.Data["Verified"];
            } 

            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(_packetIndexToReceive, packet, packetVerifed));
            if (packetVerifed ) _packetIndexToReceive++;
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