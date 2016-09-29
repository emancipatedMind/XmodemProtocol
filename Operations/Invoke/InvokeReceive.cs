using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Exceptions;

namespace XModemProtocol.Operations.Invoke {
    public class InvokeReceive : Invoker {

        protected override void Invoke() {
            _requirements.Context.State = XModemStates.ReceiverReceivingPackets;
            GetPackets();
        }

        private void GetPackets() {
            while (NotCancelled) {

            }
        } 

        private bool NotCancelled {
            get {
                if ( _requirements.Context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

    }
}