using System;
using System.Collections.Generic;
using System.IO.Ports;

namespace XModemProtocol.Communication {
    public class Communicator : ICommunicator {

        SerialPort _port;

        public Communicator(SerialPort port) {
            _port = port;
        }

        public bool ReadBufferIsEmpty => _port.BytesToRead == 0;

        public bool ReadBufferContainsData => _port.BytesToRead != 0;

        public void Flush() {
            _port.Flush();
        }

        public List<byte> ReadAllBytes() {
            return _port.ReadAllBytes();
        }

        public void Write(IEnumerable<byte> buffer) {
            _port.Write(buffer);
        }

        public void Write(byte buffer) {
            _port.Write(buffer);
        }
    }
}