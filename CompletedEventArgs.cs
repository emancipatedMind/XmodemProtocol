using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.Completed event.
    /// </summary>
    public class CompletedEventArgs : EventArgs {
        /// <summary>
        /// Data that was sent or received by XModemCommunicator.
        /// </summary>
        public List<byte> Data { get; private set; } = null;
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.CompletedEventArgs class.
        /// </summary>
        internal CompletedEventArgs(List<byte> data) {
            Data = new List<byte>(data);
        }
    }
}