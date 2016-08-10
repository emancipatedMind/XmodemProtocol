using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        List<byte> _buffer = null;
        List<byte> _tempBuffer = new List<byte>();

        int _packetCount = 0;
        int _consecutiveNAKs = 0;

        PacketSizes _packetSize = PacketSizes.OneK;
        XModemMode _mode = XModemMode.Checksum;
        States _state = States.Idle;

        bool _mutationsAllowed = true;

        System.Timers.Timer _initializationTimeOut;

        ManualResetEvent _sendOperationWaitHandle = new ManualResetEvent(true);

        CancellationToken _cancellationToken = CancellationToken.None;
    }
}