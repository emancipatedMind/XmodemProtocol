using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Detectors;
using XModemProtocol.Validators.Packet;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocol.Factories.Tools
{
    public abstract class XModemTool : IXModemTools {

        protected IPacketValidator _validator;
        protected IPacketBuilder _builder;
        protected ICancellationDetector _detector;

        protected ISummationChecksumCalculator _calculator;
        protected ICRCChecksumCalculator _cRCCalculator;

        protected IChecksumValidator _normalChecksumValidator;
        protected ICRCChecksumValidator _crcChecksumValidator;

        public abstract IPacketBuilder Builder { get; }
        public abstract IPacketValidator Validator { get; }

        public XModemTool() {
            _detector = new CancellationDetector();
        }

        public ICancellationDetector Detector {
            get {
                return _detector;
            }
        }
    }
}