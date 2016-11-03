using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XModemProtocol.Exceptions;
using XModemProtocol.Operations.Finalize;
using XModemProtocol.Operations.Invoke;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestInvoker {

        private Context _context = new Context();
        private ComSendCollection _com = new ComSendCollection();
        private CancellationTokenSource _cts;
        private IInvoker _invoker;
        private IFinalizer _finalizer;
        private List<List<byte>> _sentData;
        private List<List<byte>> _packets;
        private IEnumerable<byte> _data;
        private RandomDataGenerator _randomDataGenerator = new RandomDataGenerator {
            Domain = 0x5E,
            Offset = 0x20,
        };

        [Test]
        public void TestInvokeReceive() {
            TestModeForInvokeReceive();
            _context.Mode = XModemProtocol.XModemMode.CRC;
            TestModeForInvokeReceive();
            _context.Mode = XModemProtocol.XModemMode.Checksum;
            TestModeForInvokeReceive();
        }

        private void TestModeForInvokeReceive() {
            _data = _randomDataGenerator.GetRandomData(128);
            RunTestForInvokeReceive();
            _data = _randomDataGenerator.GetRandomData(130);
            RunTestForInvokeReceive();
            _data = _randomDataGenerator.GetRandomData(10000);
            RunTestForInvokeReceive();
            _data = _randomDataGenerator.GetRandomData(10240);
            RunTestForInvokeReceive();
        }

        private void RunTestForInvokeReceive() {
            _invoker = new InvokeReceive();
            _finalizer = new FinalizeReceive();
            _context.Data = new List<byte>();

            _packets = _context.Tools.Builder.GetPackets(_data, _context.Options);
            _packets.Add(new List<byte> { _context.Options.EOT });
            _com.CollectionToSend = _packets;

            _context.Communicator = _com;

            _invoker.Invoke(_context);
            _finalizer.Finalize(_context);
            Assert.AreEqual(_data, _context.Data); 
            Assert.AreEqual(_packets.GetRange(0, _packets.Count - 1), _context.Packets); 
        }


        [Test] 
        public void TestInvokeSend() {
            _invoker = new InvokeSend();
            _cts = new CancellationTokenSource();
            _context.Token = _cts.Token;

            _context.Communicator = _com;

            _data = _randomDataGenerator.GetRandomData(10000);
            TestMode(XModemProtocol.XModemMode.Checksum);
            TestMode(XModemProtocol.XModemMode.CRC);
            TestMode(XModemProtocol.XModemMode.OneK);
        }

        private void TestMode(XModemProtocol.XModemMode mode) {
            _context.Mode = mode;
            _sentData = new List<List<byte>>(_context.Packets);
            _sentData.Add(new List<byte> { _context.Options.EOT });
            _com.BytesToSend = new List<byte> { _context.Options.ACK };
            _com.BytesRead = new List<List<byte>>();
            _invoker.Invoke(_context);
            Assert.AreEqual(_sentData, _com.BytesRead);
            _com.Flush();
        }

        [Test] 
        public void TestNAKResend() {
            _cts = new CancellationTokenSource();
            _context.Token = _cts.Token;

            _context.Communicator = _com;

            _context.Data = _randomDataGenerator.GetRandomData(10000).ToList();
            var nakCollection = new List<byte> { _context.Options.NAK };
            var ackCollection = new List<byte> { _context.Options.ACK };
            var canCollection = Enumerable.Repeat((byte) _context.Options.CAN, _context.Options.CancellationBytesRequired); 
            _com.CollectionToSend = new List<List<byte>> {
                nakCollection,
                nakCollection,
                ackCollection,
                nakCollection,
                new List<byte> { _context.Options.CAN },
                canCollection.ToList()
            };
            _sentData = new List<List<byte>>();

            for (int i = 0; i < 3; i++)
                _sentData.Add(_context.Packets[0]);

            for (int i = 0; i < 2; i++)
                _sentData.Add(_context.Packets[1]);

            bool excThrown = false;
            try {
                _com.BytesRead = new List<List<byte>>();
                _invoker = new InvokeSend();
                _invoker.Invoke(_context);
            }
            catch (XModemProtocolException ex) {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.CancellationRequestReceived;
            }
            Assert.IsTrue(excThrown);
            Assert.AreEqual(_sentData, _com.BytesRead);
        }
    }
}