namespace XModemProtocol {
    /// <summary>
    /// The enumeration that represents the reason the Aborted event fired.
    /// </summary>
    public enum XModemAbortReason {
        /// <summary>
        /// Timeout has during initialization or transfer.
        /// </summary>
        Timeout,
        /// <summary>
        /// Cancellation request was received.
        /// </summary>
        CancelRequestReceived,
        /// <summary>
        /// The amount of consecutive NAKs received has been exceeded.
        /// </summary>
        ConsecutiveNAKLimitExceeded,
        /// <summary>
        /// Operation was cancelled by user.
        /// </summary>
        UserCancelled,
        /// <summary>
        /// No bytes were found to be sent.
        /// </summary>
        NoBytesSupplied,
        /// <summary>
        /// Operation could not begin because State wasn't idle.
        /// </summary>
        StateNotIdle,
    }
}
