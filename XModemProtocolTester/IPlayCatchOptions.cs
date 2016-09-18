using System.Collections.Generic;

namespace XModemProtocolTester {
    public interface IPlayCatchOptions {
        string PortOne { get; }
        string PortTwo { get; }
        IEnumerable<byte> Data { get; }
    }
}