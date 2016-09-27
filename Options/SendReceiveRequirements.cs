using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Communication;
using XModemProtocol.Detectors;
using XModemProtocol.Validators.Packet;

namespace XModemProtocol.Options {
    public class SendReceiveRequirements : ISendReceiveRequirements {
        public ICommunicator Communicator { get; set; }
        public IContext Context { get; set; }
        public ICancellationDetector Detector { get; set; }
        public IXModemProtocolOptions Options { get; set; }
        public IPacketValidator Validator { get; set; }
    }
}