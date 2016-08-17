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

        /// <summary>
        /// Constructor used to set properties needed by Aborted event.
        /// </summary>
        /// <param name="reason">Reason why abort event is being fired.</param>
        internal AbortedEventArgs(XModemAbortReason reason) {
            Reason = reason;
        }
    }
}