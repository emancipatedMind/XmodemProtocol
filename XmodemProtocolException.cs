using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    /// <summary>
    /// The Exception used inside the XModemCommunicator class.
    /// </summary>
    public class XModemProtocolException : ApplicationException {

        public XModemProtocolException(string message) : base(message) { }
        public XModemProtocolException(string message, Exception exception) : base(message, exception) { }

    }
}