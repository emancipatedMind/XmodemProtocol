using System;
using System.Collections.Generic;
using System.Linq;

namespace XModemProtocol.CRC {
    /// <summary>
    /// A class implementation of ZMODEM, CRC-16/ACORN, CRC-16/LTE.
    /// </summary>
    public class CRC16LTE : ICRCChecksumValidator {

        /// <summary>
        /// Used as initial value to calculation. Indices 1 (MSB), and 0 (LSB) are used.
        /// </summary>
        public IEnumerable<byte> InitialCRCValue { get; set; } = new byte[2];
        /// <summary>
        /// Polynomial used in calculation. Default = 0x1021.
        /// </summary>
        public int Polynomial {
            get { return iCRCLookUpTable.Polynomial; }
            set { iCRCLookUpTable.Polynomial = value; }
        }

        private ICRCLookUpTable iCRCLookUpTable { get; } = new LookUpTable(0x1021);

        private ICRCChecksumCalculator iCRCGetChecksum { get; } = CRCChecksumCalculator.Instance;

        /// <summary>
        /// Reference used by ValidateChecksum(). 
        /// </summary>
        public IEnumerable<byte> ChecksumReference { get; set; } = new byte[2];

        /// <summary>
        /// Initializes a new instance of the XModemProtocol.CRC16LTE class.
        /// </summary>
        public CRC16LTE() {
            iCRCGetChecksum.Table = iCRCLookUpTable;
        }

        /// <summary>
        /// A method used to check whether collection computes to ChecksumReference indicating checksum is correct.
        /// </summary>
        /// <param name="input">Message in bytes with the checksum as the last two bytes.</param>
        /// <returns>A boolean saying whether the checksum is correct(true) or not(false).</returns>
        public bool ValidateChecksum(IEnumerable<byte> input) {
            var checksum = CalculateChecksum(input);
            return checksum.SequenceEqual(ChecksumReference); 
        }

        /// <summary>
        /// A method used to calculate checksum.
        /// </summary>
        /// <param name="input">Message for which checksum will be computed.</param>
        /// <returns>A two byte enumerable containing checksum.</returns>
        public IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input) {
            return iCRCGetChecksum.CalculateChecksum(input);
        }

    }
}