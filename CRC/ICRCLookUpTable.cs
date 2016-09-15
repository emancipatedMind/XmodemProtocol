using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.CRC {
    public interface ICRCLookUpTable : ICRCPolynomial {
        ICRCLookUpTable Table { get; }
        int QueryTable(int index);
    }
}