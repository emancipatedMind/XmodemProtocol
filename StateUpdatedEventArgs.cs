using System;
namespace XModemProtocol {
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
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.StateUpdatedEventArgs class.
        /// </summary>
        /// <param name="state">The current state of the instance.</param>
        /// <param name="oldState">The old state of the instance.</param>
        internal StateUpdatedEventArgs(XModemStates state, XModemStates oldState) {
            State = state;
            OldState = oldState;
        }
    }
}