namespace XModemProtocol.Options {
    using Communication;
    using Detectors;
    using Validators.Packet;
    public class SendReceiveRequirements : ISendReceiveRequirements {
        public ICommunicator Communicator { get; set; }
        public IContext Context { get; set; }
        public ICancellationDetector Detector { get; set; }
        public IXModemProtocolOptions Options { get; set; }
        public IPacketValidator Validator { get; set; }
    }
}