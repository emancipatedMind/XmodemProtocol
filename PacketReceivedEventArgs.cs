using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    /// <summary>
    /// The event data for the PacketReceived event.
    /// </summary>
    public class PacketReceivedEventArgs : EventArgs {
        /// <summary>
        /// The packet number that was received.
        /// </summary>
        public int PacketNumber { get; private set; }
        /// <summary>
        /// The contents of the packet that was received.
        /// </summary>
        public List<byte> PacketReceived { get; private set; }

        public PacketReceivedEventArgs(int packetNumber, List<byte> packetReceived) {
            PacketNumber = packetNumber;
            PacketReceived = packetReceived;
        }

    }
}