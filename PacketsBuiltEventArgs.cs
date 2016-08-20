using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.PacketsBuilt event.
    /// </summary>
    public class PacketsBuiltEventArgs : EventArgs {
        /// <summary>
        /// The new count of packets.
        /// </summary>
        public List<List<byte>> Packets { get; private set; }
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.PacketsBuiltEventArgs class.
        /// </summary>
        /// <param name="packets">A list of the packets built.</param>
        internal PacketsBuiltEventArgs(List<List<byte>> packets) {
            Packets = new List<List<byte>>(packets);
        }
    }
}