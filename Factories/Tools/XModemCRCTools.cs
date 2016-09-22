using XModemProtocol.Builders;
using XModemProtocol.Validators.Packet;

namespace XModemProtocol.Factories.Tools
{
    public class XModemCRCTools : XModemTool {

        public XModemCRCTools() {
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
