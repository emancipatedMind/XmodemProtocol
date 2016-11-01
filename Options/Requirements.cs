namespace XModemProtocol.Options {
    using Communication;
    using Factories;
    public class Requirements : IRequirements {
        public IContext Context { get; set; }
        public IXModemProtocolOptions Options { get; set; }
        public ICommunicator Communicator { get; set; }
        public IToolFactory ToolFactory { get; set; }
    }
}