namespace XModemProtocol {
    /// <summary>
    /// The enumeration that represents the reason the Aborted event fired.
    /// </summary>
    public enum XModemAbortReason {
        /// <summary>
        /// Timeout has occured during initialization or transfer.
        /// </summary>
        Timeout,
        /// <summary>
        /// The amount of consecutive NAKs received has been exceeded.
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
        /// Initialization has failed due to Exception in OperationPending eventhandler, or Receiver never got response.
        /// </summary>
        InitializationFailed,
    }
}
