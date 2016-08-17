using System;
namespace XModemProtocol {
    /// <summary>
    /// The event data for the StateUpdated event.
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

        /// <summary>
        /// Constructor used to set properties needed by StateUpdated event.
        /// </summary>
        /// <param name="state">The current state of the instance.</param>
        /// <param name="oldState">The old state of the instance.</param>
        internal StateUpdatedEventArgs(XModemStates state, XModemStates oldState) {
            State = state;
            OldState = oldState;
        }
    }
}
