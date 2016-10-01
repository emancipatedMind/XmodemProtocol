using System.Collections.Generic;

namespace XModemProtocol.Validators.Packet {
    using Options;
    public interface IPacketValidator {
        void Reset();
        ValidationResult ValidatePacket(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}