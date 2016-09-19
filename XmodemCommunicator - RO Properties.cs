using System.Collections.Generic;
using System.Threading.Tasks;
using XModemProtocol.EventData;

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
            get {
                lock (_roleLockToken) { 
                    return _role;
                }
            }
            private set {
                lock (_roleLockToken) { 
                    XModemRole oldRole = _role;
                    _role = value;
                    if (_role == XModemRole.Receiver) {
                        _data = null;
                        Packets = null;
                        _rebuildPackets = false;
                    }
                    else if (_data == null) {
                        _role = XModemRole.Receiver;
                    } 
                    if (_role != oldRole) Task.Run(() => RoleUpdated?.Invoke(this, new RoleUpdatedEventArgs(_role, oldRole)));
                }
            }
        }

        /// <summary>
        /// Internal state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates State {
            get {
                lock (_stateLockToken) {
                    return _state;
                }
            }
            private set {
                lock (_stateLockToken) {
                    XModemStates oldState;
                    oldState = _state;
                    _state = value;
                    if (_state != oldState) Task.Run(() => StateUpdated?.Invoke(this, new StateUpdatedEventArgs(_state, oldState)));
                }
            }
        }
    }
}