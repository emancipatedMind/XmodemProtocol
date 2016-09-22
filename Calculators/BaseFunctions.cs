namespace XModemProtocol.Calculators {
    public abstract class BaseFunctions {
        protected int ApplyMask(int input, int mask) =>  input & mask;
    }
}