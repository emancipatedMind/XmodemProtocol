using System.Collections.Generic;
using System.Threading;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        List<byte> _buffer = null;
        List<byte> _tempBuffer = new List<byte>();

        int _packetIndexToSend = 0;

        int _consecutiveNAKs = 0;

        #region Backing Fields
        int _packetCount = 0;
        XModemPacketSizes _packetSize = XModemPacketSizes.OneK;
        XModemMode _mode = XModemMode.Auto;
        XModemStates _state = XModemStates.Idle;
        #endregion

        bool _mutationsAllowed = true;

        System.Timers.Timer _initializationTimeOut;

        ManualResetEvent _initializationWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _cancellationWaitHandle = new ManualResetEvent(false);

    }
}