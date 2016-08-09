using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XmodemProtocol {
    public partial class XModemCommunicator {

        int _packetCount = 0;
        PacketSizes _packetSize = PacketSizes.OneK;
        List<byte> _buffer = null;
        List<byte> _tempBuffer = new List<byte>();
        XModemMode _mode = XModemMode.Checksum;
        CancellationToken _cancellationToken = CancellationToken.None;
        int _consecutiveNAKs = 0;
        States _state = States.Idle;

        bool _mutationsAllowed = true;

        System.Timers.Timer _initializationTimeOut;

        bool _working = true;
    }
}