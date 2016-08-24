using System;

namespace XModemProtocol {
    /// <summary>
    /// The Exception used inside the XModemCommunicator class.
    /// </summary>
    public class XModemProtocolException : ApplicationException {
        /// <summary>
        /// The arguments used by the Aborted event.
        /// </summary>
        public AbortedEventArgs AbortArgs { get; private set; } = null;
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.XModeomProtocolException class.
        /// </summary>
        internal XModemProtocolException() : base() { }
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.XModeomProtocolException class.
        /// </summary>
        /// <param name="abortArgs">Used to fill AbortArgs property</param>
        internal XModemProtocolException(AbortedEventArgs abortArgs) : base() {
            AbortArgs = abortArgs;
        }
    }
}