using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TimerNS = System.Timers;

namespace XModemProtocol.Operations.Initialize {
    using Communication;
    public class SendInitializer : Initializer {

        byte _latestReponse;

        public SendInitializer(ICommunicator communicator) : base(communicator) { }

        protected override void InitializeTimeoutTimer() {
            int timeoutTime = _requirements.Options.SenderInitializationTimeout;
            if (timeoutTime < 1) return;
            _timer = new TimerNS.Timer(timeoutTime);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, TimerNS.ElapsedEventArgs e) {
            _waitHandle.Reset();
        }

        protected override void Initialize() {
            while (NoTimeoutNorCancellation()) {
                if (_communicator.IsReadBufferEmpty) continue;
                List<byte> response = _communicator.ReadAllBytes();
                byte _latestReponse = response.Last();
                if (CwasReceived()) {
                    if (ChecksumIsForced()) continue;
                }
                else if (NAKwasReceived()) {
                    if (SenderIsCRC()) {
                        _requirements.Options.Mode = XModemMode.Checksum;
                        _requirements.Context.BuildRequested = true;
                    }
                }
                else continue;
                return;
            }
            throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Timeout));
        }

        #warning Need to finish method.
        private bool NoTimeoutNorCancellation() => _waitHandle.WaitOne(0);

        private bool CwasReceived() => _latestReponse == _requirements.Options.C;

        private bool ChecksumIsForced() => _requirements.Options.Mode == XModemMode.Checksum;

        private bool NAKwasReceived() => _latestReponse == _requirements.Options.NAK;

        private bool SenderIsCRC() => _requirements.Options.Mode != XModemMode.Checksum;

    }
}