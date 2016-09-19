using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Calculators {
    using Options;
    public class NormalChecksumCalculator : ISummationChecksumCalculator {

        List<byte> _input;

        public IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input) {
            _input = input.ToList();
            return new byte[] { LeastSignificantByteOfSummation()};
        }

        private byte LeastSignificantByteOfSummation() {
            return (byte) _input.Sum(currentByte => currentByte);
        }
    }
}