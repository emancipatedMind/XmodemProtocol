using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.CRC {
    public interface ICRCChecksumCalculator : IChecksumCalculator, ICRCInitialValue {
        ICRCLookUpTable Table { get; set; }
    }
}