using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {
        private void Send() {

            bool canDetected;
            bool handled = true;
            bool firstPass = true;
            _sendOperationWaitHandle.Set();

            while (_sendOperationWaitHandle.WaitOne(0)) {
                handled = true;

                if (_cancellationWaitHandle.WaitOne(0)) {
                    Abort(true, new AbortedEventArgs(XModemAbortReason.UserCancelled));
                    _sendOperationWaitHandle.Reset();
                    break;
                }

                try {
                    if (firstPass) {
                        while (Port.BytesToRead == 0) { if (!_sendOperationWaitHandle.WaitOne(0)) throw new XModemProtocolException(""); }
                        _tempBuffer.AddRange(Encoding.ASCII.GetBytes(Port.ReadExisting()).ToList());
                        firstPass = false;
                    }
                    else {
                        _tempBuffer.AddRange(new List<byte> { (byte)Port.ReadByte() });
                    }
                }
                catch (Exception ex) when (ex is TimeoutException || ex is XModemProtocolException) {
                    Abort(true, new AbortedEventArgs(XModemAbortReason.Timeout));
                    break;
                }

                int tempBufferCount = _tempBuffer.Count;

                if (tempBufferCount == 0) continue;

                canDetected = _tempBuffer.Contains(CAN);
                switch(State) {
                    case XModemStates.SenderAwaitingInitialization:
                        _initializationTimeOut.Stop();
                        if ( DetectCancellation(_tempBuffer)) {
                            Abort(false, new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                            break;
                        }
                        if (canDetected) {
                            handled = false;
                            break;
                        }

                        if (_tempBuffer.Contains(C)) {
                            if (Mode == XModemMode.Checksum)
                                break;
                        }
                        else if (_tempBuffer.Last() == NAK) {
                            if (Mode != XModemMode.Checksum) {
                                Mode = XModemMode.Checksum;
                                BuildPackets();
                            }
                            State = XModemStates.SenderPacketSent;
                            SendPacket();
                        }
                        else 
                            _initializationTimeOut.Start();

                        break;
                    case XModemStates.SenderPacketSent:
                        if(tempBufferCount == 1) {
                            if (_tempBuffer[0] == ACK) {
                                _packetIndexToSend++;
                                ResetConsecutiveNAKs();
                            }
                            else {
                                if (IncrementConsecutiveNAKs() == true) {
                                    Abort(true, new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                    break;
                                }
                            }
                            SendPacket();
                        }
                        else {
                            if (DetectCancellation(_tempBuffer)) {
                                Abort(false, new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                                break;
                            }
                            if (canDetected) handled = false;
                        }
                        break;
                    case XModemStates.SenderAwaitingFinalACK:
                        if(tempBufferCount == 1) {
                            if (_tempBuffer[0] == ACK) {
                                Completed?.Invoke(this, new CompletedEventArgs());
                                Reset();
                                _sendOperationWaitHandle.Reset();
                                break;
                            }
                            else {
                                if (IncrementConsecutiveNAKs() == true) {
                                    Abort(true, new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
                                    break;
                                }
                                SendPacket();
                            }
                        }
                        else {
                            if (DetectCancellation(_tempBuffer)) {
                                Abort(false, new AbortedEventArgs(XModemAbortReason.CancelRequestReceived));
                                break;
                            }
                            if (canDetected) handled = false;
                        }
                        break;
                }

                if (handled) _tempBuffer = new List<byte>();
            }
        }
    }
}