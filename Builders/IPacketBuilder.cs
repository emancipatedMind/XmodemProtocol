using System.Collections.Generic;
namespace XModemProtocol.Builders {
    using Options;
    public interface IPacketBuilder {
        List<List<byte>> GetPackets(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}