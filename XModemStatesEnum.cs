using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// The instance is in the Sender role. Awaiting initialization from Receiver.
        /// </summary>
        SenderAwaitingInitialization,
        /// <summary>
        /// The instance is in the Sender role. Actively sending packets.
        /// </summary>
        SenderPacketSent,
        /// <summary>
        /// The instance is in the Sender role. EOT has been sent. Awaiting acknowledgment.
        /// </summary>
        SenderAwaitingFinalACK,
    }
}