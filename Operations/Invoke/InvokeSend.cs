using System.Linq;

namespace XModemProtocol.Operations.Invoke {
    public class InvokeSend : Invoker {

        int _indexToBeSent;

        protected override void Invoke() {
            _requirements.Context.State = XModemStates.SenderPacketsBeingSent;
            _indexToBeSent = 0;
            SendPackets();
        }

        private void SendPackets() {
            Send();
            while(NotCancelled) {
                if (ReadBufferContainsData) {
                    _buffer.Add(_requirements.Communicator.ReadSingleByte());
                }
                else if (_buffer.Count != 0) { }
                else continue;

                if (LastResponseWasACK) {
                    _indexToBeSent++;
                    if (EOTSent) {
                        return;
                    }
                    Send();
                    Reset();
                }
                else if (LastResponseWasNAK) {
                    Send();
                    Reset();
                }
                else {
                    _buffer.AddRange(_requirements.Communicator.ReadAllBytes());
                    CheckForCancellation();
                }
            }
        }

        private bool EOTSent => _requirements.Context.Packets.Count < _indexToBeSent;
        private bool AllPacketsSent => _requirements.Context.Packets.Count == _indexToBeSent;
        private bool LastResponseWasACK => _buffer.Last() == _requirements.Options.ACK; 
        private bool LastResponseWasNAK => _buffer.Last() == _requirements.Options.NAK; 
        private bool ReadBufferContainsData => _requirements.Communicator.BytesInReadBuffer != 0;

        private void Send() {
            if (AllPacketsSent) {
                SendEOT();
                return;
            }
            FirePacketToSendEvent(); 
            _requirements.Communicator.Write(_requirements.Context.Packets[_indexToBeSent]);
        }

        private void SendEOT() {
            _requirements.Communicator.Write(_requirements.Options.EOT);
        }

        protected void FirePacketToSendEvent() {
            base.FirePacketToSendEvent(_indexToBeSent + 1, _requirements.Context.Packets[_indexToBeSent]);
        }
    }
}