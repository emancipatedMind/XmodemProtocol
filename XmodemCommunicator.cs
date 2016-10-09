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
        /// SerialPort to be used to create an instance of the
        /// XModemProtocol.Communication.Communicator class.
        /// </summary>
        public SerialPort Port { set { Communicator = new Communicator(value); } }
        /// <summary>
        /// Accepts an instance of a class that implements
        /// the XModemProtocol.Communication.ICommunicator interface.
        /// Object will be used to facilitate the transfer of bytes.
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
        /// Returns the current state of XModemProtocol.XModemCommunicator.
        /// </summary>
        public XModemStates State => _context.State;

        /// <summary>
        /// Accepts an instance of a class that implements the
        /// XModemProtocol.Options.IXModemProtocolOptions interface.
        /// This contains the bytes that XModemProtocol.XModemCommunicator
        /// will use to facilitate transfer along with some other options
        /// to customize how XModemProtocol.XModemCommunicator operates.
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
        /// Mode to be used by XModemProtocol.XModemCommunicator.
        /// If using Receive operation, CRC will upgrade to OneK automatically.
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
        /// Puts XModemProtocol.XModemCommuniator in the sender role
        /// awaiting initialization byte from receiver.
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
        /// Puts XModemProtocol.XModemCommuniator in the receiver role
        /// sending the initialization byte.
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

        /// <summary>
        /// Cancels operation currently running. No effect if no operation running. 
        /// </summary>
        public void CancelOperation() {
            _tokenSource?.Cancel();
        }
        #endregion

        #region Events

        #region Sender Only Events
        /// <summary>
        /// Fires asynchronously whenever XModemProtocol.XModemCommunicator finishes building packets.
        /// </summary>
        public event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;

        /// <summary>
        /// Fires when XModemProtocol.XModemCommunicator is ready to send a packet.
        /// A blocking method will prevent packet from being sent. Does not fire
        /// when sending IXModemProtocolOptions.EOT.
        /// </summary>
        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        #endregion

        #region Receiver Only Events
        /// <summary>
        /// Fires after a successful packet has been received by
        /// XModemProtocol.XModemCommunicator. This event must complete
        /// before XModemProtocol.XModemCommunicator will send
        /// IXModemProtocolOptions.ACK. Does not fire when IXModemProtocolOptions.EOT is received.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;
        #endregion

        #region Shared Events
        /// <summary>
        /// Must complete before operation begins.
        /// Fires before the operation begins, and determines whether
        /// operation will run or not. Will not fire if Data contains
        /// no bytes, and performing Send operation.
        /// </summary>
        /// <returns>Whether to perform operation(true) or not(false).</returns>
        public event Func<bool> OperationPending;

        /// <summary>
        /// Fires when the state of XModemProtocol.XModemCommunicator is updated.
        /// </summary>
        public event EventHandler<StateUpdatedEventArgs> StateUpdated;

        /// <summary>
        /// Fires when the mode of XModemProtocol.XModemCommunicator is updated.
        /// </summary>
        public event EventHandler<ModeUpdatedEventArgs> ModeUpdated;

        /// <summary>
        /// Fires if the operation is aborted. XModemProtocol.XModemCommunicator
        /// will not return to being idle until event completes.
        /// </summary>
        public event EventHandler<AbortedEventArgs> Aborted;

        /// <summary>
        /// Fires the operation completes successfully.
        /// XModemProtocol.XModemCommunicator will not return to
        /// being idle until event completes. 
        /// </summary>
        public event EventHandler<CompletedEventArgs> Completed;
        #endregion

        #endregion

        #region Support Methods
        private void SendCancel() {
            Communicator.Write(Enumerable.Repeat(_options.CAN, _options.CANSentDuringAbort));
        }

        private void BuildPackets() {
            if (_context.Data == null || _context.Data.Count == 0) return;
            _context.Packets = _tools.Builder.GetPackets(_context.Data, Options);
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
        #endregion
    }
}