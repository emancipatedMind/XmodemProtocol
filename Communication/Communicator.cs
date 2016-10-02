using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.Ports;

namespace XModemProtocol.Communication {
    public class Communicator : ICommunicator {

        SerialPort _port;

        public Communicator(SerialPort port) {
            _port = port;
        }

        public int BytesInReadBuffer => _port.BytesToRead;

        public bool ReadBufferIsEmpty => _port.BytesToRead == 0;

        public bool ReadBufferContainsData => _port.BytesToRead != 0;

        public void Flush() {
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();
        }

        public List<byte> ReadAllBytes() {
            List<byte> byteList = new List<byte>();
            byte[] byteArray;
            do {
                int bytesToRead = _port.BytesToRead;
                byteArray = new byte[bytesToRead];
                _port.Read(byteArray, 0, bytesToRead);
                byteList.AddRange(byteArray);
                System.Threading.Tasks.Task.Delay(10); 
            } while (_port.BytesToRead > 0);
            return byteList;
        }

        public void Write(IEnumerable<byte> buffer) {
            byte[] bufferArray = buffer.ToArray();
            _port.Write(bufferArray, 0, bufferArray.Length);
        }

        public void Write(byte buffer) {
            _port.Write(new byte[] { buffer }, 0, 1);
        }
    }
}