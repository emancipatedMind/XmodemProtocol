namespace XModemProtocol.Validators.Checksum {
    using System.Collections.Generic;

    public interface IValidateChecksum {
        bool ValidateChecksum(IEnumerable<byte> input);
    }
}