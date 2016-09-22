using XModemProtocol.Builders;
using XModemProtocol.Validators.Packet;

namespace XModemProtocol.Factories.Tools
{
    public class XModemOneKTools : XModemTool {

        public XModemOneKTools() {
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