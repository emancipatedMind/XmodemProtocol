using System.Collections.Generic;

namespace XModemProtocol.Communication
{
    public interface ICommunicator {
        void Write(byte buffer);
        void Write(string buffer);
        void Write(IEnumerable<byte> buffer);
        void Flush();
        List<byte> ReadAllBytes();
        byte ReadSingleByte();
        bool ReadBufferIsEmpty { get; }
        bool ReadBufferContainsData { get; }
        int BytesInReadBuffer { get; }
    }
}