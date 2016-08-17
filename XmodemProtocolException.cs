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
        /// Default constructor.
        /// </summary>
        public XModemProtocolException() : base() { }
        /// <summary>
        /// Constructor used to set the arguments used by the Aborted event.
        /// </summary>
        /// <param name="abortArgs"></param>
        public XModemProtocolException(AbortedEventArgs abortArgs) : base() {
            AbortArgs = abortArgs;
        }

    }
}