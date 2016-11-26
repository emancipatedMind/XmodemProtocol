namespace XModemProtocol.Operations.Initialize {
    using EventData;
    using Exceptions;
    public class InitializeReceive : Initializer {

        private int _initializationBytesSent;
        private int _numOfCsSent;

        protected override void InitializeTimeoutTimer() {
            int timeoutTime = _context.Options.ReceiverInitializationTimeout;
            _timer = new System.Timers.Timer(timeoutTime);
            _timer.Elapsed += (s, e) => {
                _timer.Stop();
                _waitHandle.Set();
            };
        }

        protected override void UpdateState() {
            _context.State = XModemStates.ReceiverSendingInitializationByte;
        }

        protected override void Reset() {
            _waitHandle = new System.Threading.ManualResetEvent(true);
            if (ModeIsNotChecksum) _context.Mode = XModemMode.OneK;
            _initializationBytesSent = 0;
            _numOfCsSent = 0;
        }

        protected override void Initialize() {
            while (Running) {
                if (_waitHandle.WaitOne(0)) {
                    _waitHandle.Reset();
                    if (ModeIsNotChecksum) {
                        if (CRCInitializationHasFailed) {
                            _context.Mode = XModemMode.Checksum;
                        }
                    }
                    if (InitializationHasFailed) {
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                    }
                    _context.Communicator.Flush();
                    SendInitializationByte();
                    _timer.Start();
                }
                else if (ReadBufferContainsAtLeastOnePacketOfData) return;
            }
        }

        private bool Running {
            get {
                if ( _context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        private bool ReadBufferContainsAtLeastOnePacketOfData {
            get {
                int bytesThreshold = _context.Mode == XModemMode.Checksum ? 132 : 133;
                return _context.Communicator.BytesInReadBuffer >= bytesThreshold; 
            }
        }

        private bool ModeIsChecksum => _context.Mode == XModemMode.Checksum;
        private bool ModeIsNotChecksum => _context.Mode != XModemMode.Checksum;

        private bool CRCInitializationHasFailed =>
            ++_numOfCsSent > _context.Options.ReceiverMaxNumberOfInitializationBytesForCRC;

        private bool InitializationHasFailed =>
            ++_initializationBytesSent > _context.Options.ReceiverMaxNumberOfInitializationBytesInTotal;

        private void SendInitializationByte() {
            _context.Communicator.Write(InitializationByte);
        }

        private byte InitializationByte =>
            ModeIsChecksum ? _context.Options.NAK : _context.Options.C;

    }
}