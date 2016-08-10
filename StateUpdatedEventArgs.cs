﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the StateUpdated event.
    /// </summary>
    public class StateUpdatedEventArgs {
        /// <summary>
        /// The new state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates State { get; private set; }
        /// <summary>
        /// The old state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates OldState { get; private set; }

        public StateUpdatedEventArgs(XModemStates state, XModemStates oldState) {
            State = state;
            OldState = oldState;
        }
    }
}