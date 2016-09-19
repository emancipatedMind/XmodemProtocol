using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Calculators {
    using Options;
    public interface IChecksumCalculator {
        IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input);
    }
}