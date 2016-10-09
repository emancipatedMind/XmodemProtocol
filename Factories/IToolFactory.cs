namespace XModemProtocol.Factories {
    using Tools;
    public interface IToolFactory {
        IXModemTools GetToolsFor(XModemMode mode); 
    }
}