namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Detectors;
    using Validators.Packet;
    public class XModemTool : IXModemTools {

        public IPacketBuilder Builder { get; set; }
        public IPacketValidator Validator { get; set; }

        public XModemTool() { }
    }
}