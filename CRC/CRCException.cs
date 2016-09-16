using System;

namespace XModemProtocol.CRC {
    public class CRCException : ApplicationException {
        public CRCException(string message ) : base(message) { }
    }
}