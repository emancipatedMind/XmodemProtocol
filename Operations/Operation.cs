using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Options;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Operations.Invoke;

namespace XModemProtocol.Operations {
    public abstract class Operation : IOperation {

        protected IInvoker _invoker;
        protected IInitializer _initializer;
        protected ISendReceiveRequirements _requirements;
        protected IToolFactory _toolFactory = new XModemToolFactory();
        protected IXModemTools _tools;

        public void Go(IRequirements requirements) {
            _tools = _toolFactory.GetToolsFor(requirements.Options.Mode);
            _requirements = new SendReceiveRequirements {
                Communicator = requirements.Communicator,
                Context = requirements.Context,
                Detector = _tools.Detector,
                Options = requirements.Options,
                Validator = _tools.Validator,
            };
            Go();
        }

        protected abstract void Go();

    }
}