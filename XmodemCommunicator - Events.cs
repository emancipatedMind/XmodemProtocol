using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// This event fires whenever XModemCommunicator finishes building packets.
        /// </summary>
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt ;

        /// <summary>
        /// This event fires when a packet is sent.
        /// </summary>
        public event EventHandler<PacketSentEventArgs> PacketSent;

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
        /// </summary>
        public event EventHandler<AbortedEventArgs> Aborted;
    }
}
