namespace XModemProtocol.Communication {
    using System.Collections.Generic;
    public interface ICommunicator {
        void Write(byte buffer);
        void Write(IEnumerable<byte> buffer);
        void Flush();
        List<byte> ReadAllBytes();
        byte ReadSingleByte();
        int BytesInReadBuffer { get; }
    }
}