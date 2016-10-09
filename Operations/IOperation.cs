namespace XModemProtocol.Operations {
    using EventData;
    using Options;
    public interface IOperation {
        void Go(IRequirements requirements);
        event System.EventHandler<PacketToSendEventArgs> PacketToSend;
        event System.EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}