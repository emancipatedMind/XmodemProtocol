using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Factories {
    using Tools;
    interface IToolFactory {
        IXModemTools GetToolsFor(XModemMode mode); 
    }
}