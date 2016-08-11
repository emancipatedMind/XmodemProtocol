using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the PacketSent event.
    /// </summary>
    public class PacketSentEventArgs : EventArgs {
        /// <summary>
        /// The packet number that was sent.
        /// </summary>
        public int PacketNumber { get; private set; }
        /// <summary>
        /// The contents of the packet that was sent.
        /// </summary>
        public List<byte> PacketSent { get; private set; }

        public PacketSentEventArgs(int packetNumber, List<byte> packetSent) {
            PacketNumber = packetNumber;
            PacketSent = packetSent;
        }

    }
}
