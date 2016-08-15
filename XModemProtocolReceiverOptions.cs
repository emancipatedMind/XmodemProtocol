using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public class XModemProtocolReceiverOptions : XModemProtocolOptions, ICloneable {

        int _initializationTimeout = 3000;

        /// <summary>
        /// The time between the initialization bytes sent to initialize transfer.
        /// This must be between 1000ms and 10000ms.
        /// </summary>
        public int InitializationTimeout {
            get {
                return _initializationTimeout;
            }
            set {
                if (_initializationTimeout < 1000)
                    _initializationTimeout = 1000;
                else if (_initializationTimeout > 10000)
                    _initializationTimeout = 10000;
                else _initializationTimeout = value;
            }
        }

        int _maxNumberOfInitializationBytesForCRC = 3;
        /// <summary>
        /// Maximum number of initialization bytes to send if using CRC
        /// before falling back to normal XModem. Must be between 1, and 10.
        /// </summary>
        public int MaxNumberOfInitializationBytesForCRC {
            get { return _maxNumberOfInitializationBytesForCRC; }
            set {
                if (_maxNumberOfInitializationBytesForCRC < 1)
                    _maxNumberOfInitializationBytesForCRC = 1;
                else if (_maxNumberOfInitializationBytesForCRC > 10)
                    _maxNumberOfInitializationBytesForCRC = 10;
                else _maxNumberOfInitializationBytesForCRC = value;
            }
        }

        int _maxNumberOfInitializationBytesInTotal = 10;
        /// <summary>
        /// Maximum number of initialization bytes to send in total.
        /// If using 1k, or CRC, this number is added to the total used
        /// by MaxNumberOfInitializationBytesForCRC. 5 minimum.
        /// </summary>
        public int MaxNumberOfInitializationBytesInTotal {
            get { return _maxNumberOfInitializationBytesInTotal; }
            set {
                if (_maxNumberOfInitializationBytesInTotal < 5)
                    _maxNumberOfInitializationBytesInTotal = 5;
                else _maxNumberOfInitializationBytesInTotal = value;
            }
        }

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
                MaxNumberOfInitializationBytesInTotal = MaxNumberOfInitializationBytesInTotal,
                MaxNumberOfInitializationBytesForCRC = MaxNumberOfInitializationBytesForCRC,
                InitializationTimeout = InitializationTimeout,
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