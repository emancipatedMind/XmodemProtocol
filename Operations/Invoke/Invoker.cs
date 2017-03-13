using System;
using System.Collections.Generic;

namespace XModemProtocol.Operations.Invoke {
    using Environment;
    using EventData;
    using Exceptions;
    using Detectors;
    public abstract class Invoker : IInvoker {
        protected IContext _context;
        protected ICancellationDetector _detector;
        protected List<byte> _buffer = new List<byte>();

        public event EventHandler<PacketToSendEventArgs> PacketToSend;
        public event EventHandler<PacketReceivedEventArgs> PacketReceived;

        public void Invoke(IContext context) {
            _context = context;
            _detector = new CancellationDetector();
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
            if(_detector.CancellationDetected(_buffer, _context.Options)) {
                _context.State = XModemStates.Cancelled;
                throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
            }
        }

        protected virtual bool NotCancelled {
            get {
                if ( _context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        protected virtual void Reset() {
            _buffer = new List<byte>();
        }

    }
}