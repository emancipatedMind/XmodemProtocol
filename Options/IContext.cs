using System;
using System.Collections.Generic;
using System.Threading;

namespace XModemProtocol.Options {
    using EventData;
    public interface IContext {
        XModemStates State { get; set; }
        XModemMode Mode { get; set; }
        List<List<byte>> Packets { get; set; }
        List<byte> Data { get; set; }
        CancellationToken Token { get; set; }
        event EventHandler<StateUpdatedEventArgs> StateUpdated;
        event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;
        event EventHandler<ModeUpdatedEventArgs> ModeUpdated;
    }
}