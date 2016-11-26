namespace XModemProtocol.Options {
    using Configuration;
    /// <summary>
    /// Class used to hold options used by both the Sender, and Receiver.
    /// </summary>
    public class XModemProtocolOptions : IXModemProtocolOptions {

        #region Shared Options
        /// <summary>
        /// Shared option.
        /// Number of CANs sent during an abort.
        /// </summary>
        public virtual int CANSentDuringAbort { get; set; } =
            XModemProtocolConfigurationSection.Settings.CANSentDuringAbort.Value;

        /// <summary>
        /// Shared option.
        /// Number of consecutive CANs that will prompt an abort.
        /// </summary>
        public virtual int CancellationBytesRequired { get; set; } = 
            XModemProtocolConfigurationSection.Settings.CancellationBytesRequired.Value;
        #endregion

        #region XModem Bytes
        /// <summary>
        /// Default: 0x01.
        /// Shared option.
        /// Sender begins each 128-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte SOH { get; set; } =
            XModemProtocolConfigurationSection.Settings.SOH.Value;

        /// <summary>
        /// Default: 0x02.
        /// Shared option.
        /// Sender begins each 1024-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte STX { get; set; } =
            XModemProtocolConfigurationSection.Settings.STX.Value;

        /// <summary>
        /// Default: 0x06.
        /// Shared option.
        /// Receiver sends this to indicate packet was received successfully with no errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte ACK { get; set; } =
            XModemProtocolConfigurationSection.Settings.ACK.Value;

        /// <summary>
        /// Default: 0x15.
        /// Shared option.
        /// Receiver sends this to initiate XModem-Checksum file transfer -- OR -- indicate packet errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte NAK { get; set; } =
            XModemProtocolConfigurationSection.Settings.NAK.Value;

        /// <summary>
        /// Default: 0x43.
        /// Shared option.
        /// Receiver sends this to initiate XModem-CRC or XModem-1K file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte C { get; set; } =
            XModemProtocolConfigurationSection.Settings.C.Value;

        /// <summary>
        /// Default: 0x04.
        /// Shared option.
        /// Sender sends this to mark the end of file. Receiver must acknowledge receipt of this byte with <ACK>, otherwise Sender resends <EOT> .
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte EOT { get; set; } =
            XModemProtocolConfigurationSection.Settings.EOT.Value;

        /// <summary>
        /// Default: 0x1A.
        /// Shared option.
        /// This is used as a padding byte in the original specification.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte SUB { get; set; } =
            XModemProtocolConfigurationSection.Settings.SUB.Value;

        /// <summary>
        /// Default: 0x18.
        /// Shared option.
        /// [Commonly used but unofficial] Sender or Receiver sends this byte to abort file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public virtual byte CAN { get; set; } =
            XModemProtocolConfigurationSection.Settings.CAN.Value;
        #endregion

        #region Sender Options
        /// <summary>
        /// Default : 10000ms.
        /// Used exclusively by Sender.
        /// Timeout while waiting for initialization byte. Zero and all negative integers means no timeout should occur.
        /// </summary>
        public virtual int SenderInitializationTimeout { get; set; } =
            XModemProtocolConfigurationSection.Settings.SenderInitializationTimeout.Value;

        /// <summary>
        /// Denotes if XModemProtocol.XModemCommunicator will,
        /// when building packets in OneK mode, build packets that are
        /// only 1029 bytes long, or build packets that are both 1029
        /// bytes long, and 133 bytes long.
        /// </summary>
        public virtual OneKPacketSize SenderOneKPacketSize { get; set; } =
            XModemProtocolConfigurationSection.Settings.SenderOneKPacketSize.Value;
        #endregion

        #region ReceiverOptions
        /// <summary>
        /// Default : 5.
        /// Used exclusively by Receiver.
        /// Number of consecutive NAKs that will prompt an abort. 0 signals no abort should occur due to consecutive NAKs.
        /// </summary>
        public virtual int ReceiverConsecutiveNAKsRequiredForCancellation { get; set; } =
            XModemProtocolConfigurationSection.Settings.ReceiverConsecutiveNAKsRequiredForCancellation.Value;

        int _receiverInitializationTimeout =
            XModemProtocolConfigurationSection.Settings.ReceiverInitializationTimeout.Value;
        /// <summary>
        /// Default : 3000ms.
        /// Used exclusively by Receiver.
        /// The time between the initialization bytes sent to initialize transfer.
        /// This must be between 1000ms and 10000ms.
        /// </summary>
        public virtual int ReceiverInitializationTimeout {
            get {
                return _receiverInitializationTimeout;
            }
            set {
                if (value < 1000)
                    _receiverInitializationTimeout = 1000;
                else if (value > 10000)
                    _receiverInitializationTimeout = 10000;
                else _receiverInitializationTimeout = value;
            }
        }


        int _receiverTimeoutDuringPacketReception =
            XModemProtocolConfigurationSection.Settings.ReceiverTimeoutDuringPacketReception.Value;
        /// <summary>
        /// Default : 10000ms.
        /// Used exclusively by Receiver.
        /// After sending a packet, this is the amount of time receiver will wait for a packet before NAKing sender in case response was lost.
        /// Must be between 1000 and 20000ms.
        /// </summary>
        public virtual int ReceiverTimeoutDuringPacketReception {
            get {
                return _receiverTimeoutDuringPacketReception;
            }
            set {
                if (value < 5000)
                    _receiverTimeoutDuringPacketReception = 5000;
                else if (value > 20000)
                    _receiverTimeoutDuringPacketReception = 20000;
                else _receiverTimeoutDuringPacketReception = value;
            }
        }

        int _receiverMaxNumberOfInitializationBytesForCRC =
            XModemProtocolConfigurationSection.Settings.ReceiverMaxNumberOfInitializationBytesForCRC.Value;
        /// <summary>
        /// Default : 3.
        /// Used exclusively by Receiver.
        /// Maximum number of initialization bytes to send if using CRC
        /// before falling back to normal XModem. Must be between 1, and 10.
        /// </summary>
        public virtual int ReceiverMaxNumberOfInitializationBytesForCRC {
            get { return _receiverMaxNumberOfInitializationBytesForCRC; }
            set {
                if (value < 1)
                    _receiverMaxNumberOfInitializationBytesForCRC = 1;
                else if (value > 10)
                    _receiverMaxNumberOfInitializationBytesForCRC = 10;
                else _receiverMaxNumberOfInitializationBytesForCRC = value;

                if (ReceiverMaxNumberOfInitializationBytesInTotal < _receiverMaxNumberOfInitializationBytesForCRC)
                    _receiverMaxNumberOfInitializationBytesForCRC = ReceiverMaxNumberOfInitializationBytesInTotal;
            }
        }

        int _receiverMaxNumberOfInitializationBytesInTotal =
            XModemProtocolConfigurationSection.Settings.ReceiverMaxNumberOfInitializationBytesInTotal.Value;
        /// <summary>
        /// Default : 10.
        /// Used exclusively by Receiver.
        /// Maximum number of initialization bytes to send in total.
        /// If using 1k, this includes the total used
        /// by ReceiverMaxNumberOfInitializationBytesForCRC. 5 minimum.
        /// </summary>
        public virtual int ReceiverMaxNumberOfInitializationBytesInTotal {
            get { return _receiverMaxNumberOfInitializationBytesInTotal; }
            set {
                if (value < 5)
                    _receiverMaxNumberOfInitializationBytesInTotal = 5;
                else _receiverMaxNumberOfInitializationBytesInTotal = value;

                if (_receiverMaxNumberOfInitializationBytesInTotal < ReceiverMaxNumberOfInitializationBytesForCRC)
                    ReceiverMaxNumberOfInitializationBytesForCRC = _receiverMaxNumberOfInitializationBytesInTotal;
            }
        }

        #endregion

        /// <summary>
        /// A method to perform deep copy of instance.
        /// </summary>
        /// <returns>A deep copy of XModemProtocolReceiverOptions.</returns>
        public IXModemProtocolOptions Clone() {
            return new XModemProtocolOptions {
                // Shared Options
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
                SenderOneKPacketSize = SenderOneKPacketSize,

                // Receiver Options
                ReceiverInitializationTimeout = ReceiverInitializationTimeout,
                ReceiverTimeoutDuringPacketReception = ReceiverTimeoutDuringPacketReception,
                ReceiverMaxNumberOfInitializationBytesForCRC = ReceiverMaxNumberOfInitializationBytesForCRC,
                ReceiverMaxNumberOfInitializationBytesInTotal = ReceiverMaxNumberOfInitializationBytesInTotal,
                ReceiverConsecutiveNAKsRequiredForCancellation = ReceiverConsecutiveNAKsRequiredForCancellation,
            };
        }
    }
}