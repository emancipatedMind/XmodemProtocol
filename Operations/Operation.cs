using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Options;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Operations.Invoke;
using XModemProtocol.EventData;

namespace XModemProtocol.Operations {
    public abstract class Operation : IOperation {

        protected IInvoker _invoker;
        protected IInitializer _initializer;
        protected ISendReceiveRequirements _requirements;
        protected IToolFactory _toolFactory = new XModemToolFactory();
        protected IXModemTools _tools;
        protected XModemMode _mode;

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Go(IRequirements requirements) {
            _tools = _toolFactory.GetToolsFor(requirements.Options.Mode);
            _requirements = new SendReceiveRequirements {
                Communicator = requirements.Communicator,
                Context = requirements.Context,
                Detector = _tools.Detector,
                Options = requirements.Options,
            };
            _mode = _requirements.Options.Mode;
            Go();
        }

        protected abstract void Go();

        protected bool ModeChangedInInitialization => _requirements.Options.Mode != _mode;

        protected void FirePacketToSendEvent(object sender, PacketToSendEventArgs args) {
            PacketToSend?.Invoke(sender, args);
        }
        protected void FirePacketReceivedEvent(object sender, PacketReceivedEventArgs args) {
            PacketReceived?.Invoke(sender, args);
        }
    }
}