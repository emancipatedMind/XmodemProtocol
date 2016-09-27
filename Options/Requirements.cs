using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XModemProtocol.Options {
    using Factories;
    using Communication;
    public class Requirements : IRequirements {

        public IContext Context { get; set; }

        public IXModemProtocolOptions Options { get; set; }

        public ICommunicator Communicator { get; set; }

    }
}