using System;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// This event fires whenever XModemCommunicator finishes building packets.
        /// This event is fired asynchronously, and in a parallel manner if chained.
        /// </summary>
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;

        /// <summary>
        /// This event fires whenever XModemCommunicator is about to start a send or receive operation.
        /// Must complete before operation begins.
        /// </summary>
        public event Action OperationPending;

        /// <summary>
        /// This event fires when a packet is sent.
        /// This event is fired asynchronously, and in a parallel manner if chained.
        /// </summary>
        public event EventHandler<PacketSentEventArgs> PacketSent;

        /// <summary>
        /// This event fires when the internal state is updated.
        /// This event is fired asynchronously, and in a parallel manner if chained.
        /// </summary>
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        /// <summary>
        /// This event fires when the mode is updated.
        /// This event is fired asynchronously, and in a parallel manner if chained.
        /// </summary>
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;

        /// <summary>
        /// This event fires when the final ACK has been received or sent.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<CompletedEventArgs> Completed;

        /// <summary>
        /// This event fires when the operation has been aborted.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<AbortedEventArgs> Aborted;
    }
}
