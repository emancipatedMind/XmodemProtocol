using System.Linq;

namespace XModemProtocol.Operations.Initialize {
    using System;
    using Communication;
    using Exceptions;
    public class InitializeSend : Initializer {

        private byte _latestResponse;

        protected override void InitializeTimeoutTimer() {
            int timeoutTime = _requirements.Options.SenderInitializationTimeout;
            if (timeoutTime < 1) return;
            _timer = new System.Timers.Timer(timeoutTime);
            _timer.Elapsed += (s, e) => {
                _timer.Stop();
                _waitHandle.Set();
            };
            _timer.Start();
        }

        protected override void UpdateState() {
            _requirements.Context.State = XModemStates.SenderAwaitingInitializationFromReceiver;
        }

        protected override void Reset() {
            _waitHandle = new System.Threading.ManualResetEvent(false);
        }

        protected override void Initialize() {
            AwaitInitializationFromReceiver();
        }

        private void AwaitInitializationFromReceiver() {
            while (NoTimeoutNorCancellation) {
                if (_requirements.Communicator.ReadBufferIsEmpty) continue;
                GetLatestResponse();
                if (CwasReceived) {
                    if (ChecksumIsForced) continue;
                }
                else if (NAKwasReceived) {
                    if (SenderIsCRC) {
                        _requirements.Context.Mode = XModemMode.Checksum;
                    }
                }
                else continue;
                _timer?.Stop();
                return;
            }
        }

        private bool NoTimeoutNorCancellation {
            get {
                if (_waitHandle.WaitOne(0))
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.InitializationFailed));
                if ( _requirements.Context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        private void GetLatestResponse() => _latestResponse = _requirements.Communicator.ReadAllBytes().Last();

        private bool CwasReceived => _latestResponse == _requirements.Options.C;

        private bool ChecksumIsForced => _requirements.Context.Mode == XModemMode.Checksum;

        private bool NAKwasReceived => _latestResponse == _requirements.Options.NAK;

        private bool SenderIsCRC => _requirements.Context.Mode != XModemMode.Checksum;

    }
}