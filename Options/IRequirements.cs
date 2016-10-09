namespace XModemProtocol.Options {
    using Communication;
    public interface IRequirements {
        IXModemProtocolOptions Options { get; }
        IContext Context { get; }
        ICommunicator Communicator { get; }
    }
}