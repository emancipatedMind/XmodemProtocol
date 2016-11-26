namespace XModemProtocol.EventData {
    using System.Collections.Generic;
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.Completed event.
    /// </summary>
    public class CompletedEventArgs : System.EventArgs {
        /// <summary>
        /// Data that was sent or received by XModemCommunicator.
        /// </summary>
        public List<byte> Data { get; private set; }
        internal CompletedEventArgs(List<byte> data) {
            Data = new List<byte>(data);
        }
    }
}