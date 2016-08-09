using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmodemProtocol {
    public partial class XModemCommunicator {

        #region XModem Bytes
        /// <summary>
        /// Default: 0x01.
        /// Sender begins each 128-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte SOH { get; set; } = 0x01;

        /// <summary>
        /// Default: 0x02.
        /// Sender begins each 1024-byte packet with this header.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte STX { get; set; } = 0x02;

        /// <summary>
        /// Default: 0x06.
        /// Receiver sends this to indicate packet was received successfully with no errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte ACK { get; set; } = 0x06;

        /// <summary>
        /// Default: 0x15.
        /// Receiver sends this to initiate XModem-Checksum file transfer -- OR -- indicate packet errors.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte NAK { get; set; } = 0x15;

        /// <summary>
        /// Default: 0x43.
        /// Receiver sends this to initiate XModem-CRC or XModem-1K file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte C { get; set; } = 0x43;

        /// <summary>
        /// Default: 0x04.
        /// Sender sends this to mark the end of file. Receiver must acknowledge receipt of this byte with <ACK>, otherwise Sender resends <EOT> .
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte EOT { get; set; } = 0x04;

        /// <summary>
        /// Default: 0x1A.
        /// This is used as a padding byte in the original specification.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte SUB { get; set; } = 0x1A;

        /// <summary>
        /// Default: 0x18.
        /// [Commonly used but unofficial] Sender or Receiver sends this byte to abort file transfer.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte CAN { get; set; } = 0x18;

        /// <summary>
        /// Default: 0x1A.
        /// [Commonly used but unofficial] MS-DOS version of <EOT>.
        /// Exposed in case user needs to customize with non-standard value.
        /// </summary>
        public byte EOF { get; set; } = 0x1A;
        #endregion

        /// <summary>
        /// Port used to facilitate transfer.
        /// </summary>
        public System.IO.Ports.SerialPort Port { get; set; }

        public int NAKLimit { get; set; } = 5;

        public int NumCANForAbort { get; set; } = 5;

    }
}