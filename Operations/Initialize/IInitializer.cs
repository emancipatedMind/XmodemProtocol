using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Operations.Initialize {
    using Communication;
    using Options;
    public interface IInitializer {
        void Initialize(IRequirements info);
    }
}