using System;
using System.Collections.Generic;
namespace XModemProtocol {
    /// <summary>
    /// Class used to hold options used by both the Sender, and Receiver.
    /// </summary>
    public class XModemProtocolOptions : ICloneable {

        #region Shared Options
        /// <summary>
        /// Shared option.
        /// Mode to be used by XModemCommunicator.
        /// If using receiver, CRC will be upgraded to 1k automatically.
        /// </summary>
        public XModemMode Mode { get; set; } = XModemMode.OneK;

        /// <summary>
        /// Shared option.
        /// Number of CANs sent during an abort.
        /// </summary>
        public int CANSentDuringAbort { get; set; } = 5;

        /// <summary>
        /// Shared option.
        /// Number of consecutive CANs that will prompt an abort.
        /// </summary>
        public int CancellationBytesRequired { get; set; } = 5;
        #endregion

        #region XModem Bytes
        /// <summary>
        /// Default: 0x01.
        /// Shared option.
        /// Sender begins each 128-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte SOH { get; set; } = 0x01;

        /// <summary>
        /// Default: 0x02.
        /// Shared option.
        /// Sender begins each 1024-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte STX { get; set; } = 0x02;

        /// <summary>
        /// Default: 0x06.
        /// Shared option.
        /// Receiver sends this to indicate packet was received successfully with no errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte ACK { get; set; } = 0x06;

        /// <summary>
        /// Default: 0x15.
        /// Shared option.
        /// Receiver sends this to initiate XModem-Checksum file transfer -- OR -- indicate packet errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte NAK { get; set; } = 0x15;

        /// <summary>
        /// Default: 0x43.
        /// Shared option.
        /// Receiver sends this to initiate XModem-CRC or XModem-1K file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte C { get; set; } = 0x43;

        /// <summary>
        /// Default: 0x04.
        /// Shared option.
        /// Sender sends this to mark the end of file. Receiver must acknowledge receipt of this byte with <ACK>, otherwise Sender resends <EOT> .
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte EOT { get; set; } = 0x04;

        /// <summary>
        /// Default: 0x1A.
        /// Shared option.
        /// This is used as a padding byte in the original specification.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte SUB { get; set; } = 0x1A;

        /// <summary>
        /// Default: 0x18.
        /// Shared option.
        /// [Commonly used but unofficial] Sender or Receiver sends this byte to abort file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte CAN { get; set; } = 0x18;
        #endregion

        #region Sender Options
        /// <summary>
        /// Default : 10000ms.
        /// Used exclusively by Sender.
        /// Timeout while waiting for initialization byte. Zero and all negative integers means no timeout should occur.
        /// </summary>
        public int SenderInitializationTimeout { get; set; } = 10000;
        #endregion

        #region ReceiverOptions
        /// <summary>
        /// Default : 5.
        /// Used exclusively by Receiver.
        /// Number of consecutive NAKs that will prompt an abort.
        /// </summary>
        public int ReceiverConsecutiveNAKsRequiredForCancellation { get; set; } = 5;

        int _receiverInitializationTimeout = 3000;
        /// <summary>
        /// Default : 3000ms.
        /// Used exclusively by Receiver.
        /// The time between the initialization bytes sent to initialize transfer.
        /// This must be between 1000ms and 10000ms.
        /// </summary>
        public int ReceiverInitializationTimeout {
            get {
                return _receiverInitializationTimeout;
            }
            set {
                if (_receiverInitializationTimeout < 1000)
                    _receiverInitializationTimeout = 1000;
                else if (_receiverInitializationTimeout > 10000)
                    _receiverInitializationTimeout = 10000;
                else _receiverInitializationTimeout = value;
            }
        }


        int _receiverTimeoutDuringPacketReception = 10000;
        /// <summary>
        /// Default : 10000ms.
        /// Used exclusively by Receiver.
        /// After sending a packet, this is the amount of time receiver will wait for a packet before NAKing sender in case response was lost.
        /// Must be between 1000 and 20000ms.
        /// </summary>
        public int ReceiverTimeoutDuringPacketReception {
            get {
                return _receiverTimeoutDuringPacketReception;
            }
            set {
                if (_receiverTimeoutDuringPacketReception < 5000)
                    _receiverTimeoutDuringPacketReception = 5000;
                else if (_receiverTimeoutDuringPacketReception > 20000)
                    _receiverTimeoutDuringPacketReception = 20000;
                else _receiverTimeoutDuringPacketReception = value;
            }
        }

        int _receiverMaxNumberOfInitializationBytesForCRC = 3;
        /// <summary>
        /// Default : 3.
        /// Used exclusively by Receiver.
        /// Maximum number of initialization bytes to send if using CRC
        /// before falling back to normal XModem. Must be between 1, and 10.
        /// </summary>
        public int ReceiverMaxNumberOfInitializationBytesForCRC {
            get { return _receiverMaxNumberOfInitializationBytesForCRC; }
            set {
                if (_receiverMaxNumberOfInitializationBytesForCRC < 1)
                    _receiverMaxNumberOfInitializationBytesForCRC = 1;
                else if (_receiverMaxNumberOfInitializationBytesForCRC > 10)
                    _receiverMaxNumberOfInitializationBytesForCRC = 10;
                else _receiverMaxNumberOfInitializationBytesForCRC = value;
            }
        }

        int _receiverMaxNumberOfInitializationBytesInTotal = 10;
        /// <summary>
        /// Default : 10.
        /// Used exclusively by Receiver.
        /// Maximum number of initialization bytes to send in total.
        /// If using 1k, this includes the total used
        /// by ReceiverMaxNumberOfInitializationBytesForCRC. 5 minimum.
        /// </summary>
        public int ReceiverMaxNumberOfInitializationBytesInTotal {
            get { return _receiverMaxNumberOfInitializationBytesInTotal; }
            set {
                if (_receiverMaxNumberOfInitializationBytesInTotal < 5)
                    _receiverMaxNumberOfInitializationBytesInTotal = 5;
                else _receiverMaxNumberOfInitializationBytesInTotal = value;
            }
        }
        #endregion

        /// <summary>
        /// A method to perform deep copy of instance.
        /// </summary>
        /// <returns>A deep copy of XModemProtocolReceiverOptions.</returns>
        public object Clone() {
            return new XModemProtocolOptions {
                // Shared Options
                Mode = Mode,
                ReceiverConsecutiveNAKsRequiredForCancellation = ReceiverConsecutiveNAKsRequiredForCancellation,
                CANSentDuringAbort = CANSentDuringAbort,
                CancellationBytesRequired = CancellationBytesRequired,

                // XMODEM Bytes.
                SOH = SOH,
                STX = STX,
                ACK = ACK,
                NAK = NAK,
                C = C,
                EOT = EOT,
                SUB = SUB,
                CAN = CAN,

                // Sender Options
                SenderInitializationTimeout = SenderInitializationTimeout,

                // Receiver Options
                ReceiverInitializationTimeout = ReceiverInitializationTimeout,
                ReceiverTimeoutDuringPacketReception = ReceiverTimeoutDuringPacketReception,
                ReceiverMaxNumberOfInitializationBytesForCRC = ReceiverMaxNumberOfInitializationBytesForCRC,
                ReceiverMaxNumberOfInitializationBytesInTotal = ReceiverMaxNumberOfInitializationBytesInTotal,
            };
        }
    }
}