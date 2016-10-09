namespace XModemProtocol.Options {
    using Detectors;
    using Validators.Packet;
    public interface ISendReceiveRequirements : IRequirements {
        ICancellationDetector Detector { get; }
        IPacketValidator Validator { get; }
    }
}