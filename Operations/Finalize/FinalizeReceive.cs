namespace XModemProtocol.Operations.Finalize {
    public class FinalizeReceive : Finalizer {
        protected override void FinalizeOp() {
            if (_context.Data.Count < 1) return; 
            for (int i = _context.Data.Count - 1; _context.Data[i] == _context.Options.SUB; i--)
                _context.Data.RemoveAt(i); 
        }
    }
}