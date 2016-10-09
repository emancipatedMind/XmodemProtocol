namespace XModemProtocol.Builders {
    using Calculators;
    public class NormalPacketBuilder : PacketBuilder {

        public NormalPacketBuilder(ISummationChecksumCalculator calculator) :
            base(calculator) { }

        protected override void AttachHeader() => _currentPacket.Add(_options.SOH);

        protected override void InitializePacketSize() => _packetSize = 128;
    }
}