using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.EventData;

namespace XModemProtocol.Options {
    public class Context : IContext {

        private XModemStates _state = XModemStates.Idle;
        public XModemStates State {
            get { return _state; }
            set {
                if (value == _state) return;
                XModemStates _oldState = _state;
                _state = value;
                StateUpdated?.Invoke(this, new StateUpdatedEventArgs(_state, _oldState));
            }
        }

        private List<List<byte>> _packets;
        public List<List<byte>> Packets {
            get {
                return _packets;
            }

            set
            {
                if (value == null) return;
                else if (_packets == null || !_packets.SequenceEqual(value)) {
                    _packets = value;
                    PacketsBuilt?.Invoke(this, new PacketsBuiltEventArgs(_packets));
                }
            }
        }

        public List<byte> Data { get; set; }
        public CancellationToken Token { get; set; }

        private XModemMode _mode = XModemMode.OneK;
        public XModemMode Mode {
            get { return _mode; }
            set {
                if (_mode == value) return;
                var _oldMode = _mode;
                _mode = value;
                ModeUpdated?.Invoke(this, new ModeUpdatedEventArgs(_mode, _oldMode));
            }
        }

        public event EventHandler<StateUpdatedEventArgs> StateUpdated;
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;
    }
}