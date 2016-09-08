namespace XModemProtocol {
    /// <summary>
    /// The enumeration that represents the reason the Aborted event fired.
    /// </summary>
    public enum XModemAbortReason {
        /// <summary>
        /// Timeout has occured during transfer.
        /// </summary>
        Timeout,
        /// <summary>
        /// The amount of consecutive NAKs sent has been exceeded.
        /// </summary>
        ConsecutiveNAKLimitExceeded,
        /// <summary>
        /// Operation was cancelled by user.
        /// </summary>
        Cancelled,
        /// <summary>
        /// A cancellation request was detected on the line.
        /// </summary>
        CancellationRequestReceived,
        /// <summary>
        /// Buffer containing bytes to be sent is empty.
        /// </summary>
        BufferEmpty,
        /// <summary>
        /// Initialization has failed due to Exception in OperationPending eventhandler, Receiver never got response, or Sender never received initialization byte.
        /// Sender may be instructed to never use this abort by setting SenderInitializationTimeout to 0.
        /// </summary>
        InitializationFailed,
        /// <summary>
        /// Send or receive operation has failed due to some error.
        /// </summary>
        OperationFailed,
    }
}