namespace XModemProtocol.Builders {
    using Calculators;
    using Options;
    public class OneKPacketBuilder : PacketBuilder {

        public OneKPacketBuilder(ICRCChecksumCalculator calculator) :
            base(calculator) { }

        protected override void AttachHeader() {
            byte header = _options.STX;
            if (MixedModeSelected && DataRemainingNotMoreThanNormalPacketSize) {
                _packetSize = 128;
                header = _options.SOH;
            }
            _currentPacket.Add(header);
        }

        protected override void InitializePacketSize() =>
            _packetSize = 1024;

        private bool MixedModeSelected =>
            _options.SenderOneKPacketSize == OneKPacketSize.Mixed;

        private bool DataRemainingNotMoreThanNormalPacketSize =>
            _data.Count - _position <= 128;
    }
}