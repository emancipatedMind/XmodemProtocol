using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XModemProtocol.Communication;

namespace XModemProtocolTester
{
    public class ComStandIn : ICommunicator {
        public virtual List<byte> BytesToSend { get; set; }
        public List<List<byte>> BytesRead { get; private set; } = new List<List<byte>>();

        public virtual bool ReadBufferContainsData => BytesToSend?.Count != 0;
        public bool ReadBufferIsEmpty => !ReadBufferContainsData;
        public void Flush() {
            BytesRead = new List<List<byte>>();
            BytesToSend = new List<byte>();
        }
        public List<byte> ReadAllBytes() => BytesToSend;
        public void Write(IEnumerable<byte> buffer) {
            BytesRead.Add(buffer.ToList());
            string desc = "[ ";
            foreach (var b in buffer)
                desc += $"0x{b:X2} ";
            desc += "]";
            Debug.WriteLine(desc); 
        }
        public void Write(byte buffer) {
            BytesRead.Add(new List<byte> {buffer});
            Debug.WriteLine($"[ 0x{buffer:X2} ]");
        }

    }
}