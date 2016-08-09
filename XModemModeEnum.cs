using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmodemProtocol
{
    public enum XModemMode {
        Auto, // Auto supports both XMODEM-CRC and XMODEM-1k.
        Checksum, // Normal XMODEM mode.
    }
}