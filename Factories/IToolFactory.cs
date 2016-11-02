namespace XModemProtocol.Factories {
    public interface IToolFactory {
        Tools.IXModemTools GetToolsFor(XModemMode mode); 
        int Polynomial { get; set; }
    }
}