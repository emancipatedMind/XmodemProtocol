using System;
using System.Collections.Generic;
using System.Threading;
using XModemProtocol.EventData;
namespace XModemProtocol.Options {
    public interface IContext {
        XModemStates State { get; set; }
        List<List<byte>> Packets { get; set; }
        List<byte> Data { get; set; }
        CancellationToken Token { get; set; }
        event EventHandler<StateUpdatedEventArgs> StateUpdated;
        event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;
    }
}