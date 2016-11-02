namespace XModemProtocol.Exceptions {
    using EventData;
    /// <summary>
    /// The Exception used inside the XModemCommunicator class.
    /// </summary>
    public class XModemProtocolException : System.ApplicationException {
        /// <summary>
        /// Whether cancellation should be sent or not.
        /// </summary>
        public bool PacketVerified { get; private set; } = false;
        /// <summary>
        /// Whether cancellation should be sent or not.
        /// </summary>
        public bool SendCancel { get; private set; } = false;
        /// <summary>
        /// The arguments used by the Aborted event.
        /// </summary>
        public AbortedEventArgs AbortArgs { get; private set; } = null;
        internal XModemProtocolException(AbortedEventArgs abortArgs = null, bool sendCancel = false, bool packetVerified = false) : base() {
            AbortArgs = abortArgs;
            SendCancel = sendCancel;
            PacketVerified = packetVerified;
        }
        internal XModemProtocolException(string message) : base(message) { }
    }
}