using XModemProtocol.Options;
namespace XModemProtocol.Operations.Finalize {
    public interface IFinalizer {
        void Finalize(IRequirements requirements);
    }
}