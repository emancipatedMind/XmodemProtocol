using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.EventData;

namespace XModemProtocol.Operations.Invoke {
    using Options;
    public interface IInvoker {
        void Invoke(ISendReceiveRequirements requirements);
        event EventHandler<PacketToSendEventArgs> PacketToSend;
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}