namespace XModemProtocol.Factories.Tools {
    public interface IXModemTools {
        Builders.IPacketBuilder Builder { get; }
        Validators.Packet.IPacketValidator Validator { get; }
    }
}