using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Detectors {
    using Options;
    public interface ICancellationDetector {
        bool CancellationDetected(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}