using System;

namespace XModemProtocol.EventData {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.StateUpdated event.
    /// </summary>
    public class StateUpdatedEventArgs : EventArgs {
        /// <summary>
        /// The new state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates State { get; private set; }
        /// <summary>
        /// The old state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates OldState { get; private set; }
        internal StateUpdatedEventArgs(XModemStates state, XModemStates oldState) {
            State = state;
            OldState = oldState;
        }
    }
}