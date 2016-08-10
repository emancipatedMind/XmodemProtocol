using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// This event fires if the PacketCount property is changed.
        /// </summary>
        public event EventHandler<PacketCountUpdatedEventArgs> PacketCountUpdated;

        public event EventHandler<PacketSentEventArgs> PacketSent;

        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        public event EventHandler<CompletedEventArgs> Completed;

        private event Action ConsecutiveNAKLimitPassed;

        public event EventHandler<AbortedEventArgs> Aborted;
    }
}
