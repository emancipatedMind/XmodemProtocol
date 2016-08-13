using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public class XModemProtocolReceiverOptions : XModemProtocolOptions, ICloneable {

        public XModemProtocolReceiverOptions() { }

        public XModemProtocolReceiverOptions(XModemProtocolOptions baseOptions) {
                Mode = baseOptions.Mode;
                NAKBytesRequired = baseOptions.NAKBytesRequired;
                CancellationBytesRequired = baseOptions.CancellationBytesRequired;
                CANSentDuringAbort = baseOptions.CANSentDuringAbort;
                SOH = baseOptions.SOH;
                STX = baseOptions.STX;
                ACK = baseOptions.ACK;
                NAK = baseOptions.NAK;
                C = baseOptions.C;
                EOT = baseOptions.EOT;
                SUB = baseOptions.SUB;
                CAN = baseOptions.CAN;
                EOF = baseOptions.EOF;
        }

        public object Clone() {
            return new XModemProtocolReceiverOptions {
                Mode = Mode,
                NAKBytesRequired = NAKBytesRequired,
                CancellationBytesRequired = CancellationBytesRequired,
                CANSentDuringAbort = CANSentDuringAbort,
                SOH = SOH,
                STX = STX,
                ACK = ACK,
                NAK = NAK,
                C = C,
                EOT = EOT,
                SUB = SUB,
                CAN = CAN,
                EOF = EOF,
            };
        }
    }
}