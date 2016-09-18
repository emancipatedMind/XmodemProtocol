using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Calculators {
    public interface ICRCChecksumCalculator : IChecksumCalculator {
        IEnumerable<byte> InitialCRCValue { get; set; }
    }
}