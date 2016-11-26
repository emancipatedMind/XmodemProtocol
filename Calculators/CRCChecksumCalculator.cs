namespace XModemProtocol.Calculators {
    using System.Collections.Generic;
    using System.Linq;
    public class CRCChecksumCalculator : BaseFunctions, ICRCChecksumCalculator {

        public IEnumerable<byte> InitialCRCValue { get; set; } = new byte[2];

        private ICRCLookUpTable _table;
        private int _runningChecksum;
        private int _currentByte;
        private int _operandFromTable;
        private int _checkSum;
        private List<byte> _input;

        public CRCChecksumCalculator(ICRCLookUpTable table) {
            _table = table;
        }

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

        private byte[] CheckSumAsArray() => new byte[] { (byte)(_checkSum / 256), (byte)(_checkSum % 256) };

        private void ComputeChecksum() {
            InitializeRunningChecksum();
            for (int i = 0; i < _input.Count; i++) {
                _currentByte = _input[i];
                GetOperandFromLookupTable();
                ReCalculateRunningCheckSumWithOperandFromTable();
            }
            _checkSum = _runningChecksum;
        }

        private void InitializeRunningChecksum() =>
            _runningChecksum = ((InitialCRCValue.ElementAtOrDefault(1) << 8) | InitialCRCValue.ElementAtOrDefault(0));
        

        private void GetOperandFromLookupTable() {
            int indexOfValueNeededFromLookupTable = (_runningChecksum >> 8) ^ _currentByte;
            _operandFromTable = _table.QueryTable(indexOfValueNeededFromLookupTable); 
        }
        
        private void ReCalculateRunningCheckSumWithOperandFromTable () {
            int shiftedRunningCheckSum = _runningChecksum << 8;
            _runningChecksum = ExtractLeastSignificantWordFrom(_operandFromTable ^ shiftedRunningCheckSum);
        }

        private int ExtractLeastSignificantWordFrom(int input) =>  ApplyMask(input , 0xFFFF);

    }
}