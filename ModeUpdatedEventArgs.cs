using System;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the ModeUpdated event.
    /// </summary>
    public class ModeUpdatedEventArgs : EventArgs {
        /// <summary>
        /// The new mode of the XModemCommunicator instance.
        /// </summary>
        public XModemMode Mode { get; private set; }
        /// <summary>
        /// The old mode of the XModemCommunicator instance.
        /// </summary>
        public XModemMode OldMode { get; private set; }

        /// <summary>
        /// Constructor used to set properties needed by ModeUpdated event.
        /// </summary>
        /// <param name="mode">Current mode.</param>
        /// <param name="oldMode">Old mode.</param>
        internal ModeUpdatedEventArgs(XModemMode mode, XModemMode oldMode) {
            Mode = mode;
            OldMode = oldMode;
        }
    }
}