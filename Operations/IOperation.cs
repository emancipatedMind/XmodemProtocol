using XModemProtocol.Options;

namespace XModemProtocol.Operations {
    public interface IOperation {
        void Go(IRequirements requirements);
    }
}