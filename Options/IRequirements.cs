namespace XModemProtocol.Options {
    using Communication;
    using Factories;
    public interface IRequirements {
        IXModemProtocolOptions Options { get; }
        IContext Context { get; }
        ICommunicator Communicator { get; }
        IToolFactory ToolFactory { get; }
    }
}