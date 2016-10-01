using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.EventData;
using XModemProtocol.Options;

namespace XModemProtocol.Operations {
    public class ReceiveOperation : Operation {

        public ReceiveOperation() {
            _initializer = new Initialize.InitializeReceive();
            _invoker = new Invoke.InvokeReceive();
            _invoker.PacketReceived += FirePacketReceivedEvent;
            _finalizer = new Finalize.FinalizeReceive();
        }

        protected override void TransitionToInvoke() {
            _requirements = new SendReceiveRequirements {
                Communicator = _requirements.Communicator,
                Context = _requirements.Context,
                Detector = _tools.Detector,
                Options = _requirements.Options,
                Validator = _tools.Validator,
            };
        }
    }
}