using System.Collections.Generic;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        #region Shared Properties.
        /// <summary>
        /// Buffer used by XModemCommunicator to store raw data.
        /// </summary>
        public List<byte> Data { get; private set; } = null;

        /// <summary>
        /// Packets received or sent.
        /// </summary>
        public List<List<byte>> Packets { get; private set; } = null;

        /// <summary>
        /// Number of CANs sent during an abort.
        /// Can be customized with XModemProtocolOptions.CANsSentDuringAbort.
        /// </summary>
        public int CANsSentDuringAbort { get; private set; }

        /// <summary>
        /// Number of consecutive CANs that will prompt an abort.
        /// Can be customized with XModemProtocolOptions.CancellationBytesRequired.
        /// </summary>
        public int CancellationBytesRequired { get; private set; }

        /// <summary>
        /// Internal state of the XModemCommunicator instance.
        /// </summary>
        public XModemStates State {
            get { return _state; }
            private set {
                if (_state == value) return;
                XModemStates oldState = _state;
                _state = value;
                StateUpdated?.Invoke(this, new StateUpdatedEventArgs(_state, oldState));
            }
        }

        /// <summary>
        /// Packet size being used.
        /// </summary>
        public XModemPacketSizes PacketSize {
            get {
                return Mode == XModemMode.OneK ? XModemPacketSizes.OneK : XModemPacketSizes.Standard;
            }
        }

        /// <summary>
        /// Mode of instance.
        /// Can be customized with XModemProtocolOptions.Mode.
        /// </summary>
        public XModemMode Mode {
            get { return _mode; }
            private set {
                if (_mode == value) return;
                XModemMode oldMode = _mode;
                _mode = value;
                ModeUpdated?.Invoke(this, new ModeUpdatedEventArgs(_mode, oldMode));
            }
        }
        #endregion
        
        #region XModem Bytes
        /// <summary>
        /// Default: 0x01.
        /// Sender begins each 128-byte packet with this header.
        /// Can be customized with XModemProtocolOptions.SOH.
        /// </summary>
        public byte SOH { get; private set; }

        /// <summary>
        /// Default: 0x02.
        /// Sender begins each 1024-byte packet with this header.
        /// Can be customized with XModemProtocolOptions.STX.
        /// </summary>
        public byte STX { get; private set; }

        /// <summary>
        /// Default: 0x06.
        /// Receiver sends this to indicate packet was received successfully with no errors.
        /// Can be customized with XModemProtocolOptions.ACK.
        /// </summary>
        public byte ACK { get; private set; }

        /// <summary>
        /// Default: 0x15.
        /// Receiver sends this to initiate XModem-Checksum file transfer -- OR -- indicate packet errors.
        /// Can be customized with XModemProtocolOptions.NAK.
        /// </summary>
        public byte NAK { get; private set; }

        /// <summary>
        /// Default: 0x43.
        /// Receiver sends this to initiate XModem-CRC or XModem-1K file transfer.
        /// Can be customized with XModemProtocolOptions.C.
        /// </summary>
        public byte C { get; private set; }

        /// <summary>
        /// Default: 0x04.
        /// Sender sends this to mark the end of file. Receiver must acknowledge receipt of this byte with <ACK>, otherwise Sender resends <EOT> .
        /// Can be customized with XModemProtocolOptions.EOT.
        /// </summary>
        public byte EOT { get; private set; }

        /// <summary>
        /// Default: 0x1A.
        /// This is used as a padding byte in the original specification.
        /// Can be customized with XModemProtocolOptions.SUB.
        /// </summary>
        public byte SUB { get; private set; }

        /// <summary>
        /// Default: 0x18.
        /// [Commonly used but unofficial] Sender or Receiver sends this byte to abort file transfer.
        /// Can be customized with XModemProtocolOptions.CAN.
        /// </summary>
        public byte CAN { get; private set; }

        #endregion

        #region Sender Only.
        /// <summary>
        /// Used exclusively by Sender.
        /// File name of contents in Data if retrieved from file.
        /// If instance becomes receiver, or XModemProtocolOptions.Buffer is used to update Data,
        /// this becomes null. If XModemProtocolOptions.Filename is used,
        /// this becomes that value.
        /// Can be customized with XModemProtocolOptions.SenderFilename.
        /// </summary>
        public string SenderFilename { get; private set; } = null;
        #endregion

        #region Receiver Only.
        /// <summary>
        /// Used exclusively by Receiver.
        /// Number of consecutive NAKs that will prompt an abort.
        /// Can be customized with XModemProtocolOptions.ReceiverNAKBytesRequired.
        /// </summary>
        public int ReceiverConsecutiveNAKBytesRequiredForCancellation { get; private set; }

        /// <summary>
        /// Used exclusively by Receiver.
        /// Maximum number of initialization bytes to send if using CRC
        /// before falling back to normal XModem.
        /// Can be customized with XModemProtocolOptions.ReceiverMaxNumberOfInitializationBytesForCRC.
        /// </summary>
        public int ReceiverMaxNumberOfInitializationBytesForCRC { get; private set; }

        /// <summary>
        /// Used exclusively by Receiver.
        /// Maximum number of tries. If in CRC or 1k mode, this will include number of
        /// tries in MaxNumberOfInitializationBytesForCRC. Must be at least 5.
        /// Can be customized with XModemProtocolOptions.ReceiverMaxNumberOfInitializationBytesInTotal.
        /// </summary>
        public int ReceiverMaxNumberOfInitializationBytesInTotal { get; private set; }

        /// <summary>
        /// Used exclusively by Receiver.
        /// After sending a packet, this is the amount of time receiver will wait for a packet before a NAK is sent to prompt sender.
        /// Must be between 1000 and 20000ms.
        /// Can be customized with XModemProtocolOptions.ReceiverTimeoutDuringPacketReception.
        /// </summary>
        public int ReceiverTimeoutDuringPacketReception { get; private set; }
        #endregion
    }
}