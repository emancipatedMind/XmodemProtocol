using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XModemProtocol.Exceptions;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Options;
using XModemProtocol.Operations.Invoke;
using NUnit.Framework;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestInvoke {

        private SendReceiveRequirements _req = new SendReceiveRequirements();
        private XModemProtocolOptions _options = new XModemProtocolOptions();
        private IXModemTools _tools;
        private Context _context = new Context();
        private ComSendCollection _com = new ComSendCollection();
        private CancellationTokenSource _cts;
        private IInvoker _invoker = new InvokeSend();
        private List<List<byte>> _sentData;
        private IEnumerable<byte> _data;
        private XModemToolFactory _toolFactory;
        private RandomDataGenerator _randomDataGenerator = new RandomDataGenerator {
            Domain = 0x5E,
            Offset = 0x20,
        };

        [Test] 
        public void TestInvokeSend() {

            _cts = new CancellationTokenSource();
            _context.Token = _cts.Token;

            _toolFactory = new XModemToolFactory();

            _req.Context = _context;
            _req.Communicator = _com;
            _req.Options = _options;


            _data = _randomDataGenerator.GetRandomData(10000);
            TestMode(XModemProtocol.XModemMode.Checksum);
            TestMode(XModemProtocol.XModemMode.CRC);
            TestMode(XModemProtocol.XModemMode.OneK);
        }

        private void TestMode(XModemProtocol.XModemMode mode) {
            _options.Mode = mode;
            _tools = _toolFactory.GetToolsFor(_options.Mode);
            _req.Detector = _tools.Detector;
            _req.Context.Packets = _tools.Builder.GetPackets(_data, _options);
            _sentData = new List<List<byte>>(_req.Context.Packets);
            _sentData.Add(new List<byte> { _options.EOT });
            _com.BytesToSend = new List<byte> { _options.ACK };
            _invoker.Invoke(_req);
            Assert.AreEqual(_sentData, _com.BytesRead);
            _com.Flush();
        }

        [Test] 
        public void TestNAKResend() {
            _cts = new CancellationTokenSource();
            _context.Token = _cts.Token;

            _req.Context = _context;
            _req.Communicator = _com;
            _req.Options = _options;

            _toolFactory = new XModemToolFactory();

            _data = _randomDataGenerator.GetRandomData(10000);
            var nakCollection = new List<byte> { _options.NAK };
            var ackCollection = new List<byte> { _options.ACK };
            var canCollection = Enumerable.Repeat((byte) _options.CAN, _options.CancellationBytesRequired); 
            _tools = _toolFactory.GetToolsFor(_options.Mode);
            _req.Detector = _tools.Detector;
            _req.Context.Packets = _tools.Builder.GetPackets(_data, _options);
            _com.CollectionToSend = new List<List<byte>> {
                nakCollection,
                nakCollection,
                ackCollection,
                nakCollection,
                canCollection.ToList()
            };
            _sentData = new List<List<byte>>();

            for (int i = 0; i < 3; i++)
                _sentData.Add(_req.Context.Packets[0]);

            for (int i = 0; i < 2; i++)
                _sentData.Add(_req.Context.Packets[1]);

            bool excThrown = false;
            try {
                _invoker.Invoke(_req);
            }
            catch (XModemProtocolException ex) {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.CancellationRequestReceived;
            }
            Assert.IsTrue(excThrown);
            Assert.AreEqual(_sentData, _com.BytesRead);

        }
    }
}