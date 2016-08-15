namespace XModemProtocol {
    public partial class XModemCommunicator {

        public XModemCommunicator(System.IO.Ports.SerialPort port) {
            Port = port;
            if (Port.IsOpen == false) Port.Open();
        }

    }
}