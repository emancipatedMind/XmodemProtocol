using System.Collections.Generic;
using System.Linq;

namespace XModemProtocol {
    public partial class XModemCommunicator {

        private bool CancellationDetected(IEnumerable<byte> recv) {

            // If NumCancellationBytesRequired is less than 1, just exit.
            if (CancellationBytesRequired < 1) return false;

            // LINQ to get indices of CAN bytes.
            // 1). If byte is CAN, record index, if not, make index -1.
            // 2). Remove all elements equal to -1,
            // 3). Place elements in ascending order.
            // 4). Convert to List<byte>. Only need to perform LINQ once.
            var indicesOfCAN = (recv.Select((r, i) => { if (r == CAN) return i; else return -1; }).Where(i => i > -1).OrderBy(i => i)).ToList();

            // If the number of CANs found are not at least equal to the number required, no point in checking further.
            if (indicesOfCAN.Count < CancellationBytesRequired) return false;

            // Check to see if any consecutive amount of CANs found are above set limit.
            // If so, return true indicating such.
            for (int i = 0, counter = 0, index = indicesOfCAN[0]; i < indicesOfCAN.Count; i++) {
                int next = indicesOfCAN[i];
                if (index == next) {
                    ++index;
                    ++counter;
                }
                else {
                    index = next + 1;
                    counter = 1;
                }

                if (counter >= CancellationBytesRequired) return true;    
            }

            // No eligible CAN sequence found.
            return false;
        }

        private void CompleteOperation() {
            Completed?.Invoke(this, new CompletedEventArgs(_data));
            State = XModemStates.Idle;
        }

        private bool IncrementConsecutiveLoopsWithInsufficientCAN() {
            // 20 was abitrarily chosen limit.
            if (++_consecutiveLoopsWithCANs > 20) {
                _consecutiveLoopsWithCANs = 0;
                return false;
            }
            return true;
        }

        private byte[] CheckSum(IEnumerable<byte> packetInfo) {
            if (Mode == XModemMode.Checksum)
                return new byte[] { (byte)packetInfo.Sum(b => b) };
            else
                return CheckSumValidator.GetChecksum(packetInfo);
        }

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

        private void Abort(AbortedEventArgs e, bool sendCAN) {
            if (sendCAN) Port.Write(Enumerable.Repeat(CAN, CANsSentDuringAbort).ToArray());
            Aborted?.Invoke(this, e);
            State = XModemStates.Idle;
        }

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

        public void CancelOperation() {
            while(true) {
                switch (State) {
                    case XModemStates.ReceiverReceivingPackets:
                    case XModemStates.ReceiverSendingInitializationByte:
                    case XModemStates.SenderAwaitingInitializationFromReceiver:
                    case XModemStates.SenderPacketsBeingSent:
                        State = XModemStates.Cancelled;
                        break;
                    case XModemStates.Initializing:
                        continue;
                }
                break;
            }
        }
    }
}