using System;

namespace XModemProtocol {
    /// <summary>
    /// The Exception used inside the XModemCommunicator class.
    /// </summary>
    public class XModemProtocolException : ApplicationException {

        public AbortedEventArgs AbortArgs { get; private set; } = null;

        public XModemProtocolException(AbortedEventArgs abortArgs) : base() {
            AbortArgs = abortArgs;
        }

    }
}