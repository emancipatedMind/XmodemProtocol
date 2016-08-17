using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    /// <summary>
    /// Options used when playing receiver role.
    /// </summary>
    public class XModemProtocolReceiverOptions : XModemProtocolOptions, ICloneable {

        int _initializationTimeout = 3000;
        /// <summary>
        /// Default : 3000ms.
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


        int _receiverTimeoutForPacketReception = 10000;
        /// <summary>
        /// Default : 10000ms.
        /// After sending a packet, this is the amount of time receiver will wait for a packet before a NAK is sent to prompt sender.
        /// Must be between 1000 and 20000ms.
        /// </summary>
        public int ReceiverTimeoutForPacketReception {
            get {
                return _receiverTimeoutForPacketReception;
            }
            set {
                if (_receiverTimeoutForPacketReception < 5000)
                    _receiverTimeoutForPacketReception = 5000;
                else if (_receiverTimeoutForPacketReception > 20000)
                    _receiverTimeoutForPacketReception = 20000;
                else _receiverTimeoutForPacketReception = value;
            }
        }

        int _maxNumberOfInitializationBytesForCRC = 3;
        /// <summary>
        /// Default : 3.
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
        /// Default : 10.
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

        /// <summary>
        /// Create an instance of the XModemProtocolReceiverOptions.
        /// </summary>
        public XModemProtocolReceiverOptions() : this(new XModemProtocolOptions()) {
        }

        /// <summary>
        /// Create an instance of the XModemProtocolReceiverOptions.
        /// </summary>
        /// <param name="baseOptions">Common options.</param>
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

        /// <summary>
        /// A method to perform deep copy of instance.
        /// </summary>
        /// <returns>A deep copy of XModemProtocolReceiverOptions.</returns>
        public object Clone() {
            return new XModemProtocolReceiverOptions {
                ReceiverTimeoutForPacketReception = ReceiverTimeoutForPacketReception,
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