using System.Collections.Generic;

namespace XModemProtocol.Calculators {
    public interface ICRCChecksumCalculator : IChecksumCalculator {
        IEnumerable<byte> InitialCRCValue { get; set; }
    }
}