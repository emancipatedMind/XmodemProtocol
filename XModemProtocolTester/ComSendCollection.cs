using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XModemProtocolTester {
    public class ComSendCollection : ComStandIn {
        List<List<byte>> _collectionToSend;
        int _index;

        public override List<byte> BytesToSend {
            get {
                if (_collectionToSend == null) return null;
                else if (_collectionToSend.Count == 1) return _collectionToSend[0];
                else if (_index == _collectionToSend.Count) return null;
                return _collectionToSend[_index++];
            }
            set {
                if (value == null) {
                    CollectionToSend = null;
                }
                else
                    CollectionToSend = new List<List<byte>> { value };
            }
        }

        public List<List<byte>> CollectionToSend {
            set {
                _collectionToSend = value;
                _index = 0;
            }
        }

        public override bool ReadBufferContainsData {
            get {
                if (_collectionToSend == null) return false;
                if (_collectionToSend.Count == 1) return true;
                if (_index >= _collectionToSend.Count) return false;
                else return true;
            }
        }
    }
}
