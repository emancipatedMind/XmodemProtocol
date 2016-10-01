using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Options;

namespace XModemProtocol.Operations.Finalize {
    public class FinalizeReceive : Finalizer {
        protected override void FinalizeOp() {
            for (int i = _requirements.Context.Data.Count - 1; _requirements.Context.Data[i] == _requirements.Options.SUB; i--)
                _requirements.Context.Data.RemoveAt(i); 
        }
    }
}