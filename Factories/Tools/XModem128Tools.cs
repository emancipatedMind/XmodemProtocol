using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Validators.Packet;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocol.Factories.Tools
{
    public class XModem128Tools : XModemTool {

        public XModem128Tools() {
            _calculator = new NormalChecksumCalculator();
            _normalChecksumValidator = new NormalChecksumValidator(_calculator);
            _validator = new PacketValidator(_normalChecksumValidator);
            _builder = new NormalPacketBuilder(_calculator);
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
