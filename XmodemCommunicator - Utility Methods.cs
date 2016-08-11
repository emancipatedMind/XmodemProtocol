using System.Collections.Generic;
using System.Linq;
using CSharpToolkit;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        private List<byte> CheckSum(List<byte> packetInfo) {
            if (Mode == XModemMode.Checksum)
                return new List<byte> { (byte)packetInfo.Sum(b => b) };
            else
                // This needs to produce CRC checksum.
                return new List<byte>();
        }

        private void Abort(AbortedEventArgs e) {
            bool sendCAN = e.Reason != XModemAbortReason.CancelRequestReceived;
            Abort(e, sendCAN);
        }

        private void Abort(AbortedEventArgs e, bool sendCAN) {
            if (sendCAN) Port.Write(Enumerable.Repeat(CAN, CANSentDuringAbort).ToArray());
            Aborted?.Invoke(this, e);
            Reset();
        }

        private void Reset() {
            _tempBuffer = new List<byte>();
            _initializationTimeOut?.Dispose();
            State = XModemStates.Idle;
            ResetConsecutiveNAKs();
            _cancellationWaitHandle.Reset();
            _initializationWaitHandle.Reset();
            _packetIndexToSend = 0;
        }

        private bool IncrementConsecutiveNAKs() {
            _consecutiveNAKs++;
            if (_consecutiveNAKs > NAKBytesRequired) {
                _consecutiveNAKs = 0;
                return true;
            }
            return false;
        }

        private void ResetConsecutiveNAKs() => _consecutiveNAKs = 0;

        /// <summary>
        /// Method used to cancel operation. If State is idle, method does nothing.
        /// </summary>
        /// <returns>If instance was in position to be cancelled, returns true. Otherwise, false.</returns>
        public void CancelOperation() {
            if (State == XModemStates.Idle) return;
            _cancellationWaitHandle.Set();
        }

    }
}
