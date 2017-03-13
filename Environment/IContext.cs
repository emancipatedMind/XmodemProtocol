namespace XModemProtocol.Environment {
    using EventData;
    using Factories.Tools;
    using Options;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    public interface IContext {
        void SendCancel();
        XModemStates State { get; set; }
        XModemMode Mode { get; set; }
        List<List<byte>> Packets { get; }
        List<byte> Data { get; set; }
        CancellationToken Token { get; set; }
        IXModemTools Tools { get; }
        int Polynomial { get; set; }
        IXModemProtocolOptions Options { get; set; }
        Communication.ICommunicator Communicator { get; set; }
        event EventHandler<StateUpdatedEventArgs> StateUpdated;
        event EventHandler<PacketsBuiltEventArgs> PacketsBuilt;
        event EventHandler<ModeUpdatedEventArgs> ModeUpdated;
    }
}
