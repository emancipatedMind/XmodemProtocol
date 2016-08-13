using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the PacketSent event.
    /// </summary>
    public class PacketToSendEventArgs : EventArgs {
        /// <summary>
        /// The packet number that was sent.
        /// </summary>
        public int PacketNumber { get; private set; }
        /// <summary>
        /// The contents of the packet that was sent.
        /// </summary>
        public List<byte> PacketToSend { get; private set; }

        public PacketToSendEventArgs(int packetNumber, List<byte> packetToSend) {
            PacketNumber = packetNumber;
            PacketToSend = packetToSend;
        }

    }
}
