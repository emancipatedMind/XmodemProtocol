using System.Collections.Generic;
using System.Threading;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        // Just fields to be used by the instance.

        List<byte> _tempBuffer = null;

        int _packetIndex = 0;

        int _countOfCsSent = 0;
        int _numOfInitializationBytesSent = 0;

        int _consecutiveNAKs = 0;
        int _consecutiveLoopsWithCANs = 0;

        #region Backing Fields
        XModemMode _mode = XModemMode.OneK;
        XModemStates _state = XModemStates.Idle;
        XModemRole _role = XModemRole.None;
        #endregion

        System.Timers.Timer _initializationTimeOut;

        ManualResetEvent _initializationWaitHandle = new ManualResetEvent(false);
    }
}