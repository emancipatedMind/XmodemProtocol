namespace XModemProtocol.Factories.Tools {
    using Builders;
    using Calculators;
    using Detectors;
    using Validators.Packet;
    using Validators.Checksum;
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

        public ICancellationDetector Detector => _detector;
    }
}