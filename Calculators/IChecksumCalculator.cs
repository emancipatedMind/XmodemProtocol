using System.Collections.Generic;

namespace XModemProtocol.Calculators {
    public interface IChecksumCalculator {
        IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input);
    }
}