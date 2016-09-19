using System.Collections.Generic;
using XModemProtocol.Calculators;
using XModemProtocol.Options;

namespace XModemProtocol.Builders {
    using Options;
    public interface IPacketBuilder {
        List<List<byte>> GetPackets(IEnumerable<byte> input, IXModemProtocolOptions options);
    }
}