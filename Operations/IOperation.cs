using System;

namespace XModemProtocol.Operations {
    using EventData;
    public interface IOperation {
        void Go(Options.IContext context);
        event EventHandler<PacketToSendEventArgs> PacketToSend;
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}