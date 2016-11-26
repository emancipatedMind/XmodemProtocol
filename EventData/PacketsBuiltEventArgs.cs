namespace XModemProtocol.EventData {
    using System.Collections.Generic;
    /// <summary>
    /// Provides data for the XModemProtocol.XModemCommunicator.PacketsBuilt event.
    /// </summary>
    public class PacketsBuiltEventArgs : System.EventArgs {
        /// <summary>
        /// The new count of packets.
        /// </summary>
        public List<List<byte>> Packets { get; private set; }
        internal PacketsBuiltEventArgs(List<List<byte>> packets) {
            Packets = new List<List<byte>>(packets);
        }
    }
}