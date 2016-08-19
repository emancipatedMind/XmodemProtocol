namespace XModemProtocol {
    /// <summary>
    /// The mode of the XModemCommunicator instance.
    /// </summary>
    public enum XModemMode {
        /// <summary>
        /// Normal XModem mode.
        /// </summary>
        Checksum,
        /// <summary>
        /// 1024 packets w/ CRC
        /// </summary>
        OneK,
        /// <summary>
        /// 128 packets w/ CRC.
        /// </summary>
        CRC,
    }
}