using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol.Communication {
    public interface ICommunicator {
        void Write(byte buffer);
        void Write(IEnumerable<byte> buffer);
        void Flush();
        List<byte> ReadAllBytes();
        bool IsReadBufferEmpty { get; }
    }
}