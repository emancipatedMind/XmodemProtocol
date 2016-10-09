namespace XModemProtocol.Operations.Finalize {
    using Options;
    public interface IFinalizer {
        void Finalize(IRequirements requirements);
    }
}