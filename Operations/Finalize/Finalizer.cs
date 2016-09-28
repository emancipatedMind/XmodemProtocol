using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Options;

namespace XModemProtocol.Operations.Finalize {
    public abstract class Finalizer : IFinalizer {
        IRequirements _requirements;

        public void Finalize(IRequirements requirements) {
            _requirements = requirements;
            _requirements.Context.State = XModemStates.PendingCompletion;
            FinalizeOp();
        }

        protected abstract void FinalizeOp();

    }
}