using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    public enum States {
        Idle,
        SenderAwaitingInitialization,
        SenderPacketSent,
        SenderAwaitingFinalACK,
    }
}