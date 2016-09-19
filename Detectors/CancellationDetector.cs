using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Detectors {
    using Options;
    public class CancellationDetector : ICancellationDetector {

        List<byte> _input;
        List<int> _indicesOfCAN;
        IXModemProtocolOptions _options;

        public bool CancellationDetected(IEnumerable<byte> input , IXModemProtocolOptions options) {
            _input = _input.ToList();
            _options = options;
            if (DetectionIsUnnecessary()) return false;
            GetIndicesOfCANBytes();
            if (CountOfCANBytesAreInsufficient()) return false;
            if (CancellationConditionFound()) return true;
            return false;
        }

        private bool DetectionIsUnnecessary() => _options.CancellationBytesRequired < 1;

        private void GetIndicesOfCANBytes() {
            // LINQ to get indices of CAN bytes.
            // 1). If byte is CAN, record index, if not, make index -1.
            // 2). Remove all elements equal to -1,
            // 3). Place elements in ascending order.
            // 4). Convert to List<byte>. Only need to perform LINQ once.
            _indicesOfCAN = _input.Select((currentByte, i) => { if (currentByte == _options.CAN) return i; else return -1; })
                                     .Where(index => index > -1)
                                     .OrderBy(index => index)
                                     .ToList();
        }

        private bool CountOfCANBytesAreInsufficient() => _indicesOfCAN.Count < _options.CancellationBytesRequired;

        private bool CancellationConditionFound() {
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

            // No eligible CAN sequence found.
            return false;
        }

    }
}