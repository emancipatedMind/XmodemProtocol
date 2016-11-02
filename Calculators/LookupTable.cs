namespace XModemProtocol.Calculators {
    public class LookUpTable : BaseFunctions, ICRCLookUpTable {

        private int[] _lookupTable = new int[256];
        private int _controlByte;
        private int _tableValue;
        private int _currentIndex;

        public int Polynomial { get; private set; }

        public LookUpTable(int polynomial) {
            Polynomial = polynomial;
            MakeTable();
        }

        public int QueryTable(int index) => _lookupTable[index];

        private void MakeTable() {
            for (_currentIndex = 0; _currentIndex < _lookupTable.Length; ++_currentIndex) {
                CreateNewControlByte();
                CalculateNextTableValueFromControlByte();
                InsertTableValueIntoTable();
            }
        }

        private void CreateNewControlByte() => _controlByte = _currentIndex << 8;

        private void CalculateNextTableValueFromControlByte() {
            _tableValue = 0;
            for (int i = 0; i < 8; ++i) {
                ShiftTableValueAndXORWithPolynomialIfNecessary();
                _controlByte <<= 1;
            }
            _tableValue = ApplyMask(_tableValue, 0xFFFF);
        }

        private void ShiftTableValueAndXORWithPolynomialIfNecessary() {
            if (XorIsNecessary) _tableValue = (_tableValue << 1) ^ Polynomial;
            else _tableValue <<= 1;
        }

        private bool XorIsNecessary {
            get {
                int xorResult = _tableValue ^ _controlByte;
                int mostSignificantBitOfXorResult = ApplyMask(xorResult, 0x8000); 
                return mostSignificantBitOfXorResult == 0x8000;
            }
        }

        private void InsertTableValueIntoTable() => _lookupTable[_currentIndex] = _tableValue;

        #region ObjectOverrides
        public override string ToString() {
            return $"[Polynomial : 0x{Polynomial:4x}]";
        }

        public override int GetHashCode() {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is LookUpTable && GetHashCode() == obj.GetHashCode())
                return true;
            else return false;
        }
        #endregion        
    }
}