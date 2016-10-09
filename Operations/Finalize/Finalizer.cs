namespace XModemProtocol.Operations.Finalize {
    using Options;
    public abstract class Finalizer : IFinalizer {
        protected IRequirements _requirements;

        public void Finalize(IRequirements requirements) {
            _requirements = requirements;
            _requirements.Context.State = XModemStates.PendingCompletion;
            FinalizeOp();
        }

        protected abstract void FinalizeOp();

    }
}