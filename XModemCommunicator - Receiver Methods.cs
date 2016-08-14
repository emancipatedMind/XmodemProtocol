using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        public void InitializeReceiver(XModemProtocolReceiverOptions options) {
            if (State != XModemStates.Idle) { return; }
            State = XModemStates.Initializing;
            Reset();

            XModemProtocolReceiverOptions localOptions = (XModemProtocolReceiverOptions)options?.Clone() ?? new XModemProtocolReceiverOptions();
            if (localOptions.Mode != Mode) 
                Mode = localOptions.Mode;

            if (localOptions.InitializationTimeout < 1000) {
                localOptions.InitializationTimeout = 1000;
                options.InitializationTimeout = 1000;
            }
            else if (localOptions.InitializationTimeout > 10000) {
                localOptions.InitializationTimeout = 10000;
                options.InitializationTimeout = 10000;
            }

            _initializationTimeOut = new System.Timers.Timer(localOptions.InitializationTimeout);

            try {
                OperationPending?.Invoke();
            }
            catch {
                Abort(new AbortedEventArgs(XModemAbortReason.InitializationFailed));
                throw;
            }

            Port.Flush();
            State = XModemStates.ReceiverSendingInitializationByte;
            Task.Run(() => Receive());
        }

        private void Receive() {
            try { }
            catch(XModemProtocolException ex) { }
        }

        private void SendACK() {
            Port.Write(ACK);
            ResetConsecutiveNAKs();
        }

        private bool SendNAK() {
            Port.Write(NAK);
            return IncrementConsecutiveNAKs();

        }
    }
}