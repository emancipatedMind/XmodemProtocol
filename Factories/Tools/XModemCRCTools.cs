namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Calculators;
    using Validators.Packet;
    using Validators.Checksum;
    public class XModemCRCTools : XModemTool {

        public XModemCRCTools() {
            _cRCCalculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
            _crcChecksumValidator = new CRCChecksumValidator(_cRCCalculator);
            _validator = new PacketValidator(_crcChecksumValidator);
            _builder = new CRCPacketBuilder(_cRCCalculator);
        }

        public override IPacketBuilder Builder => _builder;

        public override IPacketValidator Validator => _validator;
    }
}