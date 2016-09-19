using System.Collections.Generic;

namespace XModemProtocol.Validators.Checksum {
    using Options;
    public interface IValidateChecksum {
        bool ValidateChecksum(IEnumerable<byte> input);
    }
}