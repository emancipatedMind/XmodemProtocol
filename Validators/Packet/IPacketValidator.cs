
namespace XModemProtocol.Validators.Packet {
    using Options;
    using System.Collections.Generic;

    public interface IPacketValidator {
        void Reset();
        ValidationResult ValidatePacket(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}