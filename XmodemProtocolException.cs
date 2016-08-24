using System;

namespace XModemProtocol {
    /// <summary>
    /// The Exception used inside the XModemCommunicator class.
    /// </summary>
    public class XModemProtocolException : ApplicationException {
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
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.XModeomProtocolException class.
        /// </summary>
        /// <param name="abortArgs">Used to fill AbortArgs property</param>
        /// <param name="sendCancel">Used to signal whether cancel should be sent or not.</param>
        internal XModemProtocolException(AbortedEventArgs abortArgs = null, bool sendCancel = false, bool packetVerified = false) : base() {
            AbortArgs = abortArgs;
            SendCancel = sendCancel;
            PacketVerified = packetVerified;
        }
    }
}