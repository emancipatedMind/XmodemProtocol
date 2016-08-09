using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmodemProtocol {
    public class PacketSentEventArgs : EventArgs {

        public int PacketNumber { get; private set; }
        public List<byte> PacketSent { get; private set; }

        public PacketSentEventArgs(int packetNumber, List<byte> packetSent) {
            PacketNumber = packetNumber;
            PacketSent = packetSent;
        }

    }
}
