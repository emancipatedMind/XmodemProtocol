namespace XModemProtocol.Calculators {
    using System.Collections.Generic;
    using System.Linq;
    public class FunctionalNormalChecksumCalculator : ISummationChecksumCalculator {

        public IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input) =>
            new byte[] { (byte)input.Sum(b => b) };

    }
}