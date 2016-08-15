using System.Collections.Generic;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Buffer used by XModemCommunicator to store raw data.
        /// </summary>
        public List<byte> Data { get; private set; } = null;

        /// <summary>
        /// Packets received or sent.
        /// </summary>
        public List<List<byte>> Packets { get; private set; } = null;

        /// <summary>
        /// File name of contents in buffer. Only used when sending.
        /// </summary>
        public string Filename { get; private set; } = null;

        /// <summary>
        /// Number of consecutive NAKs that will prompt an abort.
        /// </summary>
        public int NAKBytesRequired { get; private set; } = 5;

        /// <summary>
        /// Number of NAKs sent during an abort.
        /// </summary>
        public int CANSentDuringAbort { get; private set; } = 5;

        /// <summary>
        /// Number of consecutive CANs that will prompt an abort.
        /// </summary>
        public int CancellationBytesRequired { get; private set; } = 5;

        /// <summary>
        /// Maximum number of initialization bytes to send if using CRC
        /// before falling back to normal XModem.
        /// </summary>
        public int MaxNumberOfInitializationBytesForCRC { get; private set; } = 3;

        /// <summary>
        /// Maximum number of tries. If in CRC or 1k mode, this will include number of
        /// tries in MaxNumberOfInitializationBytesForCRC. Must be at least 5.
        /// </summary>
        public int MaxNumberOfInitializationBytesInTotal { get; private set; } = 10;

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
                return _packetSize;
            }
        }

        /// <summary>
        /// Mode of instance.
        /// </summary>
        public XModemMode Mode {
            get { return _mode; }
            private set {
                if (_mode == value) return;
                XModemMode oldMode = _mode;
                _mode = value;
                if (_mode == XModemMode.Checksum || _mode == XModemMode.CRC)
                    _packetSize = XModemPacketSizes.Standard;
                else
                    _packetSize = XModemPacketSizes.OneK;
                ModeUpdated?.Invoke(this, new ModeUpdatedEventArgs(_mode, oldMode));
            }
        }
        
        #region XModem Bytes
        /// <summary>
        /// Default: 0x01.
        /// Sender begins each 128-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte SOH { get; private set; } = 0x01;

        /// <summary>
        /// Default: 0x02.
        /// Sender begins each 1024-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte STX { get; private set; } = 0x02;

        /// <summary>
        /// Default: 0x06.
        /// Receiver sends this to indicate packet was received successfully with no errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte ACK { get; private set; } = 0x06;

        /// <summary>
        /// Default: 0x15.
        /// Receiver sends this to initiate XModem-Checksum file transfer -- OR -- indicate packet errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte NAK { get; private set; } = 0x15;

        /// <summary>
        /// Default: 0x43.
        /// Receiver sends this to initiate XModem-CRC or XModem-1K file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte C { get; private set; } = 0x43;

        /// <summary>
        /// Default: 0x04.
        /// Sender sends this to mark the end of file. Receiver must acknowledge receipt of this byte with <ACK>, otherwise Sender resends <EOT> .
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte EOT { get; private set; } = 0x04;

        /// <summary>
        /// Default: 0x1A.
        /// This is used as a padding byte in the original specification.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte SUB { get; private set; } = 0x1A;

        /// <summary>
        /// Default: 0x18.
        /// [Commonly used but unofficial] Sender or Receiver sends this byte to abort file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte CAN { get; private set; } = 0x18;

        /// <summary>
        /// Default: 0x1A.
        /// [Commonly used but unofficial] MS-DOS version of <EOT>.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte EOF { get; private set; } = 0x1A;
        #endregion

    }
}