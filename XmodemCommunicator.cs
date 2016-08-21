namespace XModemProtocol {
    public partial class XModemCommunicator {

        /// <summary>
        /// Initializes a new instance of the XModemProtocol.XModemCommunicator class.
        /// </summary>
        /// <param name="port">Port used to facilitate transfer.</param>
        public XModemCommunicator(System.IO.Ports.SerialPort port) {
            Port = port;
            if (Port.IsOpen == false) Port.Open();
            
        }

    }
}