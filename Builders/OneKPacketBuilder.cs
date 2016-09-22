namespace XModemProtocol.Builders {
    using Calculators;
    public class OneKPacketBuilder : PacketBuilder {

        public OneKPacketBuilder(ICRCChecksumCalculator calculator) :
            base(calculator) { }

        protected override void AttachHeader() {
            byte header = _options.STX;
            if (DataRemainingNotMoreThanNormalPacketSize()) {
                _packetSize = 128;
                header = _options.SOH;
            }
            _currentPacket.Add(header);
        }

        protected override void InitializePacketSize() {
            _packetSize = 1024;
        }

        private bool DataRemainingNotMoreThanNormalPacketSize() {
            if (_data.Count <= 1024)
                return _data.Count <= 128;
            int bytesPackaged = _position + _packetSize;
            return _data.Count - bytesPackaged <= 128;
        }
    }
}