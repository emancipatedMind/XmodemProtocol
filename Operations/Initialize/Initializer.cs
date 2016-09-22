using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace XModemProtocol.Operations.Initialize {
    using Communication;
    using Options;
    // using Exceptions;
    public abstract class Initializer : IInitializer {

        protected ICommunicator _communicator;
        protected IRequirements _requirements;
        protected System.Timers.Timer _timer;
        protected ManualResetEvent _waitHandle = new ManualResetEvent(false);

        protected abstract void InitializeTimeoutTimer();
        protected abstract void Initialize();

        public Initializer(ICommunicator communicator) {
            _communicator = communicator; 
        }

        public void Initialize(IRequirements requirements) {
            if (requirements == null) throw new XModemProtocolException("Requirements cannot be null.");
            _requirements = requirements;
            _requirements.Context.State = XModemStates.Initializing;
            InitializeTimeoutTimer();
            Initialize();
        }
    }
}