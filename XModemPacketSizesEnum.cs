namespace XModemProtocol {
    /// <summary>
    /// The enumeration that represents the packet sizes.
    /// </summary>
    public enum XModemPacketSizes {
        /// <summary>
        /// Packet size used by XModem-CHKSUM and XModem-CRC.
        /// </summary>
        Standard = 128,
        /// <summary>
        /// Packet size used by XModem-1k.
        /// </summary>
        OneK = 1024,
    }
}
