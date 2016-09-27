using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Validators.Packet;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocol.Factories.Tools
{
    public class XModemCRCTools : XModemTool {

        public XModemCRCTools() {
            _cRCCalculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
            _crcChecksumValidator = new CRCChecksumValidator(_cRCCalculator);
            _validator = new PacketValidator(_crcChecksumValidator);
            _builder = new CRCPacketBuilder(_cRCCalculator);
        }

        public override IPacketBuilder Builder {
            get {
                return _builder;
            }
        }

        public override IPacketValidator Validator {
            get {
                return _validator;
            }
        }
    }
}
