using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Detectors;
    using Validators.Packet;
    public interface IXModemTools {
        IPacketBuilder Builder { get; }
        IPacketValidator Validator { get; }
        ICancellationDetector Detector { get; }
    }
}
