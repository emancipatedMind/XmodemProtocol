using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Detectors;
using XModemProtocol.Validators.Packet;

namespace XModemProtocol.Options {
    public interface ISendReceiveRequirements : IRequirements {
        ICancellationDetector Detector { get; }
        IPacketValidator Validator { get; }
    }
}