using System.IO;
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

        /// <summary>
        /// Save contents of Data property to file.
        /// </summary>
        /// <param name="filename">Complete filename where to save data.</param>
        public void SaveContents(string filename) {
            File.WriteAllBytes(filename, Data.ToArray()); 
        }

    }
}