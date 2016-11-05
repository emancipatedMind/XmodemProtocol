using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using XModemProtocol.Communication;

namespace XModemProtocolTester {
    public class ComStandIn : ICommunicator {

        public virtual int BytesInReadBuffer => BytesToSend?.Count ?? 0;
        public virtual List<byte> BytesToSend { get; set; } = new List<byte>();

        public List<List<byte>> BytesRead { get; set; } = new List<List<byte>>();
        public void Flush() { }
        public List<byte> ReadAllBytes() => BytesToSend;
        public byte ReadSingleByte() => BytesToSend[0];

        public void Write(IEnumerable<byte> buffer) {
            if (BytesRead == null) BytesRead = new List<List<byte>>(); 
            BytesRead.Add(buffer.ToList());
            string desc = "[ ";
            foreach (var b in buffer)
                desc += $"0x{b:X2} ";
            desc += "]";
            Debug.WriteLine(desc); 
        }
        public void Write(byte buffer) {
            if (BytesRead == null) BytesRead = new List<List<byte>>(); 
            BytesRead.Add(new List<byte> {buffer});
            Debug.WriteLine($"[ 0x{buffer:X2} ]");
        }

    }
}