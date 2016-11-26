namespace XModemProtocol.Builders {
    using Calculators;
    using Options;
    using System.Collections.Generic;
    using System.Linq;
    public abstract class PacketBuilder : IPacketBuilder {
        protected int _position;
        protected int _packetSize;
        protected List<byte> _currentPacket;
        protected IXModemProtocolOptions _options;
        protected abstract void InitializePacketSize();
        protected abstract void AttachHeader();
        protected List<byte> _data;

        private IChecksumCalculator _calculator;
        private List<byte> _packetInfo;
        private List<List<byte>> _packets;
        private int _packetNumber;

        protected PacketBuilder(IChecksumCalculator calculator) {
            _calculator = calculator;
        }

        public List<List<byte>> GetPackets(IEnumerable<byte> input, IXModemProtocolOptions options) {
            _data = input.ToList();
            _options = options;
            _packetNumber = 1;
            _position = 0;
            _packets = new List<List<byte>>();
            InitializePacketSize();
            while(MorePacketsToBeBuilt) {
                BuildNextPacket();
                PrepareForNextPacketBuild();
            }
            return _packets;
        }

        private void BuildNextPacket() {
            ResetNewPacket();
            AttachHeader();
            AppendPacketNumberAndOnesComplementOfPacketNumber();
            AppendData();
            AppendChecksum();
            AppendNewPacketToPackets();
        }

        private void ResetNewPacket() => _currentPacket = new List<byte>();

        private void AppendPacketNumberAndOnesComplementOfPacketNumber() {
            _currentPacket.Add((byte) _packetNumber);
            byte onesComplement = (byte)(0xFF - _packetNumber);
            _currentPacket.Add(onesComplement);
        }

        private void AppendData() {
            if (DataLeftWillFillPacket) 
                _packetInfo = _data.GetRange(_position, _packetSize);
            else 
                _packetInfo = PadDataWithSUBByte();
            _currentPacket.AddRange(_packetInfo);
        }

        private bool DataLeftWillFillPacket => _position + _packetSize <= _data.Count;

        private List<byte> PadDataWithSUBByte() {
            int countOfRestOfBytes = _data.Count - _position;
            int numbOfSUB = _packetSize - countOfRestOfBytes;
            IEnumerable<byte> restOfBytes = _data.GetRange(_position, countOfRestOfBytes);
            IEnumerable<byte> paddedPacket = restOfBytes.Concat(Enumerable.Repeat(_options.SUB, numbOfSUB));
            return paddedPacket.ToList(); 
        }

        private void AppendChecksum() {
            var checksum = _calculator.CalculateChecksum(_packetInfo).ToList();
            _currentPacket.AddRange(checksum);
        }

        private void PrepareForNextPacketBuild() {
            _packetNumber++;
            _position += _packetSize;
        }

        private bool MorePacketsToBeBuilt => _position < _data.Count;

        private void AppendNewPacketToPackets() {
            _packets.Add(_currentPacket);
        }
    }
}