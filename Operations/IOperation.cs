using System;
using XModemProtocol.EventData;
using XModemProtocol.Options;

namespace XModemProtocol.Operations {
    public interface IOperation {
        void Go(IRequirements requirements);
        event EventHandler<PacketToSendEventArgs> PacketToSend;
        event EventHandler<PacketReceivedEventArgs> PacketReceived;
    }
}