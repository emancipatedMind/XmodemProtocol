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
        /// Have XModemCommunicator automatically select mode.
        /// </summary>
        OneK,
        /// <summary>
        /// 128 packets w/ CRC.
        /// </summary>
        CRC,
    }
}