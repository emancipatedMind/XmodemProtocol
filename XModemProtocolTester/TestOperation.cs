using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XModemProtocol.Environment;
using XModemProtocol.Operations;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestOperation {

        static RandomDataGenerator _rdg = new RandomDataGenerator();

        [Test]
        public void SendOperationTest() {
            RunTest(TestModeForSendOperation);
        }

        [Test]
        public void ReceiveOperationTest() {
            RunTest(TestModeForReceiveOperation);
        }

        void RunTest(Action<XModemProtocol.XModemMode> method) {
            var modes = new XModemProtocol.XModemMode[] {
                XModemProtocol.XModemMode.Checksum,
                XModemProtocol.XModemMode.CRC,
                XModemProtocol.XModemMode.OneK,
            };
            foreach (var mode in modes)
                method(mode);
        }

        void TestModeForSendOperation(XModemProtocol.XModemMode mode) {
            var communicator = new ComSendCollection();
            var context = new Context {
                Mode = mode,
                Data = _rdg.GetRandomData(40000).ToList(),
                Communicator = communicator,
            };
            var dts = new List<List<byte>> {
                new List<byte> { mode == XModemProtocol.XModemMode.Checksum ? context.Options.NAK : context.Options.C }
            };
            double payloadCount = mode == XModemProtocol.XModemMode.OneK ? 1024.0 : 128.0;
            int ackCount = (int)Math.Ceiling(context.Data.Count / payloadCount) + 1;
            dts.AddRange(Enumerable.Repeat(new List<byte> { context.Options.ACK }, ackCount));
            communicator.CollectionToSend = dts;
            IOperation operation = new SendOperation();
            operation.Go(context);
            var expectedData = new List<List<byte>>(context.Packets);
            expectedData.Add(new List<byte> { context.Options.EOT});
            Assert.AreEqual(expectedData, communicator.BytesRead);
        }

        void TestModeForReceiveOperation(XModemProtocol.XModemMode mode) {
            var randomData = _rdg.GetRandomData(40000).ToList();
            var communicator = new ComSendCollection();
            var context = new Context {
                Mode = mode,
                Data = randomData,
                Communicator = communicator,
            };
            var cts = new List<List<byte>>(context.Packets);
            cts.Add(new List<byte> {context.Options.EOT});
            var expectedData = new List<List<byte>> {
                new List<byte> { context.Mode == XModemProtocol.XModemMode.Checksum ? context.Options.NAK : context.Options.C },
            };
            expectedData.AddRange(Enumerable.Repeat(new List<byte> { context.Options.ACK }, cts.Count).ToList());
            IOperation operation = new ReceiveOperation();
            context.Data = new List<byte>();
            communicator.CollectionToSend = cts;
            operation.Go(context);
            Assert.AreEqual(expectedData, communicator.BytesRead);
        }
    }
}