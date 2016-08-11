using System.Collections.Generic;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Packets received or sent.
        /// </summary>
        public List<List<byte>> Packets { get; private set; } = null;

        /// <summary>
        /// File name of contents in buffer. Only used when sending.
        /// </summary>
        public string Filename { get; private set; } = null;

        /// <summary>
        /// Internal state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates State {
            get { return _state; }
            private set {
                if (_state == value) return;
                XModemStates oldState = _state;
                _state = value;
                if (_state == XModemStates.Idle || _state == XModemStates.SenderAwaitingInitializationFromReceiver)
                    _mutationsAllowed = true;
                else
                    _mutationsAllowed = false;
                // StateUpdated?.Invoke(this, new StateUpdatedEventArgs(_state, oldState));
                if (StateUpdated != null) 
                    Parallel.ForEach(StateUpdated.GetInvocationList(), d => {
                        d.DynamicInvoke(new object[] {this, new StateUpdatedEventArgs(_state, oldState) });
                    });
            }
        }

        /// <summary>
        /// Packet size being used.
        /// </summary>
        public XModemPacketSizes PacketSize {
            get {
                return _packetSize;
            }
            private set {
                if (_packetSize == value) return;

                if (Mode == XModemMode.Checksum) {
                    _packetSize = XModemPacketSizes.Standard;
                    return;
                } 
                else {
                    _packetSize = value;
                }
            }
        }

        /// <summary>
        /// Mode of instance.
        /// </summary>
        public XModemMode Mode {
            get { return _mode; }
            private set {
                if (_mode == value) return;
                XModemMode oldMode = _mode;
                _mode = value;
                if (_mode == XModemMode.Checksum)
                    PacketSize = XModemPacketSizes.Standard; 
                //ModeUpdated?.Invoke(this, new ModeUpdatedEventArgs(_mode, oldMode));
                if (ModeUpdated != null) 
                    Parallel.ForEach(ModeUpdated.GetInvocationList(), d => {
                        d.DynamicInvoke(new object[] {this, new ModeUpdatedEventArgs(_mode, oldMode) });
                    });
            }
        }

    }
}