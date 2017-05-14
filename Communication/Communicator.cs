namespace XModemProtocol.Communication {
    using System.Collections.Generic;
    using System.Linq;
    using System.IO.Ports;
    public class Communicator : ICommunicator {

        public Communicator(SerialPort port) {
            Port = port;
        }
        public Communicator() : this(new SerialPort()) { }

        public SerialPort Port { set; private get; }

        public virtual int BytesInReadBuffer => Port.BytesToRead;

        public virtual void Flush() {
            Port.DiscardInBuffer();
            Port.DiscardOutBuffer();
        }

        public virtual List<byte> ReadAllBytes() {
            List<byte> byteList = new List<byte>();
            byte[] byteArray;
            do {
                int bytesToRead = Port.BytesToRead;
                byteArray = new byte[bytesToRead];
                Port.Read(byteArray, 0, bytesToRead);
                byteList.AddRange(byteArray);
                System.Threading.Tasks.Task.Delay(10).Wait();
            } while (Port.BytesToRead > 0);
            return byteList;
        }

        public virtual void Write(IEnumerable<byte> buffer) {
            byte[] bufferArray = buffer.ToArray();
            Port.Write(bufferArray, 0, bufferArray.Length);
        }

        public virtual void Write(byte buffer) {
            Port.Write(new byte[] { buffer }, 0, 1);
        }

        public virtual byte ReadSingleByte() {
            return (byte) Port.ReadByte();
        }
    }
}
