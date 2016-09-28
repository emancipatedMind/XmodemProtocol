using System.Linq;
using System.Timers;

namespace XModemProtocol.Operations.Initialize {
    using Communication;
    using Exceptions;
    public class InitializeSend : Initializer {

        byte _latestResponse;

        protected override void InitializeTimeoutTimer() {
            int timeoutTime = _requirements.Options.SenderInitializationTimeout;
            if (timeoutTime < 1) return;
            _timer = new Timer(timeoutTime);
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e) {
            _timer.Stop();
            _waitHandle.Set();
        }

        protected override void Initialize() {
            _requirements.Context.State = XModemStates.SenderAwaitingInitializationFromReceiver;
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
                        _requirements.Options.Mode = XModemMode.Checksum;
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

        private bool ChecksumIsForced => _requirements.Options.Mode == XModemMode.Checksum;

        private bool NAKwasReceived => _latestResponse == _requirements.Options.NAK;

        private bool SenderIsCRC => _requirements.Options.Mode != XModemMode.Checksum;

    }
}