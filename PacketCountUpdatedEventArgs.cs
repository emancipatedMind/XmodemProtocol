using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public class PacketCountUpdatedEventArgs : EventArgs {
        public int PacketCount { get; private set; }

        public PacketCountUpdatedEventArgs(int packetCount) {
            PacketCount = packetCount;
        }
    }
}