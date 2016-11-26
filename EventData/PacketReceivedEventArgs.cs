namespace XModemProtocol.EventData {
    using System.Collections.Generic;
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.PacketReceived event.
    /// </summary>
    public class PacketReceivedEventArgs : System.EventArgs {
        /// <summary>
        /// The packet number that was received.
        /// </summary>
        public int PacketNumber { get; private set; }
        /// <summary>
        /// The contents of the packet that was received.
        /// </summary>
        public List<byte> PacketReceived { get; private set; }
        internal PacketReceivedEventArgs(int packetNumber, List<byte> packetReceived) {
            PacketNumber = packetNumber;
            PacketReceived = new List<byte>(packetReceived);
        }
    }
}