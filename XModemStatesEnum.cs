namespace XModemProtocol {
    /// <summary>
    /// The enumeration that represents the internal state of the XModemCommunicator instance.
    /// </summary>
    public enum XModemStates {
        /// <summary>
        /// The default state.
        /// </summary>
        Idle,
        /// <summary>
        /// Notes that XModemCommunicator has begun initializing.
        /// </summary>
        Initializing,
        /// <summary>
        /// Notes that XModemCommunicator's current operation has been cancelled. Performing cleanup.
        /// </summary>
        Cancelled,
        /// <summary>
        /// The instance is in the Sender role. Awaiting initialization from Receiver.
        /// </summary>
        SenderAwaitingInitializationFromReceiver,
        /// <summary>
        /// The instance is in the Sender role. Actively sending packets.
        /// </summary>
        SenderPacketsBeingSent,
        /// <summary>
        /// The instance is in the Receiver role. Sending the initialization byte.
        /// </summary>
        ReceiverSendingInitializationByte,
        /// <summary>
        /// The instance is in the Receiver role. Actively receiving packets. 
        /// </summary>
        ReceiverReceivingPackets,
        /// <summary>
        /// Notes that XModemCommunicator has finished its operation. If in the Sender role, it's either awaiting the final ACK or awaiting completion of the Completed event.
        /// If in the Receiver role, it has received the final ACK and is awaiting completion of the Completed event.
        /// </summary>
        PendingCompletion,
    }
}