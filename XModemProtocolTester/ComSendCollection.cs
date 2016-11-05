using System.Collections.Generic;

namespace XModemProtocolTester {
    public class ComSendCollection : ComStandIn {
        List<List<byte>> _collectionToSend = new List<List<byte>>();
        int _index = 0;

        public override List<byte> BytesToSend {
            get {
                if (_collectionToSend.Count == 1) return _collectionToSend[0];
                if (_collectionToSend.Count == 0
                    || _index == _collectionToSend.Count) return new List<byte>();
                return _collectionToSend[_index++];
            }
            set {
                if (value == null)
                    CollectionToSend = new List<List<byte>>();
                else
                    CollectionToSend = new List<List<byte>> { value };
            }
        }

        public List<List<byte>> CollectionToSend {
            set {
                if (value != null) {
                    _collectionToSend = new List<List<byte>>(value);
                    _index = 0;
                }
            }
        }

        public override int BytesInReadBuffer {
            get {
                if (_index >= _collectionToSend.Count) return 0;
                return _collectionToSend[_index].Count;
            }
        }
    }
}