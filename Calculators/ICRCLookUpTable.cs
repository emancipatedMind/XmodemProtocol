namespace XModemProtocol.Calculators {
    public interface ICRCLookUpTable {
        int QueryTable(int index);
        int Polynomial { get; }
    }
}