namespace XModemProtocol.Options {
    public interface IContext {
        XModemStates State { get; set; }
        bool BuildRequested { get; set; }
    }
}