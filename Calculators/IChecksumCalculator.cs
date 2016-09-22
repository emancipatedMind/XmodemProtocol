using System.Collections.Generic;

namespace XModemProtocol.Calculators {
    using Options;
    public interface IChecksumCalculator {
        IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input);
    }
}