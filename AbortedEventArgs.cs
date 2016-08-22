using System;

namespace XModemProtocol {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.Aborted event.
    /// </summary>
    public class AbortedEventArgs : EventArgs {
        /// <summary>
        /// Reason for abort.
        /// </summary>
        public XModemAbortReason Reason { get; private set; }
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.AbortedEventArgs class.
        /// </summary>
        /// <param name="reason">Reason why abort event is being fired.</param>
        internal AbortedEventArgs(XModemAbortReason reason) {
            Reason = reason;
        }
    }
}