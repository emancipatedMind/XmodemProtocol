namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Calculators;
    using Validators.Packet;
    using Validators.Checksum;
    public class XModemOneKTools : XModemTool {

        public XModemOneKTools() {
            _cRCCalculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
            _crcChecksumValidator = new CRCChecksumValidator(_cRCCalculator);
            _validator = new PacketValidator(_crcChecksumValidator);
            _builder = new OneKPacketBuilder(_cRCCalculator);
        }

        public override IPacketBuilder Builder => _builder;

        public override IPacketValidator Validator => _validator;
    }
}