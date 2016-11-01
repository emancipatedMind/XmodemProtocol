namespace XModemProtocol.Operations {
    using Factories.Tools;
    using Options;
    using Operations.Finalize;
    using Operations.Initialize;
    using Operations.Invoke;
    using EventData;
    public abstract class Operation : IOperation {

        #region Fields
        protected IInvoker _invoker;
        protected IInitializer _initializer;
        protected IFinalizer _finalizer;
        protected IRequirements _requirements;
        protected IXModemTools _tools;
        protected XModemMode _mode;
        #endregion

        #region Events
        public event System.EventHandler<PacketToSendEventArgs> PacketToSend;
        public event System.EventHandler<PacketReceivedEventArgs> PacketReceived;
        protected void FirePacketToSendEvent(object sender, PacketToSendEventArgs args) {
            PacketToSend?.Invoke(sender, args);
        }
        protected void FirePacketReceivedEvent(object sender, PacketReceivedEventArgs args) {
            PacketReceived?.Invoke(sender, args);
        }
        #endregion

        #region Methods
        public void Go(IRequirements requirements) {
            _requirements = requirements;
            _tools = _requirements.ToolFactory.GetToolsFor(requirements.Context.Mode);
            _mode = _requirements.Context.Mode;
            _initializer.Initialize(_requirements);
            if (ModeChangedInInitialization) {
                _tools = _requirements.ToolFactory.GetToolsFor(requirements.Context.Mode);
                TransitionToInvoke();
            }
            _invoker.Invoke(_requirements);
            _finalizer.Finalize(_requirements);
        }
        #endregion

        #region Support Methods
        protected virtual void TransitionToInvoke() { }
        private bool ModeChangedInInitialization => _requirements.Context.Mode != _mode;
        #endregion
    }
}