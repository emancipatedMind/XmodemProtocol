using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Operations {
    public class SendOperation : Operation {

        public SendOperation() {
            _initializer = new Initialize.InitializeSend();
            _invoker = new Invoke.InvokeSend();
        }

        protected override void Go() {
            _initializer.Initialize(_requirements);
            if (_requirements.Context.BuildRequested == true) {
                _tools = _toolFactory.GetToolsFor(_requirements.Options.Mode);
                _requirements.Context.Packets = _tools.Builder.GetPackets(_requirements.Context.Data, _requirements.Options);
            }
            _invoker.Invoke(_requirements);
        }

    }
}