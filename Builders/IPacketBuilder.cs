namespace XModemProtocol.Builders {
    using System.Collections.Generic;
    public interface IPacketBuilder {
        List<List<byte>> GetPackets(IEnumerable<byte> input, Options.IXModemProtocolOptions options);
    }
}