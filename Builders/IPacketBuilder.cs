using System.Collections.Generic;
using XModemProtocol.Calculators;
using XModemProtocol.Options;

namespace XModemProtocol.Builders {
    public interface IPacketBuilder {
        List<List<byte>> GetPackets(IEnumerable<byte> input);
    }
}