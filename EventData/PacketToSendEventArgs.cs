namespace XModemProtocol.EventData {
    using System;
    using System.Collections.Generic;
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.PacketToSend event.
    /// </summary>
    public class PacketToSendEventArgs : System.EventArgs {
        /// <summary>
        /// The packet number that was sent.
        /// </summary>
        public int PacketNumber { get; private set; }
        /// <summary>
        /// The contents of the packet that was sent.
        /// </summary>
        public List<byte> PacketToSend { get; private set; }
        internal PacketToSendEventArgs(int packetNumber, List<byte> packetToSend) {
            PacketNumber = packetNumber;
            PacketToSend = new List<byte>(packetToSend);
        }
    }
}