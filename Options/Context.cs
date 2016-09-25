using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XModemProtocol.Options {
    public class Context : IContext {

        public XModemStates State { get; set; } = XModemStates.Idle;
        public bool BuildRequested { get; set; } = false;
        public List<List<byte>> Packets { get; set; } = null;
        public CancellationToken Token { get; set; }

    }
}