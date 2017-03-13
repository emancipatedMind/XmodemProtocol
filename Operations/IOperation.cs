namespace XModemProtocol.Operations {
    using EventData;
    using System;
    public interface IOperation {
        void Go(XModemProtocol.Environment.IContext context);
        event EventHandler<PacketToSendEventArgs> PacketToSend;
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}