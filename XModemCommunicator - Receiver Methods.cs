using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        public void InitializeReceiver(XModemProtocolReceiverOptions options) {

        }

        private void Receive() {
            try {

            }
            catch(XModemProtocolException ex) {

            }

        }

        private void SendACK() {
            Port.Write(ACK);
            ResetConsecutiveNAKs();
        }

        private bool SendNAK() {
            Port.Write(NAK);
            return IncrementConsecutiveNAKs();

        }
    }
}