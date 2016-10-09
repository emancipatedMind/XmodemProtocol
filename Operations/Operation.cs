namespace XModemProtocol.Operations {
    using Factories;
    using Factories.Tools;
    using Options;
    using Operations.Finalize;
    using Operations.Initialize;
    using Operations.Invoke;
    using EventData;
    public abstract class Operation : IOperation {

        protected IInvoker _invoker;
        protected IInitializer _initializer;
        protected IFinalizer _finalizer;
        protected ISendReceiveRequirements _requirements;
        protected IToolFactory _toolFactory = new XModemToolFactory();
        protected IXModemTools _tools;
        protected XModemMode _mode;

        public event System.EventHandler<PacketToSendEventArgs> PacketToSend;
        public event System.EventHandler<PacketReceivedEventArgs> PacketReceived;

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