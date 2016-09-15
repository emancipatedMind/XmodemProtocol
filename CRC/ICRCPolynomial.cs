using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.CRC {
    public interface ICRCPolynomial {
        int Polynomial { get; set; }
    }
}
