namespace XModemProtocol.Operations.Initialize {
    using Exceptions;
    using EventData;
    public class InitializeReceive : Initializer {

        private int _initializationBytesSent;
        private int _numOfCsSent;

        protected override void InitializeTimeoutTimer() {
            int timeoutTime = _requirements.Options.ReceiverInitializationTimeout;
            _timer = new System.Timers.Timer(timeoutTime);
            _timer.Elapsed += (s, e) => {
                _timer.Stop();
                _waitHandle.Set();
            };
        }

        protected override void UpdateState() {
            _requirements.Context.State = XModemStates.ReceiverSendingInitializationByte;
        }

        protected override void Reset() {
            _waitHandle = new System.Threading.ManualResetEvent(true);
            if (ModeIsNotChecksum) _requirements.Context.Mode = XModemMode.OneK;
            _initializationBytesSent = 0;
            _numOfCsSent = 0;
        }

        protected override void Initialize() {
            while (Running) {
                if (ReadBufferContainsAtLeastOnePacketOfData) return;
                if (_waitHandle.WaitOne(0)) {
                    _waitHandle.Reset();
                    if (ModeIsNotChecksum) {
                        if (CRCInitializationHasFailed) {
                            _requirements.Context.Mode = XModemMode.Checksum;
                        }
                    }
                    if (InitializationHasFailed) {
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                    }
                    _requirements.Communicator.Flush();
                    SendInitializationByte();
                    _timer.Start();
                }
            }
        }

        private bool Running {
            get {
                if ( _requirements.Context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        private bool ReadBufferContainsAtLeastOnePacketOfData {
            get {
                int bytesThreshold = _requirements.Context.Mode == XModemMode.Checksum ? 132 : 133;
                return _requirements.Communicator.BytesInReadBuffer >= bytesThreshold; 
            }
        }

        private bool ModeIsChecksum => _requirements.Context.Mode == XModemMode.Checksum;
        private bool ModeIsNotChecksum => _requirements.Context.Mode != XModemMode.Checksum;

        private bool CRCInitializationHasFailed =>
            ++_numOfCsSent > _requirements.Options.ReceiverMaxNumberOfInitializationBytesForCRC;

        private bool InitializationHasFailed =>
            ++_initializationBytesSent > _requirements.Options.ReceiverMaxNumberOfInitializationBytesInTotal;

        private void SendInitializationByte() {
            _requirements.Communicator.Write(InitializationByte);
        }

        private byte InitializationByte =>
            ModeIsChecksum ? _requirements.Options.NAK : _requirements.Options.C;

    }
}