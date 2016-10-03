using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Options;
using XModemProtocol.Operations.Finalize;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Operations.Invoke;
using XModemProtocol.EventData;

namespace XModemProtocol.Operations {
    public abstract class Operation : IOperation {

        protected IInvoker _invoker;
        protected IInitializer _initializer;
        protected IFinalizer _finalizer;
        protected ISendReceiveRequirements _requirements;
        protected IToolFactory _toolFactory = new XModemToolFactory();
        protected IXModemTools _tools;
        protected XModemMode _mode;

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Go(IRequirements requirements) {
            _tools = _toolFactory.GetToolsFor(requirements.Context.Mode);
            _requirements = new SendReceiveRequirements {
                Detector = _tools.Detector,
                Communicator = requirements.Communicator,
                Context = requirements.Context,
                Options = requirements.Options,
                Validator = _tools.Validator,
            };
            _mode = _requirements.Context.Mode;
            _initializer.Initialize(_requirements);
            if (ModeChangedInInitialization) {
                _tools = _toolFactory.GetToolsFor(requirements.Context.Mode);
                TransitionToInvoke();
            }
            _invoker.Invoke(_requirements);
            _finalizer.Finalize(_requirements);
        }

        protected abstract void TransitionToInvoke();

        private bool ModeChangedInInitialization => _requirements.Context.Mode != _mode;

        protected void FirePacketToSendEvent(object sender, PacketToSendEventArgs args) {
            PacketToSend?.Invoke(sender, args);
        }
        protected void FirePacketReceivedEvent(object sender, PacketReceivedEventArgs args) {
            PacketReceived?.Invoke(sender, args);
        }
    }
}