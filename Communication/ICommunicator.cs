using System.Collections.Generic;

namespace XModemProtocol.Communication
{
    public interface ICommunicator {
        void Write(byte buffer);
        void Write(IEnumerable<byte> buffer);
        void Flush();
        List<byte> ReadAllBytes();
        bool ReadBufferIsEmpty { get; }
        bool ReadBufferContainsData { get; }
        int BytesInReadBuffer { get; }
    }
}