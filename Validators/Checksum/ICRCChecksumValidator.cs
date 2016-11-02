using System.Collections.Generic;

namespace XModemProtocol.Validators.Checksum {
    public interface ICRCChecksumValidator : IValidateChecksum {
        IEnumerable<byte> ChecksumReference { get; set; }
    }
}