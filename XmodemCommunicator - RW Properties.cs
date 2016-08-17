namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Port used to facilitate transfer.
        /// </summary>
        public System.IO.Ports.SerialPort Port { get; set; }

        /// <summary>
        /// Object used for CRC.
        /// </summary>
        public CRC16LTE CheckSumValidator { get; set; } = new CRC16LTE();
    }
}