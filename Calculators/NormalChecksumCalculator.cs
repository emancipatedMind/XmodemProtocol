namespace XModemProtocol.Calculators {
    using System.Collections.Generic;
    using System.Linq;
    public class NormalChecksumCalculator : ISummationChecksumCalculator {

        List<byte> _input;
        public IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input) {
            _input = input.ToList();
            return new byte[] { LeastSignificantByteOfSummation()};
        }

        private byte LeastSignificantByteOfSummation() =>
            (byte) _input.Sum(currentByte => currentByte);
    }
}