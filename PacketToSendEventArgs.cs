using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.PacketToSend event.
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
        /// <summary>
        /// Initializes a new instance of the XModemProtocol.PacketToSendEventArgs class.
        /// </summary>
        /// <param name="packetNumber">The packet number to be sent.</param>
        /// <param name="packetToSend">The packet to be sent.</param>
        internal PacketToSendEventArgs(int packetNumber, List<byte> packetToSend) {
            PacketNumber = packetNumber;
            PacketToSend = packetToSend;
        }
    }
}