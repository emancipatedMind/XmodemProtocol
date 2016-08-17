using System;
using System.Collections.Generic;

namespace XModemProtocol {
    /// <summary>
    /// Options to be used when playing sender role. Buffer is the preferred property. If it is null
    /// XModemCommunicator will look to load the file specified by the Filename property. If that fails, 
    /// or is null, XModemCommunicator will check its own memory for bytes to send. If this also fails,
    /// the operation is aborted.
    /// </summary>
    public class XModemProtocolSenderOptions : XModemProtocolOptions, ICloneable {

        /// <summary>
        /// Create an instance of the XModemProtocolSenderOptions.
        /// </summary>
        public XModemProtocolSenderOptions() : this(new XModemProtocolOptions()) { }

        /// <summary>
        /// Create an instance of the XModemProtocolSenderOptions.
        /// </summary>
        /// <param name="baseOptions"></param>
        public XModemProtocolSenderOptions(XModemProtocolOptions baseOptions) {
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
        /// File to be loaded for use.
        /// </summary>
        public string Filename { get; set; } = null;

        /// <summary>
        /// To be used if Receiver needs to be prompted.
        /// </summary>
        public IEnumerable<byte> Prompt { get; set; } = null;

        /// <summary>
        /// Bytes to be used in Send operation.
        /// </summary>
        public IEnumerable<byte> Buffer { get; set; } = null;

        /// <summary>
        /// Timeout to be used for initialization. Zero and all non-positive integers means no timeout should occur.
        /// </summary>
        public int InitializationTimeout { get; set; } = 10000;

        /// <summary>
        /// A method to perform deep copy of instance.
        /// </summary>
        /// <returns>A deep copy of XModemProtocolSenderOptions.</returns>
        public object Clone() {
            return new XModemProtocolSenderOptions {
                Buffer = Buffer == null ? null : new List<byte>(Buffer),
                Prompt = Prompt == null ? null : new List<byte>(Prompt),
                Filename = Filename,
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