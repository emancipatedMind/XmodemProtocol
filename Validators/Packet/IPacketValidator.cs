using System.Collections.Generic;

namespace XModemProtocol.Validators.Packet {
    using Options;
    public interface IPacketValidator {
        void Reset();
        bool ValidatePacket(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}