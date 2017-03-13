namespace XModemProtocol.Operations.Initialize {
    using Environment;
    using System.Threading;
    public abstract class Initializer : IInitializer {

        protected IContext _context;
        protected System.Timers.Timer _timer;
        protected ManualResetEvent _waitHandle;

        protected abstract void InitializeTimeoutTimer();
        protected abstract void Initialize();
        protected abstract void UpdateState();
        protected abstract void Reset();

        public void Initialize(IContext context) {
            _context = context;
            Reset();
            InitializeTimeoutTimer();
            UpdateState();
            Initialize();
        }
    }
}