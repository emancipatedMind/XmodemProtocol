using System.Collections.Generic;
using System.Linq;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Holds logic for when an operation has completed.
        /// </summary>
        private void CompleteOperation() {
            Completed?.Invoke(this, new CompletedEventArgs(_data));
            State = XModemStates.Idle;
        }

        /// <summary>
        /// Increment consecutive loops with insufficient amount of CANs.
        /// </summary>
        /// <returns>Whether _consecutiveloopsWithCANs is more than 20.</returns>
        private bool IncrementConsecutiveLoopsWithInsufficientCAN() {
            if (++_consecutiveLoopsWithCANs > 20) {
                _consecutiveLoopsWithCANs = 0;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Perform checksum with supplied list.
        /// </summary>
        /// <param name="packetInfo">Packets to be checked.</param>
        /// <returns>Two count list of checksum</returns>
        private List<byte> CheckSum(List<byte> packetInfo) {
            if (Mode == XModemMode.Checksum)
                return new List<byte> { (byte)packetInfo.Sum(b => b) };
            else
                return CheckSumValidator.ComputeChecksum(packetInfo).ToList();
        }

        /// <summary>
        /// Set common options for instance.
        /// </summary>
        /// <param name="options">Options.</param>
        private void SetCommonOptions(XModemProtocolOptions options) {
            SOH = options.SOH;
            STX = options.STX;
            ACK = options.ACK;
            NAK = options.NAK;
            C = options.C;
            CAN = options.CAN;
            EOT = options.EOT;
            SUB = options.SUB;
            CancellationBytesRequired = options.CancellationBytesRequired;
            ReceiverConsecutiveNAKBytesRequiredForCancellation = options.ReceiverConsecutiveNAKsRequiredForCancellation;
            CANsSentDuringAbort = options.CANSentDuringAbort;
        }

        /// <summary>
        /// This overload of the Abort method can override the general rule of when to initiate a cancel or not.
        /// </summary>
        /// <param name="e">An instance of the AbortedEventArgs class.</param>
        /// <param name="sendCAN">Whether to initiate cancel or not.</param>
        private void Abort(AbortedEventArgs e, bool sendCAN) {
            if (sendCAN) Port.Write(Enumerable.Repeat(CAN, CANsSentDuringAbort).ToArray());
            Aborted?.Invoke(this, e);
            State = XModemStates.Idle;
        }

        /// <summary>
        /// Resets variables, and some cleanup in the instance in order to prepare for an operation.
        /// </summary>
        private void Reset() {
            _tempBuffer = null;
            _initializationTimeOut?.Dispose();
            _initializationTimeOut = null;
            _consecutiveNAKs = 0;
            _initializationWaitHandle.Reset();
            _packetIndex = Role == XModemRole.Receiver ? 1 : 0;
            _consecutiveLoopsWithCANs = 0;
            _countOfCsSent = 0;
            _numOfInitializationBytesSent = 0;
        }

        /// <summary>
        /// Method used to cancel operation. If State is idle, or PendingCompletion, method does nothing.
        /// If XModemCommunicator is initializaing, it must finish initializing before cancel happens.
        /// </summary>
        /// <returns>If instance was in position to be cancelled, returns true. Otherwise, false.</returns>
        public void CancelOperation() {
            if (State == XModemStates.Idle || State == XModemStates.PendingCompletion) return;
            else if (State == XModemStates.Initializing) while(State == XModemStates.Initializing) { } 
            State = XModemStates.Cancelled;
        }
    }
}