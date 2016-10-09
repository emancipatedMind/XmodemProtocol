using System.Threading;

namespace XModemProtocol.Operations.Initialize {
    using Options;
    public abstract class Initializer : IInitializer {

        protected IRequirements _requirements;
        protected System.Timers.Timer _timer;
        protected ManualResetEvent _waitHandle;

        protected abstract void InitializeTimeoutTimer();
        protected abstract void Initialize();
        protected abstract void UpdateState();
        protected abstract void Reset();

        public void Initialize(IRequirements requirements) {
            _requirements = requirements;
            Reset();
            InitializeTimeoutTimer();
            UpdateState();
            Initialize();
        }
    }
}