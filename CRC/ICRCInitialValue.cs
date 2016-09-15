using System.Collections.Generic;

namespace XModemProtocol.CRC {
    public interface ICRCInitialValue {
        IEnumerable<byte> InitialCRCValue { get; set; }
    }
}