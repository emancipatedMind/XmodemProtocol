using System.Collections.Generic;
using System.Threading;
namespace XModemProtocol.Options {
    public interface IContext {
        XModemStates State { get; set; }
        bool BuildRequested { get; set; }
        List<List<byte>> Packets { get; set; }
        CancellationToken Token { get; set; }
    }
}