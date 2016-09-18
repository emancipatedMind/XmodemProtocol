using System.Collections.Generic;

namespace XModemProtocol.Validators.Checksum {
    using Calculators;
    public interface ICRCChecksumValidator : IValidateChecksum {
        IEnumerable<byte> ChecksumReference { get; set; }
    }
}