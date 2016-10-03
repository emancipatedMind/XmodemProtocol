using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Exceptions;
using XModemProtocol.EventData;
using XModemProtocol.Validators.Packet;

namespace XModemProtocol.Operations.Invoke {
    public class InvokeReceive : Invoker {

        System.Timers.Timer _watchDogTimer;
        System.Threading.ManualResetEvent _watchDogWaitHandle = new System.Threading.ManualResetEvent(false);
        bool _checkShouldOccur = false;
        int _numbOfBytesToShave = 0;
        int _packetNumber = 0;
        int _consecutiveNAKs = 0;
        List<byte> _packet;
        ValidationResult _result;

        protected override void Invoke() {
            _requirements.Context.State = XModemStates.ReceiverReceivingPackets;
            _numbOfBytesToShave = _requirements.Context.Mode == XModemMode.Checksum ? 4 : 5;
            _checkShouldOccur = _requirements.Options.ReceiverConsecutiveNAKsRequiredForCancellation > 0;
            _requirements.Validator.Reset();
            ConfigureTimers();
            StartWatchDog();
            GetPackets();
        }

        private void ConfigureTimers() {
            _watchDogTimer = new System.Timers.Timer(_requirements.Options.ReceiverTimeoutDuringPacketReception);
            _watchDogTimer.Elapsed += (s, e) => {
                _watchDogWaitHandle.Set();
            };
        }

        private void GetPackets() {
            while (NotCancelled) {
                if (_requirements.Communicator.ReadBufferContainsData) {
                    _buffer.AddRange(_requirements.Communicator.ReadAllBytes());
                }
                else if (_watchDogWaitHandle.WaitOne(0)) {
                    SendNAK();
                    Reset();
                    ResetWatchDog();
                    continue;
                }
                else continue;

                PauseAndResetWatchDog();
                CheckForCancellation();

                if (EOTwasReceived) {
                    SendACK();
                    return;
                }

                if (BufferLengthIsTooShort) continue;

                if (BufferContainsValidPacket) {
                    SendACK();
                    if (PacketIsNotDuplicate) {
                        ExtractDataFromBuffer();
                        FirePacketReceivedEvent();
                    }
                }
                else SendNAK();

                Reset();
            }
        }

        private bool BufferLengthIsTooShort {
            get {
                if (_buffer.Count < 2) return true;
                else if (_buffer[0] == _requirements.Options.SOH) {
                    return _buffer.Count < 132;
                }
                else if (_buffer[0] == _requirements.Options.STX) {
                    return _buffer.Count < 1029;
                }
                else return true;
            }
        }

        private void ExtractDataFromBuffer() {
            _packet = _buffer.GetRange(3, _buffer.Count - _numbOfBytesToShave);
            _requirements.Context.Data.AddRange(_packet);
            _requirements.Context.Packets.Add(_buffer);
        }

        private void StartWatchDog() {
            _watchDogTimer.Start();
        }

        private void PauseAndResetWatchDog() {
            _watchDogTimer.Stop();
            ResetWatchDog();
        }

        private void ResetWatchDog() => _watchDogWaitHandle.Reset();

        private bool BufferContainsValidPacket  {
            get {
                _result = _requirements.Validator.ValidatePacket(_buffer, _requirements.Options);
                return _result != ValidationResult.Invalid;
            }
        }

        private bool PacketIsNotDuplicate =>
            _result != ValidationResult.Duplicate;

        private void SendACK() {
            _requirements.Communicator.Write(_requirements.Options.ACK);
            _consecutiveNAKs = 0;
        }

        private bool EOTwasReceived => _buffer[0] == _requirements.Options.EOT;
        
        private void SendNAK() {
            if (_checkShouldOccur && ConsecutiveNAKsAboveLimit)
                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
            _requirements.Communicator.Write(_requirements.Options.NAK);
        }

        private bool ConsecutiveNAKsAboveLimit => ++_consecutiveNAKs >= _requirements.Options.ReceiverConsecutiveNAKsRequiredForCancellation;

        protected void FirePacketReceivedEvent() {
            base.FirePacketReceivedEvent(++_packetNumber, _packet);
        }
    }
}