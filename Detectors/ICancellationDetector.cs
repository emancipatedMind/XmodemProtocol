namespace XModemProtocol.Detectors {
    using Options;
    using System.Collections.Generic;
    public interface ICancellationDetector {
        bool CancellationDetected(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}