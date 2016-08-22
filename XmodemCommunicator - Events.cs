using System;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        #region Sender Only Events
        /// <summary>
        /// Used exclusively by Sender.
        /// This event fires asynchronously whenever XModemCommunicator finishes building packets.
        /// </summary>
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;

        /// <summary>
        /// Used exclusively by Sender.
        /// This event fires just before a packet is sent. A blocking method can prevent packet from being
        /// sent. Does not fire when sending EOT.
        /// </summary>
        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        #endregion

        #region Receiver Only Events
        /// <summary>
        /// Used exclusively by Receiver.
        /// This event fires after a successful packet has been received, and verified. This event must
        /// complete before XModemCommunicator will ACK sender. Does not fire when EOT is received.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        #endregion

        #region Shared Events
        /// <summary>
        /// This event fires whenever XModemCommunicator is about to start a send or receive operation.
        /// Must complete before operation begins.
        /// </summary>
        public event Action OperationPending;

        /// <summary>
        /// This event fires asynchronously whenever XModemCommunicator changes its state.
        /// </summary>
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        /// <summary>
        /// This event fires asynchronously whenever XModemCommunicator changes its mode.
        /// </summary>
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;

        /// <summary>
        /// This event fires asynchronously whenever XModemCommunicator changes its role.
        /// </summary>
        public event EventHandler<RoleChangedEventArgs> RoleUpdated;

        /// <summary>
        /// This event fires when the operation has been aborted.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<AbortedEventArgs> Aborted;

        /// <summary>
        /// This event fires when the final ACK has been received or sent.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<CompletedEventArgs> Completed;
        #endregion
    }
}