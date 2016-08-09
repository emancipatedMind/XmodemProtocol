using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmodemProtocol {
    public partial class XModemCommunicator {

        public List<List<byte>> Packets { get; private set; } = null;
        public string Filename { get; private set; } = null;
        public int CancellationBytesRequired { get; private set; } = 5;

        public States State {
            get { return _state; }
            private set {
                if (_state == value) return;
                _state = value;
                StateUpdated?.Invoke(this, new StateUpdatedEventArgs(value));
                if (_state == States.Idle || _state == States.SenderAwaitingInitialization)
                    _mutationsAllowed = true;
                else
                    _mutationsAllowed = false;
            }
        }

        public PacketSizes PacketSize {
            get { return _packetSize; }
            private set {
                _packetSize = value;
                BuildPackets();
            }
        }

        public int PacketIndexToSend { get; private set; } 

        public int PacketCount {
            get { return _packetCount; }
            private set {
                if (_packetCount == value) return;
                _packetCount = value;
                PacketCountUpdated?.Invoke(this, new PacketCountUpdatedEventArgs(_packetCount));
            }
        }


        public XModemMode Mode {
            get { return _mode; }
            private set {
                if (!_mutationsAllowed) return;
                _mode = value;
                if (_mode == XModemMode.Checksum)
                    PacketSize = PacketSizes.Standard; 
                else
                    PacketSize = PacketSizes.OneK; 
            }
        }

    }
}