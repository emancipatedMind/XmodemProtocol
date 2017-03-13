namespace XModemProtocol.Operations {
    using Environment;
    using Operations.Finalize;
    using Operations.Initialize;
    using Operations.Invoke;
    using EventData;
    public abstract class Operation : IOperation {

        #region Fields
        protected IInvoker _invoker;
        protected IInitializer _initializer;
        protected IFinalizer _finalizer;
        protected IContext _context;
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
        public void Go(IContext context) {
            _context = context;
            _initializer.Initialize(_context);
            _invoker.Invoke(_context);
            _finalizer.Finalize(_context);
        }
        #endregion
    }
}