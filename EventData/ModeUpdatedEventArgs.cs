namespace XModemProtocol.EventData {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.ModeUpdated event.
    /// </summary>
    public class ModeUpdatedEventArgs : System.EventArgs {
        /// <summary>
        /// The new mode of the XModemCommunicator instance.
        /// </summary>
        public XModemMode Mode { get; private set; }
        /// <summary>
        /// The old mode of the XModemCommunicator instance.
        /// </summary>
        public XModemMode OldMode { get; private set; }
        internal ModeUpdatedEventArgs(XModemMode mode, XModemMode oldMode) {
            Mode = mode;
            OldMode = oldMode;
        }
    }
}