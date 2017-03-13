namespace XModemProtocol.Environment {
    using Communication;
    using Configuration;
    using EventData;
    using Factories;
    using Factories.Tools;
    using Options;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    public class Context : IContext {

        IXModemProtocolOptions _options = new XModemProtocolOptions();
        IToolFactory _toolFactory = new XModemToolFactory();

        private ICommunicator _communicator = new NullCommunicator();
        public ICommunicator Communicator {
            get { return _communicator; }
            set {
                if (value == null)
                    _communicator = new NullCommunicator();
                else
                    _communicator = value;
            }
        }

        public void SendCancel() => Communicator.Write(Enumerable.Repeat(Options.CAN, Options.CANSentDuringAbort));

        public int Polynomial {
            get { return _toolFactory.Polynomial; }
            set {
                if (State == XModemStates.Idle) 
                    _toolFactory.Polynomial = value;
            }
        }

        public IXModemProtocolOptions Options {
            get { return _options; }
            set {
                if (value == null) value = new XModemProtocolOptions();
                _options = value;
            }
        }

        public IXModemTools Tools {
            get { return _toolFactory.GetToolsFor(_mode); }
        }

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

        public List<List<byte>> Packets { get; } = new List<List<byte>>();

        public CancellationToken Token { get; set; }

        public void BuildPackets() {
            Packets.Clear();
            if (_data.Count > 1) {
                Packets.AddRange(Tools.Builder.GetPackets(_data, Options));
                PacketsBuilt?.Invoke(this, new PacketsBuiltEventArgs(Packets));
            }
        }

        private XModemMode _mode = XModemProtocolConfigurationSection.Settings.Mode.Value;
        public XModemMode Mode {
            get { return _mode; }
            set {
                if (_mode == value) return;
                var _oldMode = _mode;
                _mode = value;
                ModeUpdated?.Invoke(this, new ModeUpdatedEventArgs(_mode, _oldMode));
                BuildPackets();
            }
        }

        private List<byte> _data = new List<byte>();
        public List<byte> Data {
            get { return _data; }
            set {
                if (value == null) value = new List<byte>();
                if (_data.SequenceEqual(value)) return;
                _data = new List<byte>(value);
                BuildPackets();
            }
        }

        public event EventHandler<StateUpdatedEventArgs> StateUpdated;
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;
    }
}
