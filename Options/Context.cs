using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Options {
    public class Context : IContext {

        public XModemStates State { get; set; } = XModemStates.Idle;
        public bool BuildRequested { get; set; } = false;

    }
}