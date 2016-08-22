using System.Collections.Generic;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Packets received or sent.
        /// </summary>
        public List<List<byte>> Packets { get; private set; } = null;

        /// <summary>
        /// Denotes what role is currently being played or was last played by instance.
        /// </summary>
        public XModemRole Role {
            get { return _role; }
            private set {
                if (_role == value) return;
                XModemRole oldRole = _role;
                _role = value;
                Task.Run(() => RoleChanged?.Invoke(this, new RoleChangedEventArgs(_role, oldRole)));
            }
        }

        /// <summary>
        /// Internal state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates State {
            get {
                lock (this) {
                    return _state;
                }
            }
            private set {
                if (_state == value) return;
                XModemStates oldState;
                lock (this) {
                    oldState = _state;
                    _state = value;
                }
                Task.Run(() => StateUpdated?.Invoke(this, new StateUpdatedEventArgs(_state, oldState)));
            }
        }

    }
}