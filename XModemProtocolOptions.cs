using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XmodemProtocol {
    public abstract class XModemProtocolOptions {

        private int _cancellationBytesRequired = 5;

        public XModemMode Mode { get; set; } = XModemMode.Auto;
        public int CancellationBytesRequired {
            get { return _cancellationBytesRequired; }
            set {
                if (value < 0) return;
                _cancellationBytesRequired = value;
            }
        }

    }
}