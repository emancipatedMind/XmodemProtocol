namespace XModemProtocol.Operations.Finalize {
    using Environment;
    public abstract class Finalizer : IFinalizer {

        protected IContext _context;

        public void Finalize(IContext context) {
            _context = context;
            _context.State = XModemStates.PendingCompletion;
            FinalizeOp();
        }

        protected virtual void FinalizeOp() { }

    }
}