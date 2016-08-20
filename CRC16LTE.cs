using System.Collections.Generic;
using System.Linq;

namespace XModemProtocol {
    /// <summary>
    /// A class implementation of ZMODEM, CRC-16/ACORN, CRC-16/LTE.
    /// </summary>
    public class CRC16LTE {

        int _polynomial = 0;
        List<int> _lookupTable;

        /// <summary>
        /// new List<byte> {0, 0}
        /// </summary>
        public static List<byte> Zeros { get; } = new List<byte> { 0, 0 };
        /// <summary>
        /// new List<byte> {0xFF, 0xFF}
        /// </summary>
        public static List<byte> Ones { get; } = new List<byte> { 0xFF, 0xFF };
        /// <summary>
        /// Used as initial value to calculation. Indices 1, and 0 are used.
        /// </summary>
        public IEnumerable<byte> InitialCRCValue { get; set; } = Zeros;
        /// <summary>
        /// Polynomial used in calculation.
        /// </summary>
        public int Polynomial {
            get { return _polynomial; }
            set {
                // If _polynomial and value are equal, do nothing.
                if (_polynomial == value) return;
                _polynomial = 0xFFFF & value;
                // If setting polynomial, table must be calculated.
                _lookupTable = new List<int>(256);
                int temp, a;
                for (int i = 0; i < _lookupTable.Capacity; ++i) {
                    temp = 0;
                    a = i << 8;
                    for (int j = 0; j < 8; ++j) {
                        if (((temp ^ a) & 0x8000) != 0)
                            temp = (temp << 1) ^ _polynomial;
                        else
                            temp <<= 1;
                        a <<= 1;
                    }
                    _lookupTable.Add(0xFFFF & temp);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the XModemProtocol.CRC16LTE class.
        /// </summary>
        /// <param name="polynomial">Polynomial to be used. 16 bits.</param>
        public CRC16LTE(int polynomial = 0x1021) {
            Polynomial = polynomial;
        }

        /// <summary>
        /// A method used to calculate checksum.
        /// </summary>
        /// <param name="input">Message for which checksum will be computed.</param>
        /// <returns>A two byte enumerable containing checksum.</returns>
        public List<byte> ComputeChecksum(IEnumerable<byte> input) {
            return input.Aggregate(
                ((InitialCRCValue.ElementAtOrDefault(1) << 8) | InitialCRCValue.ElementAtOrDefault(0)),
                (crc, next) => 0xFFFF & ((crc << 8) ^ _lookupTable[((crc >> 8) ^ (0xff & next))]),
                crc => new List<byte> { (byte)(crc / 256), (byte)(crc % 256) }
            );
        }

        /// <summary>
        /// A method used to check whether the checksum is correct.
        /// </summary>
        /// <param name="input">Message in bytes with the checksum as the last two bytes.</param>
        /// <returns>A boolean saying whether the checksum is correct(true) or not(false).</returns>
        public bool ApproveMessage(IEnumerable<byte> input) {
            List<byte> checksum = ComputeChecksum(input);
            return checksum.SequenceEqual(Zeros); 
        }

    }

}