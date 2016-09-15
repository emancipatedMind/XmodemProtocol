using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.CRC {
    public class LookUpTable : BaseFunctions, ICRCLookUpTable {

        private int[] _lookupTable = new int[256];
        private int _polynomial;
        private int _controlByte;
        private int _tableValue;
        private bool _tableBuilt = false;


        public int Polynomial {
            get { return _polynomial; }
            set {
                value = ApplyMask(value, 0xFFFF);
                if (_polynomial == value) return;
                _polynomial = value;
                _tableBuilt = false;
            }
        }

        public ICRCLookUpTable Table { get { return this; } }

        public LookUpTable(int polynomial) {
            Polynomial = polynomial;
        }

        public int QueryTable(int index) {
            if (_tableBuilt == false) MakeTable(); 
            return _lookupTable[index];
        }

        private void MakeTable() {
            for (int index = 0; index < _lookupTable.Length; ++index) {
                CreateNewControlByteUsing(index);
                CalculateNextTableValueFromControlByte();
                InsertTableValueIntoTableAt(index);
            }
            _tableBuilt = true;
        }

        private void CalculateNextTableValueFromControlByte() {
            _tableValue = 0;
            for (int i = 0; i < 8; ++i) {
                ShiftTableValueAndXORWithPolynomialIfNecessary();
                _controlByte <<= 1;
            }
            _tableValue = ApplyMask(_tableValue, 0xFFFF);
        }

        private void ShiftTableValueAndXORWithPolynomialIfNecessary() {
            bool xorIsNecessary = IsXorWithPolynomialNeeded();
            _tableValue <<= 1;
            if (xorIsNecessary) _tableValue = _tableValue ^ Polynomial;
        }

        private bool IsXorWithPolynomialNeeded() {
            int xorResult = _tableValue ^ _controlByte;
            int mostSignificantBit = ApplyMask(xorResult, 0x8000); 
            return mostSignificantBit == 0x8000;
        }

        private void InsertTableValueIntoTableAt(int index) => _lookupTable[index] = _tableValue;

        private void CreateNewControlByteUsing(int index) => _controlByte = index << 8;

        #region ObjectOverrides
        public override string ToString() {
            return $"[Polynomial : 0x{Polynomial:4x}]";
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
