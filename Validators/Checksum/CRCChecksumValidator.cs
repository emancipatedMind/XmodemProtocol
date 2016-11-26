namespace XModemProtocol.Validators.Checksum {
    using Calculators;
    using System.Collections.Generic;
    using System.Linq;

    public class CRCChecksumValidator : ICRCChecksumValidator {

        /// <summary>
        /// Reference used by ValidateChecksum(). 
        /// </summary>
        public IEnumerable<byte> ChecksumReference { get; set; } = new byte[2];

        public CRCChecksumValidator(ICRCChecksumCalculator calculator) {
            _calculator = calculator;
        }

        private ICRCChecksumCalculator _calculator;

        /// <summary>
        /// A method used to check whether collection computes to ChecksumReference indicating checksum is correct.
        /// </summary>
        /// <param name="input">Message in bytes with the checksum as the last two bytes.</param>
        /// <returns>A boolean saying whether the checksum is correct(true) or not(false).</returns>
        public bool ValidateChecksum(IEnumerable<byte> input) {
            var checksum = _calculator.CalculateChecksum(input);
            return checksum.SequenceEqual(ChecksumReference); 
        }
    }
}