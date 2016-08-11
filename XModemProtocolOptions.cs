namespace XModemProtocol {
    /// <summary>
    /// Abstract class used to hold the options of both the Sender, and Receiver.
    /// </summary>
    public abstract class XModemProtocolOptions {

        /// <summary>
        /// Mode to be used for operation. Default : Auto
        /// </summary>
        public XModemMode Mode { get; set; } = XModemMode.Auto;

    }
}