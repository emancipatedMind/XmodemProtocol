using System;
using System.Collections.Generic;
using System.Linq;

namespace XModemProtocol {
    public partial class XModemCommunicator {
        /// <summary>
        /// Receiver Method.
        /// Initialize session as receiver.
        /// </summary>
        /// <param name="options">The options to be used this session.</param>
        public void Receive(XModemProtocolOptions options = null) {
            // If state is not idle, then return. 
            // If it is, change it to Initializing, change Role, and reset instance.
            if (State != XModemStates.Idle) return;
            State = XModemStates.Initializing;
            Role = XModemRole.Receiver;
            Reset();

            if (options != null) {
                // Get a clone of the optiosn passed in. This is a deep copy that so that it cannot be changed by outside world.
                XModemProtocolOptions localOptions =  (XModemProtocolOptions)options.Clone();

                // Set mode.
                Mode = localOptions.Mode;

                // Set the common options.
                SetCommonOptions(localOptions);

                // Get the timeout between Packet Reception.
                ReceiverTimeoutDuringPacketReception = localOptions.ReceiverTimeoutDuringPacketReception;

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

                ReceiverInitializationTimeout = localOptions.ReceiverInitializationTimeout;
            }
            else {
                // This must be re-assigned to itself in case an upgrade from CRC to OneK is needed.
                Mode = Mode;
            }

            // This object is used to control how often the initialization byte is sent to receiver.
            // Whenever initializationWaitHandle is set, the receiver will send an initialization byte, 
            // will start this timer, and reset initializationWaitHandle. Once this timer elapses, it will set
            // _initializationWaitHandle, and process will repeat.
            _initializationTimeOut = new System.Timers.Timer(ReceiverInitializationTimeout);
            _initializationTimeOut.Elapsed += (s, e) => {
                _initializationTimeOut.Stop();
                _initializationWaitHandle.Set();
            };
            _initializationWaitHandle.Set();

            // Reset these variables
            _data = new List<byte>();
            Packets = new List<List<byte>>();

            // Invoke OperationPending event. If exception is thrown, the operation is aborted, and the 
            // exception is rethrown. 
            try {
                if (OperationPending != null) {
                    bool? performOperation = OperationPending?.Invoke();
                    if (performOperation.HasValue == true && performOperation.Value == false) {
                        Abort(new AbortedEventArgs(XModemAbortReason.CancelledByOperationPendingEvent), false);
                        return;
                    }
                }
            }
            catch {
                Abort(new AbortedEventArgs(XModemAbortReason.CancelledByOperationPendingEvent), false);
                throw;
            }

            // Flush port, change state, and perform operation.
            Port.Flush();
            State = XModemStates.ReceiverSendingInitializationByte;
            ReceiveOperation();
        }


        /// <summary>
        /// Receiver Method. Method that performs receive operation.
        /// </summary>
        private void ReceiveOperation() {

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
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
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
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Timeout), true);
                                }
                                receiverWatchDogWaitHandle.Reset();
                                continue;
                            }

                            // Nothing to do, continue from top.
                            else continue;
                            
                            // Stop watchDogTimer, and reset waitHandle.
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
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
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
                                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded), true);
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
                                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded), true);
                            }

                            // Restart watchDogTimer, and go to top of loop.
                            receiverWatchDog.Start();
                            continue;
                        // If operation cancelled, break out of infinite loop with an exception.
                        case XModemStates.Cancelled:
                            throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.Cancelled), true);
                    }
                }
            }

            // Only Exception caught here is XModemProtocolException.
            catch(XModemProtocolException ex) {
                // If AbortArgs was provided with value, means that this is an abort.
                if (ex.AbortArgs != null) {
                    Abort(ex.AbortArgs, ex.SendCancel);
                }
                // If not, operation completed successfully.
                else {
                    // Remove SUB bytes from data.
                    for (int i = _data.Count - 1; i > -1; i--) {
                        if (_data[i] == SUB)
                            _data.RemoveAt(i);
                        else break;
                    }
                    CompleteOperation();
                }
            }
            // All others will bubble to top after abort.
            catch (Exception) {
                Abort(new AbortedEventArgs(XModemAbortReason.OperationFailed), false);
                throw;
            }
        }

        /// <summary>
        /// Receiver Method. Method used to validate whether packet is a valid packet or not.
        /// </summary>
        /// <param name="buffer">Packet passed in.</param>
        /// <param name="payLoadSize">Size of payload. Normally 128 or 1024.</param>
        /// <returns>Returns bool indicating whether packet was validated or not.</returns>
        private bool ValidatePacket(IEnumerable<byte> buffer, int payLoadSize) {
            List<byte> packet = buffer.ToList();
            bool packetVerifed = true;

            // Figure out how much of packet is the information we are looking for.
            payLoadSize -= Mode == XModemMode.Checksum ? 4 : 5;
            try {
                // Check to see if packet number is next in sequence.
                // If it isn't, see if it is the previous packet.
                // If so, pass validation.
                // If it isn't packet expected, or a resend of last packet,
                // fail validation. 
                if (packet[1] != ((byte)_packetIndex)) {
                    if (packet[1] != ((byte)_packetIndex - 1)) {
                        throw new XModemProtocolException();
                    }
                    throw new XModemProtocolException(packetVerified:true);
                }

                // Check to make sure inverse byte matches as well.
                byte inversePacketNumber = (byte)(0xFF - _packetIndex);
                if (packet[2] != inversePacketNumber) {
                    throw new XModemProtocolException();
                }

                // Extract payload.
                List<byte> payLoad = packet.GetRange(3, payLoadSize);

                // Here is where we validate the contents using either
                // CRC or checksum depending on the mode.
                if (Mode == XModemMode.Checksum) {
                    if ((CheckSum(payLoad))[0] != packet[131]) {
                        throw new XModemProtocolException();
                    }
                }
                else {
                    if (!CheckSum(packet.GetRange(3, packet.Count - 3)).SequenceEqual(CRC16LTE.Zeros)) {
                        throw new XModemProtocolException();
                    }
                }
                // Validation has passed at this point. Add payload to Data, and packet to List of Packets.
                Packets.Add(packet);
                _data.AddRange(payLoad);
            }
            catch(XModemProtocolException ex) {
                packetVerifed = ex.PacketVerified;
            } 

            // Fire event signaling packet has been received. This fires whether packet was validated or not.
            // Only time it does not fire is if packet was repeat.
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(_packetIndex, packet, packetVerifed));
            // Increment packetIndex, and return validation status.
            if (packetVerifed ) _packetIndex++;
            return packetVerifed;
        }

        /// <summary>
        /// Receiver Method. Send ACK.
        /// </summary>
        private void SendACK() {
            Port.Write(ACK);
            _consecutiveNAKs = 0;
        }

        /// <summary>
        /// Receiver Method. Increment Consecutive NAKS sent. If below limit, send NAK.
        /// </summary>
        /// <returns>Returns true if ReceiverConsecutiveNAKBytesRequiredForCancellation has been exceeded.</returns>
        private bool SendNAK() {
            if (++_consecutiveNAKs > ReceiverConsecutiveNAKBytesRequiredForCancellation) return true;
            Port.Write(NAK);
            return false;
        }
    }
}