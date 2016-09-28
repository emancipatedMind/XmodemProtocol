using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Communication;
using XModemProtocol.EventData;
using XModemProtocol.Exceptions;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Operations;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Operations.Invoke;
using XModemProtocol.Options;

namespace XModemProtocol {
    public class XModemCommunicator {

        IRequirements _requirements;
        CancellationTokenSource _tokenSource;
        IToolFactory _toolFactory = new XModemToolFactory();
        IXModemTools _tools;
        IContext _context = new Context();
        ICommunicator _communicator;
        XModemRole _role;
        IOperation _operation;

        public XModemCommunicator(ICommunicator communicator) {
            _communicator = communicator;
            Options = new Options.XModemProtocolOptions();
            _context.PacketsBuilt += (s, e) => {
                Task.Run(()=> this.PacketsBuilt?.Invoke(this, e));
            };
            _context.StateUpdated += (s, e) => {
                Task.Run(()=> this.StateUpdated?.Invoke(this, e));
            };
        } 

        public XModemCommunicator(SerialPort port) : this(new Communicator(port))  {
        }

        public SerialPort Port {
            set {
                _communicator = new Communicator(value);     
            }
        }

        public void CancelOperation() {
            _tokenSource?.Cancel();
        }

        private void SendCancel() {
            _communicator.Write(Enumerable.Repeat(_options.CAN, _options.CANSentDuringAbort));
        }

        private void BuildPackets() {
            if (_context.Data.Count == 0) return;
            _context.Packets = _tools.Builder.GetPackets(_context.Data, Options);
        }

        public IEnumerable<byte> Data {
            set {
                if (value == null) return;
                else if (_context.Data == null || _context.Data.SequenceEqual(value) == false) {
                    _context.Data = new List<byte>(value);
                    BuildPackets();
                }
            }
        }

        public XModemStates State => _context.State;

        IXModemProtocolOptions _options;
        public IXModemProtocolOptions Options {
            private get { return _options; }
            set {
                if (value == null) value = new Options.XModemProtocolOptions();
                IXModemProtocolOptions oldOptions = _options;
                _options = (IXModemProtocolOptions)value.Clone();
                _options.ModeUpdated += (s, e) => {
                    Task.Run(() => this.ModeUpdated?.Invoke(this, e));
                };
                bool modeCheck = false;
                if (oldOptions == null || (modeCheck = oldOptions.Mode != _options.Mode) ) {
                    _tools = _toolFactory.GetToolsFor(_options.Mode);
                    if (modeCheck) BuildPackets();
                } 
            }
        }

        public void Send(IXModemProtocolOptions options) {
            _role = XModemRole.Sender;
            PerformOperation();
        }

        public void Receive(IXModemProtocolOptions options) {
            _role = XModemRole.Receiver;
            PerformOperation();
        }

        private void PerformOperation() {
            try {
                if (OperationPending != null) 
                    if (OperationPending() == false)
                        throw new Exceptions.XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelledByOperationPendingEvent));
                _tokenSource = new CancellationTokenSource();
                _context.Token = _tokenSource.Token;
                switch (_role) {
                    case XModemRole.Sender:
                        _operation = new SendOperation();
                        _operation.PacketToSend += (s, e) => {
                            PacketToSend?.Invoke(this, e);
                        };
                        if (_context.Data == null || _context.Data.Count == 0)
                            throw new Exceptions.XModemProtocolException(new AbortedEventArgs(XModemAbortReason.BufferEmpty)); 
                        break;
                    default:
                        return;
                }
                _requirements = new Requirements {
                    Communicator = _communicator,
                    Context = _context,
                    Options = _options,
                };
                _operation.Go(_requirements);
                Completed?.Invoke(this, new CompletedEventArgs(_context.Data));
            }
            catch (XModemProtocolException ex) {
                switch(ex.AbortArgs.Reason) {
                    case XModemAbortReason.Cancelled:
                    case XModemAbortReason.ConsecutiveNAKLimitExceeded:
                        SendCancel();
                        break;
                }
                Aborted.Invoke(this, new AbortedEventArgs(ex.AbortArgs.Reason));
            }
            finally {
                _context.State = XModemStates.Idle;
            }
        }

        #region Sender Only Events
        /// <summary>
        /// Used exclusively by Sender.
        /// This event fires asynchronously whenever XModemCommunicator finishes building packets.
        /// </summary>
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;

        /// <summary>
        /// Used exclusively by Sender.
        /// This event fires just before a packet is sent. A blocking method can prevent packet from being
        /// sent. Does not fire when sending EOT.
        /// </summary>
        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        #endregion

        #region Receiver Only Events
        /// <summary>
        /// Used exclusively by Receiver.
        /// This event fires after a successful packet has been received, and verified. This event must
        /// complete before XModemCommunicator will ACK sender. Does not fire when EOT is received.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        #endregion

        #region Shared Events
        /// <summary>
        /// This event fires whenever XModemCommunicator is about to start a send or receive operation.
        /// Must complete before operation begins.
        /// </summary>
        /// <returns>Whether to perform operation(true) or not(false).</returns>
        public event Func<bool> OperationPending;

        /// <summary>
        /// This event fires asynchronously whenever XModemCommunicator changes its state.
        /// </summary>
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        /// <summary>
        /// This event fires asynchronously whenever XModemCommunicator changes its mode.
        /// </summary>
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;

        /// <summary>
        /// This event fires when the operation has been aborted.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<AbortedEventArgs> Aborted;

        /// <summary>
        /// This event fires when the final ACK has been received or sent.
        /// State is not returned to idle until after this event has completed. 
        /// </summary>
        public event EventHandler<CompletedEventArgs> Completed;
        #endregion
    }
}