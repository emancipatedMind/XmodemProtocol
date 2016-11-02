using System.Linq;

namespace XModemProtocol.Operations.Invoke {
    public class InvokeSend : Invoker {

        int _indexToBeSent;

        protected override void Invoke() {
            _context.State = XModemStates.SenderPacketsBeingSent;
            _indexToBeSent = 0;
            SendPackets();
        }

        private void SendPackets() {
            Send();
            while(NotCancelled) {
                if (ReadBufferContainsData) {
                    _buffer.Add(_context.Communicator.ReadSingleByte());
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
                    _buffer.AddRange(_context.Communicator.ReadAllBytes());
                    CheckForCancellation();
                }
            }
        }

        private bool EOTSent => _context.Packets.Count < _indexToBeSent;
        private bool AllPacketsSent => _context.Packets.Count == _indexToBeSent;
        private bool LastResponseWasACK => _buffer.Last() == _context.Options.ACK; 
        private bool LastResponseWasNAK => _buffer.Last() == _context.Options.NAK; 
        private bool ReadBufferContainsData => _context.Communicator.BytesInReadBuffer != 0;

        private void Send() {
            if (AllPacketsSent) {
                SendEOT();
                return;
            }
            FirePacketToSendEvent(); 
            _context.Communicator.Write(_context.Packets[_indexToBeSent]);
        }

        private void SendEOT() {
            _context.Communicator.Write(_context.Options.EOT);
        }

        protected void FirePacketToSendEvent() {
            base.FirePacketToSendEvent(_indexToBeSent + 1, _context.Packets[_indexToBeSent]);
        }
    }
}