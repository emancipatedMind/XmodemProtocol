namespace XModemProtocol.Calculators {
    using Support;
    using System.Linq;
    public class FunctionalLookUpTable : ICRCLookUpTable {

        int[] _table;

        public FunctionalLookUpTable(int polynomial) {
            Polynomial = polynomial;
            _table = Enumerable
                .Range(0, 256)
                .Select(d => d << 8)
                .Select(d =>
                    Enumerable.Range(0, 8)
                    .Select(s => d << s)
                    .Aggregate(0,
                        (value, next) =>
                            (((value ^ next) & 0x8000) != 0) ?
                                ((value << 1) ^ Polynomial) :
                                value << 1
                    )
                    .ApplyMask(0xFFFF)
                )
                .ToArray();
        }

        public int Polynomial { get; }

        public int QueryTable(int index) => _table[index];

        #region ObjectOverrides
        public override string ToString() =>
            $"[Polynomial : 0x{Polynomial:4x}]";

        public override int GetHashCode() =>
            ToString().GetHashCode();

        public override bool Equals(object obj) =>
            obj is FunctionalLookUpTable && GetHashCode() == obj.GetHashCode();
        #endregion
    }
}