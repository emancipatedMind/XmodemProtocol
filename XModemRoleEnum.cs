namespace XModemProtocol {
    /// <summary>
    /// Denotes what role the XModemCommunicator instance is currently playing, or last played.
    /// </summary>
    public enum XModemRole {
        /// <summary>
        /// Notes that the instance has not yet performed a Send or Receiver operation.
        /// </summary>
        None,
        /// <summary>
        /// Represents the Sender role.
        /// </summary>
        Sender,
        /// <summary>
        /// Represents the Receiver role.
        /// </summary>
        Receiver,
    }
}