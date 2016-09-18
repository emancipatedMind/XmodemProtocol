using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Calculators {
    public abstract class BaseFunctions {
        protected int ApplyMask(int input, int mask) =>  input & mask;
    }
}