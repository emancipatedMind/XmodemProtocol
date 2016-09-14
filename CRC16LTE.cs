using System.Collections.Generic;
using System.Linq;

namespace XModemProtocol {
    /// <summary>
    /// A class implementation of ZMODEM, CRC-16/ACORN, CRC-16/LTE.
    /// </summary>
    public class CRC16LTE {

        LookUpTable _tableCalculator;

        /// <summary>
        /// new byte[] {0, 0}
        /// </summary>
        public static byte[] Zeros { get; } = new byte[] { 0, 0 };
        /// <summary>
        /// new byte[] {0xFF, 0xFF}
        /// </summary>
        public static byte[] Ones { get; } = new byte[] { 0xFF, 0xFF };
        /// <summary>
        /// Used as initial value to calculation. Indices 1 (MSB), and 0 (LSB) are used.
        /// </summary>
        public IEnumerable<byte> InitialCRCValue { get; set; } = Zeros;
        /// <summary>
        /// Polynomial used in calculation.
        /// </summary>
        public int Polynomial {
            get { return _tableCalculator.Polynomial; }
            set {
                value = ExtractLeastSignificantWordFrom(value);
                if (_tableCalculator.Polynomial == value) return;
                _tableCalculator = new LookUpTable(value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the XModemProtocol.CRC16LTE class.
        /// </summary>
        /// <param name="polynomial">Polynomial to be used. 16 bits.</param>
        public CRC16LTE(int polynomial = 0x1021) {
            _tableCalculator = new LookUpTable(polynomial);
        }

        /// <summary>
        /// A method used to check whether collection computes to 0 indicating checksum is correct.
        /// </summary>
        /// <param name="input">Message in bytes with the checksum as the last two bytes.</param>
        /// <returns>A boolean saying whether the checksum is correct(true) or not(false).</returns>
        public bool ApproveMessage(IEnumerable<byte> input) {
            byte[] checksum = GetChecksum(input);
            return checksum.SequenceEqual(Zeros); 
        }

        /// <summary>
        /// A method used to calculate checksum.
        /// </summary>
        /// <param name="input">Message for which checksum will be computed.</param>
        /// <returns>A two byte enumerable containing checksum.</returns>
        public byte[] GetChecksum(IEnumerable<byte> input) {
            int initialValueForCalculation = ((InitialCRCValue.ElementAtOrDefault(1) << 8) | InitialCRCValue.ElementAtOrDefault(0));
            int checkSum = ComputeChecksum(input.ToList(), initialValueForCalculation);
            byte[] checkSumAsArray = new byte[] { (byte)(checkSum / 256), (byte)(checkSum % 256) };
            return checkSumAsArray;
        }

        private int ComputeChecksum(List<byte> input, int initialValueForCalculation) {
            int runningCheckSum = initialValueForCalculation;
            for (int i = 0; i < input.Count; i++) {
                int operandFromLookupTable = GetOperandFromLookupTable(runningCheckSum, input[i]);
                runningCheckSum = ReCalculateRunningCheckSumWith(operandFromLookupTable, runningCheckSum);
            }
            return runningCheckSum;
        }

        private int GetOperandFromLookupTable(int runningCheckSum, byte currentByte) {
            int indexOfValueNeededFromLookupTable = (runningCheckSum >> 8) ^ currentByte;
            return _tableCalculator.QueryTable(indexOfValueNeededFromLookupTable); 
        }
        
        private int ReCalculateRunningCheckSumWith (int valueFromLookupTable, int runningCheckSum) {
            int shiftedRunningCheckSum = runningCheckSum << 8;
            return ExtractLeastSignificantWordFrom(valueFromLookupTable ^ shiftedRunningCheckSum);
        }

        private int ExtractLeastSignificantWordFrom(int input) =>  input & 0xFFFF;

        class LookUpTable {

            private int[] _lookupTable = new int[256];
            public int Polynomial { get; private set; }
            int _controlByte;
            int _tableValue;

            public LookUpTable(int polynomial) {
                Polynomial = polynomial;
                MakeTable();
            }

            public int QueryTable(int index) => _lookupTable[index];

            private void MakeTable() {
                for (int index = 0; index < _lookupTable.Length; ++index) {
                    CreateNewControlByteUsing(index);
                    CalculateNextTableValueFromControlByte();
                    InsertTableValueIntoTableAt(index);
                }
            }

            private void CalculateNextTableValueFromControlByte() {
                _tableValue = 0;
                for (int i = 0; i < 8; ++i) {
                    ShiftTableValueAndXORWithControlByteIfNecessary();
                    _controlByte <<= 1;
                }
                _tableValue = ApplyMask(_tableValue, 0xFFFF);
            }

            private void ShiftTableValueAndXORWithControlByteIfNecessary() {
                bool xorIsNecessary = IsXorWithPolynomialNeeded();
                _tableValue <<= 1;
                if (xorIsNecessary) _tableValue = _tableValue ^ Polynomial;
            }

            private bool IsXorWithPolynomialNeeded() {
                int xorResult = _tableValue ^ _controlByte;
                int mostSignificantBit = ApplyMask(xorResult, 0x8000); 
                return mostSignificantBit == 0x8000;
            }

            private int ApplyMask(int input, int mask) =>  input & mask;

            private void InsertTableValueIntoTableAt(int index) => _lookupTable[index] = _tableValue;

            private void CreateNewControlByteUsing(int index) => _controlByte = index << 8;

            #region ObjectOverrides
            public override string ToString()
            {
                return $"A lookup table for CRC for the polynomial 0x{Polynomial:4x}.";
            }

            public override int GetHashCode()
            {
                return ToString().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                if (obj is LookUpTable && GetHashCode() == obj.GetHashCode())
                    return true;
                else return false;
            }
            #endregion        

        }
    }
}