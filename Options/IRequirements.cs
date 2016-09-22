using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Options {
    using Factories;
    public interface IRequirements {
        IXModemProtocolOptions Options { get; }
        IContext Context { get; }
        IToolFactory Tools { get; }
    }
}