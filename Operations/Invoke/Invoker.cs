using System;
using System.Collections.Generic;
using XModemProtocol.EventData;
using XModemProtocol.Exceptions;
using XModemProtocol.Options;

namespace XModemProtocol.Operations.Invoke {
    public abstract class Invoker : IInvoker {
        protected ISendReceiveRequirements _requirements;
        protected List<byte> _buffer = new List<byte>();

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Invoke(ISendReceiveRequirements requirements) {
            _requirements = requirements;
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
            if(_requirements.Detector.CancellationDetected(_buffer, _requirements.Options)) {
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