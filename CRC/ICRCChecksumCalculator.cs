using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.CRC {
    public interface ICRCChecksumCalculator : ICRCInitialValue {
        ICRCLookUpTable Table { get; set; }
        IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input);
    }
}