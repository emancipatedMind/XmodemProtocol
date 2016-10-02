using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol.Communication;

namespace XModemProtocolTester {
    public class InterPlayComm : ICommunicator {

        public InterPlayComm Partner { get; set; }
        public List<byte> Data { get; set; } = new List<byte>();
        public string Name { get; set; } = "InterPlayComm";
        public bool WriteDebug { get; set; } = true;

        public int BytesInReadBuffer => Data?.Count ?? 0;
        public bool ReadBufferContainsData => Data?.Count > 0;
        public bool ReadBufferIsEmpty => !ReadBufferContainsData;

        public void Flush() => Data = new List<byte>();

        public List<byte> ReadAllBytes() {
            var data = Data;
            Flush();
            return data;
        }

        public void Write(IEnumerable<byte> buffer) {
            //foreach (var b in buffer) {
            //    Partner.Data.Add(b);
            //}
            Partner.Data.AddRange(buffer);
            if (WriteDebug) {
                string desc = $"{Name} => [ ";
                foreach (var b in buffer)
                    desc += $"0x{b:X2} ";
                desc += "]";
                System.Diagnostics.Debug.WriteLine(desc);
            }
        }

        public void Write(byte buffer) {
            Partner.Data.Add(buffer);
            if (WriteDebug) 
                System.Diagnostics.Debug.WriteLine($"{Name} => [ 0x{buffer:X2} ]");
        }
    }
}