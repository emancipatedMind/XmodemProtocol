using System.Collections.Generic;

namespace XModemProtocol.Detectors {
    using Options;
    public interface ICancellationDetector {
        bool CancellationDetected(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}