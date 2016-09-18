using System.Collections.Generic;

namespace XModemProtocol.Validators.Packet {
    public interface IPacketValidator {
        void Reset();
        bool ValidatePacket(IEnumerable<byte> input);
    }
}