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
        private List<List<byte>> _packets;

        public XModemStates State {
            get { return _state; }
            set {
                if (value == _state) return;
                XModemStates _oldState = _state;
                _state = value;
                StateUpdated?.Invoke(this, new StateUpdatedEventArgs(_state, _oldState));
            }
        }
        public bool BuildRequested { get; set; } = false;
        public CancellationToken Token { get; set; }

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

        public event EventHandler<StateUpdatedEventArgs> StateUpdated;
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;
    }
}