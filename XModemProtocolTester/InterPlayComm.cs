using System.Collections.Generic;
using System.Text;
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
        public void Write(string buffer) => Write(Encoding.Default.GetBytes(buffer));

        public List<byte> ReadAllBytes() {
            var data = Data;
            Flush();
            return data;
        }

        public void Write(IEnumerable<byte> buffer) {
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

        public byte ReadSingleByte() {
            byte byteRead = Data[0];
            Data.RemoveAt(0);
            return byteRead;
        }
    }
}