using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmodemProtocol {
    public class XModemProtocolSenderOptions : XModemProtocolOptions, ICloneable {

        /// <summary>
        /// File to be loaded for use. Set to null in order to use last file.
        /// </summary>
        public string Filename { get; set; } = null;

        /// <summary>
        /// To be used if Receiver needs to be prompted.
        /// </summary>
        public List<byte> Prompt { get; set; } = null;

        public object Clone() {
            return new XModemProtocolSenderOptions {
                Filename = Filename,
                Prompt = Prompt,
                Mode = Mode,
            };
        }

    }
}