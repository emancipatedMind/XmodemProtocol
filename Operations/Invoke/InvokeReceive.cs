using System.Collections.Generic;

namespace XModemProtocol.Operations.Invoke {
    using Exceptions;
    using EventData;
    using Validators.Packet;
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
            _context.State = XModemStates.ReceiverReceivingPackets;
            _numbOfBytesToShave = _context.Mode == XModemMode.Checksum ? 4 : 5;
            _checkShouldOccur = _context.Options.ReceiverConsecutiveNAKsRequiredForCancellation > 0;
            _context.Tools.Validator.Reset();
            ConfigureTimers();
            StartWatchDog();
            GetPackets();
        }

        private void ConfigureTimers() {
            _watchDogTimer = new System.Timers.Timer(_context.Options.ReceiverTimeoutDuringPacketReception);
            _watchDogTimer.Elapsed += (s, e) => {
                _watchDogWaitHandle.Set();
            };
        }

        private void GetPackets() {
            while (NotCancelled) {
                if (ReadBufferContainsData) {
                    _buffer.AddRange(_context.Communicator.ReadAllBytes());
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

        private bool ReadBufferContainsData => _context.Communicator.BytesInReadBuffer != 0;

        private bool BufferLengthIsTooShort {
            get {
                if (_buffer.Count < 2) return true;
                else if (_buffer[0] == _context.Options.SOH) {
                    return _buffer.Count < 132;
                }
                else if (_buffer[0] == _context.Options.STX) {
                    return _buffer.Count < 1029;
                }
                else return true;
            }
        }

        private void ExtractDataFromBuffer() {
            _packet = _buffer.GetRange(3, _buffer.Count - _numbOfBytesToShave);
            _context.Data.AddRange(_packet);
            _context.Packets.Add(_buffer);
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
                _result = _context.Tools.Validator.ValidatePacket(_buffer, _context.Options);
                return _result != ValidationResult.Invalid;
            }
        }

        private bool PacketIsNotDuplicate =>
            _result != ValidationResult.Duplicate;

        private void SendACK() {
            _context.Communicator.Write(_context.Options.ACK);
            _consecutiveNAKs = 0;
        }

        private bool EOTwasReceived => _buffer[0] == _context.Options.EOT;
        
        private void SendNAK() {
            if (_checkShouldOccur && ConsecutiveNAKsAboveLimit)
                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.ConsecutiveNAKLimitExceeded));
            _context.Communicator.Write(_context.Options.NAK);
        }

        private bool ConsecutiveNAKsAboveLimit => ++_consecutiveNAKs >= _context.Options.ReceiverConsecutiveNAKsRequiredForCancellation;

        protected void FirePacketReceivedEvent() {
            base.FirePacketReceivedEvent(++_packetNumber, _packet);
        }
    }
}