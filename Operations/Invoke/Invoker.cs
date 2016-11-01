using System;
using System.Collections.Generic;

namespace XModemProtocol.Operations.Invoke {
    using EventData;
    using Exceptions;
    using Options;
    using Detectors;
    using Factories.Tools;
    public abstract class Invoker : IInvoker {
        protected IXModemTools _tools;
        protected IRequirements _requirements;
        protected ICancellationDetector _detector;
        protected List<byte> _buffer = new List<byte>();

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Invoke(IRequirements requirements) {
            _requirements = requirements;
            _detector = CancellationDetector.Instance;
            _tools = _requirements.ToolFactory.GetToolsFor(_requirements.Context.Mode); 
            Invoke();
        }

        protected abstract void Invoke();

        protected virtual void FirePacketToSendEvent (int packetNumber, List<byte> packet) {
            PacketToSend?.Invoke(this, new PacketToSendEventArgs(packetNumber, packet)); 
        }

        protected virtual void FirePacketReceivedEvent (int packetNumber, List<byte> packet) {
            PacketReceived?.Invoke(this, new PacketReceivedEventArgs(packetNumber, packet)); 
        }

        protected virtual void CheckForCancellation() {
            if(_detector.CancellationDetected(_buffer, _requirements.Options)) {
                _requirements.Context.State = XModemStates.Cancelled;
                throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
            }
        }

        protected virtual bool NotCancelled {
            get {
                if ( _requirements.Context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        protected virtual void Reset() {
            _buffer = new List<byte>();
        }

    }
}