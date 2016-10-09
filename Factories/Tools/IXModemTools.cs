namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Detectors;
    using Validators.Packet;
    public interface IXModemTools {
        IPacketBuilder Builder { get; }
        IPacketValidator Validator { get; }
        ICancellationDetector Detector { get; }
    }
}
