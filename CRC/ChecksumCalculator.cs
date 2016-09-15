using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.CRC {
    public class ChecksumCalculator : BaseFunctions, ICRCChecksumCalculator {

        public static ChecksumCalculator Instance { get; } = new ChecksumCalculator();
        public IEnumerable<byte> InitialCRCValue { get; set; } = new byte[2];
        public ICRCLookUpTable Table { get; set; }

        private ChecksumCalculator() { }

        private int _runningChecksum;
        private int _currentByte;
        private int _operandFromTable;

        private int _checkSum;

        private List<byte> _input;

        /// <summary>
        /// A method used to calculate checksum.
        /// </summary>
        /// <param name="input">Message for which checksum will be computed.</param>
        /// <returns>A two byte enumerable containing checksum.</returns>
        public IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input) {
            _input = input.ToList();
            ComputeChecksum();
            return CheckSumAsArray();
        }

        private byte[] CheckSumAsArray() {
            return new byte[] { (byte)(_checkSum / 256), (byte)(_checkSum % 256) };
        }

        private void ComputeChecksum() {
            InitializeRunningChecksum();
            for (int i = 0; i < _input.Count; i++) {
                _currentByte = _input[i];
                GetOperandFromLookupTable();
                ReCalculateRunningCheckSumWithOperandFromTable();
            }
            _checkSum = _runningChecksum;
        }

        private void InitializeRunningChecksum() {
            _runningChecksum = ((InitialCRCValue.ElementAtOrDefault(1) << 8) | InitialCRCValue.ElementAtOrDefault(0));
        }

        private void GetOperandFromLookupTable() {
            int indexOfValueNeededFromLookupTable = (_runningChecksum >> 8) ^ _currentByte;
            _operandFromTable = Table.QueryTable(indexOfValueNeededFromLookupTable); 
        }
        
        private void ReCalculateRunningCheckSumWithOperandFromTable () {
            int shiftedRunningCheckSum = _runningChecksum << 8;
            _runningChecksum = ExtractLeastSignificantWordFrom(_operandFromTable ^ shiftedRunningCheckSum);
        }

        private int ExtractLeastSignificantWordFrom(int input) =>  ApplyMask(input , 0xFFFF);

    }
}