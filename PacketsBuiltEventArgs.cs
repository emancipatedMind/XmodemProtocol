using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the PacketsBuilt event.
    /// </summary>
    public class PacketsBuiltEventArgs : EventArgs {
        /// <summary>
        /// The new count of packets.
        /// </summary>
        public List<List<byte>> Packets { get; private set; }

        /// <summary>
        /// Constructor that sets properties needed by PacketsBuilt event.
        /// </summary>
        /// <param name="packets">A list of the packets built.</param>
        internal PacketsBuiltEventArgs(List<List<byte>> packets) {
            Packets = new List<List<byte>>(packets);
        }
    }
}