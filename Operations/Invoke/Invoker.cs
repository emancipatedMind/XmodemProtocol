using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Detectors;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Options;
using XModemProtocol.Communication;
using XModemProtocol.EventData;

namespace XModemProtocol.Operations.Invoke {
    public abstract class Invoker : IInvoker {
        protected ISendReceiveRequirements _requirements;
        protected List<byte> _buffer = new List<byte>();

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Invoke(ISendReceiveRequirements requirements) {
            _requirements = requirements;
            Invoke();
        }

        protected abstract void Invoke();

        protected virtual void FirePacketToSendEvent (int packetNumber, List<byte> packet) {
            PacketToSend?.Invoke(this, new PacketToSendEventArgs(packetNumber, packet)); 
        }

        protected virtual void FirePacketReceivedEvent (int packetNumber, List<byte> packet, bool packetVerified) {
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packetNumber, packet, packetVerified)); 
        }

    }
}