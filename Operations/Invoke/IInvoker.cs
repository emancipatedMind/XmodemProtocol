using System;

namespace XModemProtocol.Operations.Invoke {
    using EventData;
    public interface IInvoker {
        void Invoke(Options.IContext context);
        event EventHandler<PacketToSendEventArgs> PacketToSend;
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}