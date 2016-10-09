namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Calculators;
    using Validators.Packet;
    using Validators.Checksum;
    public class XModem128Tools : XModemTool {

        public XModem128Tools() {
            _calculator = new NormalChecksumCalculator();
            _normalChecksumValidator = new NormalChecksumValidator(_calculator);
            _validator = new PacketValidator(_normalChecksumValidator);
            _builder = new NormalPacketBuilder(_calculator);
        }

        public override IPacketBuilder Builder => _builder;

        public override IPacketValidator Validator => _validator;
    }
}