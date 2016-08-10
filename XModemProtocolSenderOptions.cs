using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocol {
    /// <summary>
    /// Options to be used when playing sender role. Buffer is the preferred property. If it is null
    /// XModemCommunicator will look to load the file specified by the Filename property. If that fails, 
    /// or is null, XModemCommunicator will check its own memory for bytes to send. If this also fails,
    /// the operation is aborted.
    /// </summary>
    public class XModemProtocolSenderOptions : XModemProtocolOptions, ICloneable {

        /// <summary>
        /// File to be loaded for use.
        /// </summary>
        public string Filename { get; set; } = null;

        /// <summary>
        /// To be used if Receiver needs to be prompted.
        /// </summary>
        public IEnumerable<byte> Prompt { get; set; } = null;

        /// <summary>
        /// Bytes to be used in Send operation.
        /// </summary>
        public IEnumerable<byte> Buffer { get; set; } = null;

        public object Clone() {
            return new XModemProtocolSenderOptions {
                Buffer =  Buffer == null ? null : new List<byte>(Buffer),
                Prompt =  Prompt == null ? null : new List<byte>(Prompt),
                Mode = Mode,
                Filename = Filename,
            };
        }

    }
}