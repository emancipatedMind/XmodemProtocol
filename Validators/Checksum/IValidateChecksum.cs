using System.Collections.Generic;

namespace XModemProtocol.Validators.Checksum {
    public interface IValidateChecksum {
        bool ValidateChecksum(IEnumerable<byte> input);
    }
}