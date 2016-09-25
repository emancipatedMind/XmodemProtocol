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
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Operations.Invoke;
using XModemProtocol.Options;

namespace XModemProtocol {
    public class XModemBase {

        public XModemBase(SerialPort port) {
            Port = port;
        }

        IXModemProtocolOptions _options = new Options.XModemProtocolOptions();
        IRequirements _requirements;
        CancellationTokenSource _tokenSource;
        IToolFactory _toolFactory = new XModemToolFactory();
        IContext _context = new Context();
        List<byte> _data;
        ICommunicator _communicator;
        IInitializer _initializer;
        IInvoker _invoker;

        SerialPort Port { get; set; }

        public void CancelOperation() {
            _tokenSource?.Cancel();
        }

        private void SendCancel() {
            _communicator.Write(Enumerable.Repeat(_options.CAN, _options.CANSentDuringAbort));
        }

        public IEnumerable<byte> Data {
            set {
                if (value == null) return;
                else if (_data == null || !_data.SequenceEqual(value)) {
                    _data = value.ToList();
                }
            }
        }

        public XModemStates State {
            get {
                return _context.State;
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
        /// This event fires asynchronously whenever XModemCommunicator changes its role.
        /// </summary>
        public event EventHandler<RoleUpdatedEventArgs> RoleUpdated;

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