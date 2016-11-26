namespace XModemProtocol.Calculators {
    using System.Collections.Generic;
    public interface IChecksumCalculator {
        IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input);
    }
}