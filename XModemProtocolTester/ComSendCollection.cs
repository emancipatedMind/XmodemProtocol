using System.Collections.Generic;

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
                if (value == null)
                    CollectionToSend = null;
                else
                    CollectionToSend = new List<List<byte>> { value };
            }
        }

        public List<List<byte>> CollectionToSend {
            set {
                if (value != null) 
                    _collectionToSend = new List<List<byte>>(value);
                _index = 0;
            }
        }

        public override int BytesInReadBuffer {
            get {
                if (_collectionToSend == null || _index >= _collectionToSend.Count) return 0;
                return _collectionToSend[_index].Count;
            }
        }
    }
}