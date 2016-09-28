using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Options;
using XModemProtocol.Communication;
using XModemProtocol.Exceptions;

namespace XModemProtocol.Operations.Invoke {
    public class InvokeSend : Invoker {

        int _indexToBeSent;

        protected override void Invoke() {
            _requirements.Context.State = XModemStates.SenderPacketsBeingSent;
            _indexToBeSent = 0;
            SendPackets();
        }

        private void SendPackets() {
            SendPacket();
            while(NotCancelled) {
                if (_requirements.Communicator.ReadBufferContainsData) {
                    _buffer.AddRange(_requirements.Communicator.ReadAllBytes());
                }
                else if (_buffer.Count != 0) { }
                else continue;

                CheckForCancellation();

                if(LastResponseWasACK) {
                    _indexToBeSent++;
                    if (LastPacketAlreadySent) {
                        SendEOT();
                        return;
                    }
                    SendPacket();
                    Reset();
                }
                else if (LastResponseWasNAK) {
                    SendPacket();
                    Reset();
                }

            }
        }

        private void CheckForCancellation() {
            if(_requirements.Detector.CancellationDetected(_buffer, _requirements.Options)) {
                _requirements.Context.State = XModemStates.Cancelled;
                throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.CancellationRequestReceived));
            }
        }

        private bool NotCancelled {
            get {
                if ( _requirements.Context.Token.IsCancellationRequested)
                    throw new XModemProtocolException(new EventData.AbortedEventArgs(XModemAbortReason.Cancelled));
                return true;
            }
        }

        private bool LastPacketAlreadySent => _requirements.Context.Packets.Count == _indexToBeSent;
        private bool LastResponseWasACK => _buffer.Last() == _requirements.Options.ACK; 
        private bool LastResponseWasNAK => _buffer.Last() == _requirements.Options.NAK; 

        private void SendPacket() {
            FirePacketToSendEvent(); 
            _requirements.Communicator.Write(_requirements.Context.Packets[_indexToBeSent]);
        }

        private void Reset() {
            _buffer = new List<byte>();
        }

        private void SendEOT() {
            _requirements.Communicator.Write(_requirements.Options.EOT);
        }

        protected void FirePacketToSendEvent() {
            base.FirePacketToSendEvent(_indexToBeSent + 1, _requirements.Context.Packets[_indexToBeSent]);
        }
    }
}