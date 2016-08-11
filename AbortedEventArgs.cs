using System;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the Aborted event.
    /// </summary>
    public class AbortedEventArgs : EventArgs {
        /// <summary>
        /// Reason for abort.
        /// </summary>
        public XModemAbortReason Reason { get; private set; }

        public AbortedEventArgs(XModemAbortReason reason) {
            Reason = reason;
        }
    }
}