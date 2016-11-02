using System.Collections.Generic;

namespace XModemProtocol.Builders {
    public interface IPacketBuilder {
        List<List<byte>> GetPackets(IEnumerable<byte> input, Options.IXModemProtocolOptions options);
    }
}