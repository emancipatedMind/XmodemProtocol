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
            InitializeReceiver(new XModemProtocolOptions()); 
        }
        /// <summary>
        /// Receiver Method.
        /// Initialize session as receiver.
        /// </summary>
        /// <param name="options">The options to be used this session.</param>
        public void InitializeReceiver(XModemProtocolOptions options) {
            // If state is not idle, then return. 
            // If it is, change it to Initializing, and reset instance.
            if (State != XModemStates.Idle) return;
            State = XModemStates.Initializing;
            Role = XModemRole.Receiver;
            Reset();

            // SenderFilename is set to null. This property is only used by sender.
            SenderFilename = null;

            // Get a clone of the optiosn passed in. This is a deep copy that so that it cannot be changed by outside world.
            XModemProtocolOptions localOptions =  (XModemProtocolOptions)options.Clone();

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
            if (localOptions.ReceiverMaxNumberOfInitializationBytesForCRC > localOptions.ReceiverMaxNumberOfInitializationBytesInTotal) { 
                ReceiverMaxNumberOfInitializationBytesInTotal = ReceiverMaxNumberOfInitializationBytesForCRC = localOptions.ReceiverMaxNumberOfInitializationBytesForCRC;
                options.ReceiverMaxNumberOfInitializationBytesInTotal = ReceiverMaxNumberOfInitializationBytesForCRC;
            }
            else {
                ReceiverMaxNumberOfInitializationBytesForCRC = localOptions.ReceiverMaxNumberOfInitializationBytesForCRC;
                ReceiverMaxNumberOfInitializationBytesInTotal = localOptions.ReceiverMaxNumberOfInitializationBytesInTotal;
            }

            // This object is used to control how often the initialization byte is sent to receiver.
            // Whenever initializationWaitHandle is set, the receiver will send an initialization byte, 
            // will start this timer, and reset initializationWaitHandle. Once this timer elapses, it will set
            // _initializationWaitHandle, and process will repeat.
            _initializationTimeOut = new System.Timers.Timer(localOptions.ReceiverInitializationTimeout);
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
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed), false);
                throw;
            }

            // Flush port, change state, and perform operation.
            Port.Flush();
            State = XModemStates.ReceiverSendingInitializationByte;
            Task.Run(() => Receive());
        }


        private void Receive() {

            _tempBuffer = new List<byte>();

            // The receiver must send an initialization byte in order to prompt sender to start sending packets.
            // Depending on the mode, a C or NAK will be sent.
            byte initializationByte = Mode == XModemMode.Checksum ? NAK : C;

            // These objects are used as a timeout while packets are being sent. If, after a receiver responds to a packet,
            // the sender does not respond with another packet, the receiver will NAK the sender until in case response
            // was lost to prompt another send.
            System.Threading.ManualResetEvent receiverWatchDogWaitHandle = new System.Threading.ManualResetEvent(false); 
            System.Timers.Timer receiverWatchDog = new System.Timers.Timer(ReceiverTimeoutDuringPacketReception);
            receiverWatchDog.Elapsed += (s, e) => {
                receiverWatchDogWaitHandle.Set();
            };

            try {

                XModemProtocolException exc;

                // Operation loop. Only way out is an exception.
                while(true) {

                    switch(State) {
                        case XModemStates.ReceiverSendingInitializationByte:
                            // Has sender sent any bytes to be parsed? If so, change state, disable
                            // _initializationTimeOut, and restart at top of loop.
                            if (Port.BytesToRead != 0) {
                                State = XModemStates.ReceiverReceivingPackets;
                                _initializationTimeOut.Stop();
                            }

                            // If not, we have to see if it is time to reprompt the sender with another
                            // initialization byte.
                            else if (_initializationWaitHandle.WaitOne(0)) {

                                // If we're sending C's, we have to see if the number of C's has exceeded
                                // limit before a regression to normal XModem is required. This is accompanied
                                // by changing the initialization byte to NAK.
                                if (initializationByte == C) {
                                    if (++_countOfCsSent > ReceiverMaxNumberOfInitializationBytesForCRC) {
                                        Mode = XModemMode.Checksum;
                                        initializationByte = NAK;
                                    }
                                }

                                // We also must check if we have exceeded the number of initialization bytes in general.
                                if(++_numOfInitializationBytesSent > ReceiverMaxNumberOfInitializationBytesInTotal) {
                                    exc = new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                                    exc.Data.Add("SendCancel", false);
                                    throw exc;
                                }

                                // Send initialization byte, and reset waitHandle, and restart _initializationTimeOut.
                                Port.Write(initializationByte);
                                _initializationWaitHandle.Reset();
                                _initializationTimeOut.Start();
                            }

                            // This state does not need to clear _tempBuffer, so start from top.
                            continue;
                        case XModemStates.ReceiverReceivingPackets:

                            // Check if there are any bytes on port to read.
                            if (Port.BytesToRead != 0) {
                                _tempBuffer.AddRange(Port.ReadAllBytes());
                            }

                            // Check if there are bytes left over from previous iteration to examine.
                            else if (_tempBuffer.Count != 0) { }

                            // No bytes to examine, so see if watchDog has elapsed. If so, NAK sender, reset waitHandle, 
                            // and start from top.
                            else if (receiverWatchDogWaitHandle.WaitOne(0)) {
                                if (SendNAK() == true) {
                                    exc = new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout));
                                    exc.Data.Add("SendCancel", true);
                                    throw exc;
                                }
                                receiverWatchDogWaitHandle.Reset();
                                continue;
                            }

                            // Nothing to do, continue from top.
                            else continue;
                            
                            // Bytes to parse, stop watchDogTimer, and reset waitHandle.
                            receiverWatchDog.Stop();
                            receiverWatchDogWaitHandle.Reset();

                            int packetSize;
                            List<byte> packet;

                            // If header byte is EOT, then transmission is over. Send ACK, and exit operation loop.
                            if (_tempBuffer[0] == EOT) {
                                SendACK();
                                State = XModemStates.PendingCompletion;
                                throw new XModemProtocolException(null);
                            }

                            // See if Sender has sent cancellation.
                            if (DetectCancellation(_tempBuffer)) {
                                State = XModemStates.Cancelled;
                                exc = new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
                                exc.Data.Add("SendCancel", false);
                                throw exc;
                            } 

                            // If bytes available are less than the smallest packet accepted, continue from top restarting watchDogTimer.
                            if (_tempBuffer.Count < 132) {
                                receiverWatchDog.Start();
                                continue;
                            }

                            // Determine the full packet size depending on first the Mode.
                            // If the Mode is Checksum, only 132 bytes are accepted.
                            // If not, CRC is in use, and we must examine header byte to see length of 
                            // full packet. 
                            if (Mode == XModemMode.Checksum)
                                packetSize = 132;
                            else if (_tempBuffer[0] == SOH) {
                                packetSize = 133;
                            }
                            else if (_tempBuffer[0] == STX) {
                                packetSize = 1029;
                            }
                            // If we have made it this far, response was not understood. Flush port, clear _tempBuffer, NAK sender,
                            // and restart watchDogTimer.
                            else {
                                Port.Flush();
                                if(SendNAK() == true) {
                                    exc = new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                    exc.Data.Add("SendCancel", true);
                                    throw exc;
                                }
                                receiverWatchDog.Start();
                                _tempBuffer = new List<byte>();
                                continue;
                            }

                            // If full packet has not been received yet, let's continue from top possibly reading in the rest of the packet.
                            if (_tempBuffer.Count < packetSize) {
                                receiverWatchDog.Start();
                                continue;
                            }

                            // Remove the bytes from _tempBuffer, and place them in holding variable to pass to 
                            // method to validate packet.
                            packet = new List<byte>(_tempBuffer.GetRange(0, packetSize));
                            _tempBuffer.RemoveRange(0, packetSize);

                            // If packet checks out, ACK sender.
                            if(ValidatePacket(packet, packetSize) == true) {
                                SendACK();
                            }
                            // If packet validation fails, NAK sender.
                            else if(SendNAK() == true) {
                                exc = new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                exc.Data.Add("SendCancel", true);
                                throw exc;
                            }

                            // Restart watchDogTimer, and go to top of loop.
                            receiverWatchDog.Start();
                            continue;
                        // If operation cancelled, break out of infinite loop with an exception.
                        case XModemStates.Cancelled:
                            exc = new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Cancelled));
                            exc.Data.Add("SendCancel", true);
                            throw exc;
                    }

                }
            }

            // Only Exception caught here is XModemProtocolException. All others will bubble to top.
            catch(XModemProtocolException ex) {
                // If AbortArgs was provided with value, means that this is an abort.
                if (ex.AbortArgs != null)
                    Abort(ex.AbortArgs, (bool)ex.Data["SendCancel"]);
                // If not, operation completed successfully.
                else {
                    // Remove SUB bytes from data.
                    for (int i = Data.Count - 1; i > -1; i--)
                    {
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

            // Figure out how much of packet is the information we are looking for.
            payLoadSize -= Mode == XModemMode.Checksum ? 4 : 5;
            try {
                XModemProtocolException ex;

                // Check to see if packet number is next in sequence.
                // If it isn't, see if it is the previous packet.
                // If so, pass validation.
                // If it isn't packet expected, or a resend of last packet
                // fail validation. 
                if (packet[1] != ((byte)_packetIndex)) {
                    if (packet[1] != ((byte)_packetIndex - 1)) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verified", false);
                        throw ex;
                    }
                    ex = new XModemProtocolException();
                    ex.Data.Add("Verified", true);
                    throw ex;
                }

                // Check to make sure inverse byte matches as well.
                byte inversePacketNumber = (byte)(0xFF - _packetIndex);
                if (packet[2] != inversePacketNumber) {
                    ex = new XModemProtocolException();
                    ex.Data.Add("Verified", false);
                    throw ex;
                }

                // Extract payload.
                List<byte> payLoad = packet.GetRange(3, payLoadSize);

                // Here is where we validate the contents using either
                // CRC or checksum depending on the mode.
                if (Mode == XModemMode.Checksum) {
                    byte simpleChecksum = (CheckSum(payLoad))[0];
                    if (simpleChecksum != packet[131]) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verified", false);
                        throw ex;
                    }
                }
                else {
                    if (!CheckSum(packet.GetRange(3, packet.Count - 3)).SequenceEqual(CRC16LTE.Zeros)) {
                        ex = new XModemProtocolException();
                        ex.Data.Add("Verified", false);
                        throw ex;
                    }
                }
                // Validation has passed at this point. Add payload to Data, and packet to List of Packets.
                Packets.Add(packet);
                Data.AddRange(payLoad);
            }
            catch(XModemProtocolException ex) {
                packetVerifed = (bool) ex.Data["Verified"];
            } 

            // Fire event signaling packet has been received. This fires whether packet was validated or not.
            // Only time it does not fire is if packet was repeat.
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(_packetIndex, packet, packetVerifed));
            // Increment packetIndex, and return validation status.
            if (packetVerifed ) _packetIndex++;
            return packetVerifed;
        }

        /// <summary>
        /// Send ACK.
        /// </summary>
        private void SendACK() {
            Port.Write(ACK);
            _consecutiveNAKs = 0;
        }

        /// <summary>
        /// Increment Consecutive NAKS sent. If below limit, send NAK.
        /// </summary>
        private bool SendNAK() {
            if (++_consecutiveNAKs > ReceiverConsecutiveNAKBytesRequiredForCancellation) return true;
            Port.Write(NAK);
            return false;
        }
    }
}