namespace XModemProtocol.Detectors {
    using Options;
    using System.Collections.Generic;
    using System.Linq;
    public class CancellationDetector : ICancellationDetector {

        byte[] _input;
        List<int> _indicesOfCAN;
        IXModemProtocolOptions _options;

        public bool CancellationDetected(IEnumerable<byte> input , IXModemProtocolOptions options) {
            _input = input.ToArray();
            _options = options;
            if (DetectionIsUnnecessary) return false;
            GetIndicesOfCANBytes();
            if (CountOfCANBytesAreInsufficient) return false;
            if (CancellationConditionFound) return true;
            return false;
        }

        private bool DetectionIsUnnecessary => _options.CancellationBytesRequired < 1;

        private void GetIndicesOfCANBytes() {
            _indicesOfCAN = new List<int>();
            for (int index = 0; index < _input.Length; index++)
                if (_input[index] == _options.CAN)
                    _indicesOfCAN.Add(index);
        }

        private bool CountOfCANBytesAreInsufficient => _indicesOfCAN.Count < _options.CancellationBytesRequired;

        private bool CancellationConditionFound {
            get {
                for (int i = 0, counter = 0, index = _indicesOfCAN[0]; i < _indicesOfCAN.Count; i++) {
                    int next = _indicesOfCAN[i];
                    if (index == next) {
                        ++index;
                        ++counter;
                    }
                    else {
                        index = next + 1;
                        counter = 1;
                    }

                    if (counter >= _options.CancellationBytesRequired) return true;
                }
                return false;
            }
        }

    }
}