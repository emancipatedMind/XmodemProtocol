using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Validators.Packet;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocol.Factories.Tools
{
    public class XModemOneKTools : XModemTool {

        public XModemOneKTools() {
            _cRCCalculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
            _crcChecksumValidator = new CRCChecksumValidator(_cRCCalculator);
            _validator = new PacketValidator(_crcChecksumValidator);
            _builder = new OneKPacketBuilder(_cRCCalculator);
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