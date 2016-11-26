namespace XModemProtocol.Validators.Checksum {
    using Calculators;
    using System.Collections.Generic;
    using System.Linq;

    public class NormalChecksumValidator : IChecksumValidator {
        IChecksumCalculator _calculator;
        byte _calculatedChecksum;
        byte _checksumReceived;
        List<byte> _payload;
        List<byte> _input;

        public NormalChecksumValidator(IChecksumCalculator calculator) {
            _calculator = calculator;
        }

        public bool ValidateChecksum(IEnumerable<byte> input) {
            _input = input.ToList();
            GetPayLoadOfInput();
            ExtractChecksumOfInput();
            CalculateChecksum();
            return WhetherCalculatedChecksumAndChecksumReceivedWereIdentical();
        }

        private void GetPayLoadOfInput() =>
            _payload = _input.GetRange(0, _input.Count - 1);

        private void ExtractChecksumOfInput() =>
            _checksumReceived = _input.Last();

        private void CalculateChecksum() =>
            _calculatedChecksum = _calculator.CalculateChecksum(_payload).ElementAt(0);


        private bool WhetherCalculatedChecksumAndChecksumReceivedWereIdentical() =>
            _calculatedChecksum == _checksumReceived;
    }
}