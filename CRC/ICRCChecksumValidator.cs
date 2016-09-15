using System.Collections.Generic;

namespace XModemProtocol.CRC {
    public interface ICRCChecksumValidator : IChecksumCalculator, ICRCPolynomial, ICRCInitialValue {
        IEnumerable<byte> ChecksumReference { get; set; }
        bool ValidateChecksum(IEnumerable<byte> input);
    }
}