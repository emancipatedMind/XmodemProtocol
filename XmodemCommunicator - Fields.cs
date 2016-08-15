using System.Collections.Generic;
using System.Threading;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        List<byte> _tempBuffer = new List<byte>();

        int _packetIndexToSend = 0;

        int _packetIndexToReceive = 1;

        int _countOfCsSent = 0;
        int _numOfInitializationBytesSent = 0;

        int _consecutiveNAKs = 0;

        #region Backing Fields
        XModemPacketSizes _packetSize = XModemPacketSizes.OneK;
        XModemMode _mode = XModemMode.OneK;
        XModemStates _state = XModemStates.Idle;
        #endregion

        System.Timers.Timer _initializationTimeOut;

        ManualResetEvent _initializationWaitHandle = new ManualResetEvent(false);
        ManualResetEvent _cancellationWaitHandle = new ManualResetEvent(false);

    }
}