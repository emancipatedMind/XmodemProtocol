namespace XModemProtocol.Operations.Initialize {
    using Exceptions;
    using System.Linq;
    public class InitializeSend : Initializer {

        private byte _latestResponse;

        protected override void InitializeTimeoutTimer() {
            int timeoutTime = _context.Options.SenderInitializationTimeout;
            if (timeoutTime < 1) return;
            _timer = new System.Timers.Timer(timeoutTime);
            _timer.Elapsed += (s, e) => {
                _timer.Stop();
                _waitHandle.Set();
            };
            _timer.Start();
        }

        protected override void UpdateState() {
            _context.State = XModemStates.SenderAwaitingInitializationFromReceiver;
        }

        protected override void Reset() {
            _waitHandle = new System.Threading.ManualResetEvent(false);
        }

        protected override void Initialize() {
            AwaitInitializationFromReceiver();
        }

        private void AwaitInitializationFromReceiver() {
            while (NoTimeoutNorCancellation) {
                if (ReadBufferIsEmpty) continue;
                GetLatestResponse();
                if (CwasReceived) {
                    if (ChecksumIsForced) continue;
                }
                else if (NAKwasReceived) {
                    if (SenderIsCRC) {
                        _context.Mode = XModemMode.Checksum;
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
                if (_context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        private bool ReadBufferIsEmpty => _context.Communicator.BytesInReadBuffer == 0;

        private void GetLatestResponse() => _latestResponse = _context.Communicator.ReadAllBytes().Last();

        private bool CwasReceived => _latestResponse == _context.Options.C;

        private bool ChecksumIsForced => _context.Mode == XModemMode.Checksum;

        private bool NAKwasReceived => _latestResponse == _context.Options.NAK;

        private bool SenderIsCRC => _context.Mode != XModemMode.Checksum;

    }
}