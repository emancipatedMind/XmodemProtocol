using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Port used to facilitate transfer.
        /// </summary>
        public System.IO.Ports.SerialPort Port { get; set; }

        /// <summary>
        /// Object used for CRC.
        /// </summary>
        public CRC16LTE CheckSumValidator { get; set; } = new CRC16LTE();

        #region Shared Properties.
        /// <summary>
        /// Buffer used by XModemCommunicator to store raw data.
        /// </summary>
        public IEnumerable<byte> Data {
            get {
                if (_data == null) return null;
                return new List<byte>(_data);
            }
            set {
                if (State != XModemStates.Idle) return;
                if (value == null) {
                    Role = XModemRole.Receiver;
                }
                else if (_data == null || !_data.SequenceEqual(value)) {
                    _data = new List<byte>(value);
                    Role = XModemRole.Sender;
                    _rebuildPackets = true;
                }
            }
        }

        /// <summary>
        /// Mode of instance.
        /// </summary>
        public XModemMode Mode {
            get {
                lock (_modeLockToken) { 
                    return _mode;
                }
            }
            set {
                lock (_modeLockToken) {
                    bool isReceiver = Role == XModemRole.Receiver;
                    if (isReceiver == true && value == XModemMode.CRC)
                        value = XModemMode.OneK;
                    XModemMode oldMode = _mode;
                    _mode = value;
                    if (isReceiver == false) {
                        if (Packets != null && oldMode == XModemMode.OneK && _mode != XModemMode.Checksum) {
                            if (_mode == XModemMode.OneK) {
                                if (Packets.Count > 1 && Packets[0].Count < 1029)
                                    _rebuildPackets = true;
                            } 
                            else if (_mode == XModemMode.CRC) {
                                foreach(var p in Packets) {
                                    if (p.Count != 133) {
                                        _rebuildPackets = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else _rebuildPackets = true;
                    }
                    if (_mode != oldMode) Task.Run(() => ModeUpdated?.Invoke(this, new ModeUpdatedEventArgs(_mode, oldMode)));
                }
            }
        }
        #endregion

        #region Shared options.
        /// <summary>
        /// Default : 5.
        /// Shared option.
        /// Number of CANs sent during an abort.
        /// </summary>
        public int CANsSentDuringAbort { get; set; } = 5;

        /// <summary>
        /// Default : 5.
        /// Shared option.
        /// Number of consecutive CANs that will prompt an abort.
        /// </summary>
        public int CancellationBytesRequired { get; set; } = 5;
        #endregion

        #region XModem Bytes.
        /// <summary>
        /// Default: 0x01.
        /// Sender begins each 128-byte packet with this header.
        /// </summary>
        public byte SOH { get; set; } = 0x01;

        /// <summary>
        /// Default: 0x02.
        /// Sender begins each 1024-byte packet with this header.
        /// </summary>
        public byte STX { get; set; } = 0x02;

        /// <summary>
        /// Default: 0x06.
        /// Receiver sends this to indicate packet was received successfully with no errors.
        /// </summary>
        public byte ACK { get; set; } = 0x06;

        /// <summary>
        /// Default: 0x15.
        /// Receiver sends this to initiate XModem-Checksum file transfer -- OR -- indicate packet errors.
        /// </summary>
        public byte NAK { get; set; } = 0x15;

        /// <summary>
        /// Default: 0x43.
        /// Receiver sends this to initiate XModem-CRC or XModem-1K file transfer.
        /// </summary>
        public byte C { get; set; } = 0x43;

        /// <summary>
        /// Default: 0x04.
        /// Sender sends this to mark the end of file. Receiver must acknowledge receipt of this byte with <ACK>, otherwise Sender resends <EOT> .
        /// </summary>
        public byte EOT { get; set; } = 0x04;

        /// <summary>
        /// Default: 0x1A.
        /// This is used as a padding byte in the original specification.
        /// </summary>
        public byte SUB { get; set; } = 0x1A;

        /// <summary>
        /// Default: 0x18.
        /// [Commonly used but unofficial] Sender or Receiver sends this byte to abort file transfer.
        /// </summary>
        public byte CAN { get; set; } = 0x18;

        #endregion

        #region Sender Only.

        /// <summary>
        /// Default : 10000ms.
        /// Used exclusively by Sender.
        /// Timeout while waiting for initialization byte. Zero and all negative integers means no timeout should occur.
        /// </summary>
        public int SenderInitializationTimeout { get; set; } = 10000;

        #endregion

        #region Receiver Only.
        /// <summary>
        /// Default : 5.
        /// Used exclusively by Receiver.
        /// Number of consecutive NAKs that will prompt an abort.
        /// </summary>
        public int ReceiverConsecutiveNAKBytesRequiredForCancellation { get; set; } = 5;

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

        /// <summary>
        /// Default : 10000ms.
        /// Used exclusively by Receiver.
        /// After sending a packet, this is the amount of time receiver will wait for a packet before NAKing sender in case response was lost.
        /// Must be between 1000 and 20000ms.
        /// </summary>
        public int ReceiverTimeoutDuringPacketReception {
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

    }
}