using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Options;
using XModemProtocol.Communication;
using XModemProtocol.EventData;

namespace XModemProtocol.Operations.Invoke {
    public abstract class Invoker : IInvoker {
        protected IRequirements _requirements;
        protected List<byte> _buffer = new List<byte>();
        protected IXModemTools _tools;

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Invoke(IRequirements requirements) {
            _requirements = requirements;
            _tools = _requirements.ToolFactory.GetToolsFor(_requirements.Options.Mode);
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