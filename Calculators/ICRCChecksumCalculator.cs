namespace XModemProtocol.Calculators {
    using System.Collections.Generic;
    public interface ICRCChecksumCalculator : IChecksumCalculator {
        IEnumerable<byte> InitialCRCValue { get; set; }
    }
}