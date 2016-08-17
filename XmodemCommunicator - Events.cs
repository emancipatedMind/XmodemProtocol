using System;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Used exclusively by Sender.
        /// This event fires whenever XModemCommunicator finishes building packets.
        /// </summary>
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;

        /// <summary>
        /// This event fires whenever XModemCommunicator is about to start a send or receive operation.
        /// Must complete before operation begins.
        /// </summary>
        public event Action OperationPending;

        /// <summary>
        /// Used exclusively by Sender.
        /// This event fires just before a packet is sent. A blocking method can prevent packet from being
        /// sent.
        /// </summary>
        public event EventHandler<PacketToSendEventArgs> PacketToSend;

        /// <summary>
        /// Used exclusively by Receiver.
        /// This event fires after a successful packet has been received, and verified. This event must
        /// complete before XModemCommunicator will ACK sender.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        /// <summary>
        /// This event fires when the internal state is updated.
        /// </summary>
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        /// <summary>
        /// This event fires when the mode is updated.
        /// </summary>
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;

        /// <summary>
        /// This event fires when the final ACK has been received or sent.
        /// </summary>
        public event EventHandler<CompletedEventArgs> Completed;

        /// <summary>
        /// This event fires when the operation has been aborted.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<AbortedEventArgs> Aborted;
    }
}
