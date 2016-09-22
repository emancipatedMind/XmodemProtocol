using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Options {
    using Factories;
    public class Requirements : IRequirements {

        public IContext Context { get; private set; }

        public IXModemProtocolOptions Options { get; private set; }

        public IToolFactory Tools { get; private set; }

        public Requirements(IContext context, IXModemProtocolOptions options, IToolFactory tools) {
            Context = context;
            Options = options;
            Tools = tools;
        }

    }
}