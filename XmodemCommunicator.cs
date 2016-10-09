using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Communication;
using XModemProtocol.EventData;
using XModemProtocol.Exceptions;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Operations;
using XModemProtocol.Options;

namespace XModemProtocol
{
    public class XModemCommunicator {

        #region Fields
        IToolFactory _toolFactory = new XModemToolFactory();
        IContext _context = new Context();
        IRequirements _requirements;
        CancellationTokenSource _tokenSource;
        IXModemTools _tools;
        IOperation _operation;
        IXModemProtocolOptions _options;
        #endregion

        #region Constructors
        public XModemCommunicator(ICommunicator communicator) {
            Communicator = communicator;
            Options = new XModemProtocolOptions();
            _context.PacketsBuilt += (s, e) => {
                Task.Run(()=> PacketsBuilt?.Invoke(this, e));
            };
            _context.StateUpdated += (s, e) => {
                Task.Run(()=> StateUpdated?.Invoke(this, e));
            };
            _context.ModeUpdated += (s, e) => {
                Task.Run(()=> ModeUpdated?.Invoke(this, e));
            };
        } 

        public XModemCommunicator(SerialPort port) : this(new Communicator(port)) { }
        #endregion

        #region Properties
        /// <summary>
        /// Port passed to channel to facilitate transfer.
        /// </summary>
        public SerialPort Port { set { Communicator = new Communicator(value); } }
        /// <summary>
        /// Channel used to facilitate transfer.
        /// </summary>
        public ICommunicator Communicator { private get; set; }

        /// <summary>
        /// Data received from transfer or data to be sent.
        /// </summary>
        public IEnumerable<byte> Data
        {
            set
            {
                if (value == null) return;
                if (_context.Data == null || _context.Data.SequenceEqual(value) == false)
                {
                    _context.Data = new List<byte>(value);
                    BuildPackets();
                }
            }
            get
            {
                if (_context.Data == null || _context.Data.Count < 1) return null;
                return new List<byte>(_context.Data);
            }
        }

        /// <summary>
        /// Current state.
        /// </summary>
        public XModemStates State => _context.State;

        /// <summary>
        /// Options used during transfer.
        /// </summary>
        public IXModemProtocolOptions Options {
            private get { return _options; }
            set {
                if (value == null) value = new Options.XModemProtocolOptions();
                IXModemProtocolOptions oldOptions = _options;
                _options = (IXModemProtocolOptions)value.Clone();
                if (oldOptions == null) {
                    _tools = _toolFactory.GetToolsFor(_context.Mode);
                }
            }
        }

        /// <summary>
        /// Mode to be used by XModemCommunicator.
        /// If using receiver, CRC will be upgraded to 1k automatically.
        /// </summary>
        public XModemMode Mode {
            get { return _context.Mode; }
            set {
                if (_context.Mode == value) return;
                _context.Mode = value;
                BuildPackets();
            }
        }

        #endregion

        #region Operations
        /// <summary>
        /// Send Operation. Send the bytes contained in Data.
        /// </summary>
        public void Send() {
            if (_context.State != XModemStates.Idle) return;
            _operation = new SendOperation();
            _operation.PacketToSend += (s, e) =>
            {
                PacketToSend?.Invoke(this, e);
            };
            if (_context.Data == null || _context.Data.Count == 0)
            {
                Aborted?.Invoke(this, new AbortedEventArgs(XModemAbortReason.BufferEmpty));
                _context.State = XModemStates.Idle;
                return;
            }
            PerformOperation();
        }

        /// <summary>
        /// Receive Operation. Receive bytes from awaiting sender.
        /// </summary>
        public void Receive() {
            if (_context.State != XModemStates.Idle) return;
            _operation = new ReceiveOperation();
            _operation.PacketReceived += (s, e) =>
            {
                PacketReceived?.Invoke(this, e);
            };
            _context.Data = new List<byte>();
            _context.Packets = new List<List<byte>>();
            PerformOperation();
        }

        private void PerformOperation()  {
            try {
                if (OperationPending != null) 
                    if (OperationPending() == false)
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelledByOperationPendingEvent));
                _tokenSource = new CancellationTokenSource();
                _context.Token = _tokenSource.Token;
                _requirements = new Requirements {
                    Communicator = Communicator,
                    Context = _context,
                    Options = _options,
                };
                Communicator.Flush();
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
                Aborted?.Invoke(this, new AbortedEventArgs(ex.AbortArgs.Reason));
            }
            finally {
                _context.State = XModemStates.Idle;
            }
        }

        /// <summary>
        /// Used to cancel operation in progress.
        /// </summary>
        public void CancelOperation() {
            _tokenSource?.Cancel();
        }
        #endregion

        #region Events

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

        #endregion

        #region Support Methods
        private void SendCancel()
        {
            Communicator.Write(Enumerable.Repeat(_options.CAN, _options.CANSentDuringAbort));
        }

        private void BuildPackets()
        {
            if (_context.Data == null || _context.Data.Count == 0) return;
            _context.Packets = _tools.Builder.GetPackets(_context.Data, Options);
        }
        #endregion
    }
}