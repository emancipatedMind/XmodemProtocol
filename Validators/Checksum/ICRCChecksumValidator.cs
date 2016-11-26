namespace XModemProtocol.Validators.Checksum {
    using System.Collections.Generic;

    public interface ICRCChecksumValidator : IValidateChecksum {
        IEnumerable<byte> ChecksumReference { get; set; }
    }
}