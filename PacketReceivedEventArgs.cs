using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.PacketReceived event.
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

        /// <summary>
        /// Flag which denotes whether the packet was validated.
        /// </summary>
        public bool PacketVerified { get; private set; }

        /// <summary>
        /// Initializes a new instance of the XModemProtocol.PacketReceivedEventArgs class.
        /// </summary>
        /// <param name="packetNumber">Packet number received.</param>
        /// <param name="packetReceived">Actual packet received.</param>
        /// <param name="packetVerified">Whether packet was verified.</param>
        internal PacketReceivedEventArgs(int packetNumber, List<byte> packetReceived, bool packetVerified) {
            PacketNumber = packetNumber;
            PacketReceived = packetReceived;
            PacketVerified = packetVerified;
        }

    }
}