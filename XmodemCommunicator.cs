namespace XModemProtocol {
    using Communication;
    using EventData;
    using Exceptions;
    using Operations;
    using Options;
    using System;
    using System.Collections.Generic;
    using System.IO.Ports;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    public class XModemCommunicator {

        #region Fields
        IContext _context = new Context();
        CancellationTokenSource _tokenSource;
        IXModemProtocolOptions _copy;
        #endregion

        #region Constructors
        public XModemCommunicator() {
            _context.PacketsBuilt += (s, e) => {
                Task.Run(()=> PacketsBuilt?.Invoke(this, e));
            };
            _context.StateUpdated += (s, e) => {
                Task.Run(()=> StateUpdated?.Invoke(this, e));
            };
            _context.ModeUpdated += (s, e) => {
                Task.Run(()=> ModeUpdated?.Invoke(this, e));
            };
            _copy = _context.Options.Clone();
        }

        public XModemCommunicator(SerialPort port) : this(new Communicator(port)) { }
        public XModemCommunicator(ICommunicator communicator) : this() {
            Communicator = communicator;
        } 
        #endregion

        #region Properties
        /// <summary> 
        /// Polynomial used CRC lookup table. Defaulted to the polynomial
        /// X^16 + X^12 + X^5 + 1 (0x1021).
        /// </summary>
        public int Polynomial {
            get { return _context.Polynomial; }
            set { _context.Polynomial = value; }
        }

        /// <summary> 
        /// SerialPort to be used to create an instance of the
        /// XModemProtocol.Communication.Communicator class.
        /// </summary>
        public SerialPort Port {
            set {
                if (value == null)
                    _context.Communicator = null;
                else
                    _context.Communicator = new Communicator(value);
            }
        }

        /// <summary>
        /// Accepts an instance of a class that implements
        /// the XModemProtocol.Communication.ICommunicator interface.
        /// Object will be used to facilitate the transfer of bytes.
        /// </summary>
        public ICommunicator Communicator { set { _context.Communicator = value; } }

        /// <summary>
        /// Data received from transfer or data to be sent.
        /// </summary>
        public IEnumerable<byte> Data {
            set { _context.Data = value?.ToList(); }
            get { return new List<byte>(_context.Data); }
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
        /// By default, is instance of
        /// XModemProtocol.Options.XModemProtocolOptions.
        /// </summary>
        public IXModemProtocolOptions Options {
            set {
                _copy = value;
                _context.Options = value;
            }
            get { return _copy; }
        }

        /// <summary>
        /// Mode to be used by XModemProtocol.XModemCommunicator.
        /// If using Receive operation, CRC will upgrade to OneK automatically.
        /// </summary>
        public XModemMode Mode {
            get { return _context.Mode; }
            set { _context.Mode = value; }
        }
        #endregion

        #region Operations
        /// <summary>
        /// Puts XModemProtocol.XModemCommuniator in the sender role
        /// awaiting initialization byte from receiver.
        /// </summary>
        public void Send() => PerformOperation(SendSetup);

        /// <summary>
        /// Puts XModemProtocol.XModemCommuniator in the receiver role
        /// sending the initialization byte.
        /// </summary>
        public void Receive() => PerformOperation(ReceiveSetup);

        /// <summary>
        /// Cancels operation currently running. No effect if no operation running. 
        /// </summary>
        public void CancelOperation() => _tokenSource?.Cancel();
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
        private void SendCancel() => _context.SendCancel();

        private IOperation SendSetup() {
            IOperation operation = new SendOperation();
            operation.PacketToSend += (s, e) => PacketToSend?.Invoke(this, e);
            if (_context.Data == null || _context.Data.Count == 0) 
                throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.BufferEmpty));
            return operation;
        }

        private IOperation ReceiveSetup() {
            IOperation operation = new ReceiveOperation();
            operation.PacketReceived += (s, e) => PacketReceived?.Invoke(this, e);
            _context.Data = new List<byte>();
            return operation;
        }

        private void PerformOperation(Func<IOperation> setup)  {
            try {
                if (_context.State != XModemStates.Idle) 
                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.StateNotIdle));
                if (_context.Communicator is NullCommunicator) 
                    throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CommunicatorIsNull));
                IOperation operation = setup();
                if (OperationPending != null && OperationPending() == false)
                        throw new XModemProtocolException(new AbortedEventArgs(XModemAbortReason.CancelledByOperationPendingEvent));
                _tokenSource = new CancellationTokenSource();
                _context.Token = _tokenSource.Token;
                _context.Communicator.Flush();
                operation.Go(_context);
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

        #region Finalizer 
        ~XModemCommunicator() {
            CancelOperation();
        }
        #endregion

    }
}
