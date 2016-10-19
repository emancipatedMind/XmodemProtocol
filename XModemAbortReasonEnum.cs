namespace XModemProtocol {
    /// <summary>
    /// The enumeration that represents the reason the Aborted event fired.
    /// </summary>
    public enum XModemAbortReason {
        /// <summary>
        /// Buffer containing bytes to be sent is empty.
        /// </summary>
        BufferEmpty,
        /// <summary>
        /// A cancellation request was detected on the line.
        /// </summary>
        CancellationRequestReceived,
        /// <summary>
        /// Operation was cancelled by user.
        /// </summary>
        Cancelled,
        /// <summary>
        /// Operation was foregone by OperationPendingEvent returning false.
        /// </summary>
        CancelledByOperationPendingEvent,
        /// <summary>
        /// Operation cannot run. Communication channel is null.
        /// </summary>
        CommunicatorIsNull,
        /// <summary>
        /// The amount of consecutive NAKs sent has been exceeded.
        /// </summary>
        ConsecutiveNAKLimitExceeded,
        /// <summary>
        /// Initialization has failed because Receiver never got response, or Sender never received initialization byte.
        /// Sender may be instructed to never use this abort by setting SenderInitializationTimeout to 0.
        /// </summary>
        InitializationFailed,
        /// <summary>
        /// Send or receive operation has failed due to some error.
        /// </summary>
        OperationFailed,
        /// <summary>
        /// Operation could not begin because State was not idle.
        /// </summary>
        StateNotIdle,
        /// <summary>
        /// Timeout has occured during transfer.
        /// </summary>
        Timeout,
    }
}