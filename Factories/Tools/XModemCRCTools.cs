using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Builders;
using XModemProtocol.Detectors;
using XModemProtocol.Validators.Packet;

namespace XModemProtocol.Factories.Tools {
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
