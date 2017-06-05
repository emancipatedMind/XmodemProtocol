using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using XModemProtocol;
using XModemProtocol.Environment;
using XModemProtocol.Exceptions;
using XModemProtocol.Operations.Finalize;
using XModemProtocol.Operations.Invoke;

namespace XModemProtocolTester {
    [TestFixture]
    public class TestInvoker {

        static RandomDataGenerator _randomDataGenerator = new RandomDataGenerator();
        XModemMode[] _modes = (XModemMode[])System.Enum.GetValues(typeof(XModemMode));

        [Test]
        public void InvokeReceiveTest() {
            foreach(var m in _modes)
                TestModeForInvokeReceive(m);
        }

        private void TestModeForInvokeReceive(XModemMode mode) {
            int[] values = new int[] { 128, 130, 10000, 10240 };
            foreach(var v in values)
                RunTestForInvokeReceive(_randomDataGenerator.GetRandomData(v), mode);
        }

        private void RunTestForInvokeReceive(IEnumerable<byte> data, XModemMode mode) {
            var communicator = new ComSendCollection();
            var invoker = new InvokeReceive();
            var finalizer = new FinalizeReceive();
            var context = new Context {
                Mode = mode,
                Communicator = communicator,
            };

            var packets = context.Tools.Builder.GetPackets(data, context.Options);
            packets.Add(new List<byte> { context.Options.EOT });
            communicator.CollectionToSend = packets;

            invoker.Invoke(context);
            finalizer.Finalize(context);
            Assert.AreEqual(data, context.Data);
            Assert.AreEqual(packets.GetRange(0, packets.Count - 1), context.Packets);
        }

        [Test]
        public void InvokeSendTest() {
            foreach(var m in _modes)
                TestModeForInvokeSend(_randomDataGenerator.GetRandomData(10000), m);
        }

        private void TestModeForInvokeSend(IEnumerable<byte> _data, XModemMode mode) {
            var invoker = new InvokeSend();
            var cts = new CancellationTokenSource();
            var communicator = new ComSendCollection();
            var context = new Context {
                Token = cts.Token,
                Communicator = communicator,
                Mode = mode,
            };
            var expectedData = new List<List<byte>>(context.Packets);
            expectedData.Add(new List<byte> { context.Options.EOT });
            communicator.BytesToSend = new List<byte> { context.Options.ACK };
            communicator.BytesRead = new List<List<byte>>();
            invoker.Invoke(context);
            Assert.AreEqual(expectedData, communicator.BytesRead);
        }

        [Test]
        public void NAKResendTest() {
            var cts = new CancellationTokenSource();
            var communicator = new ComSendCollection();
            var invoker = new InvokeSend();
            var context = new Context {
                Communicator = communicator,
                Token = cts.Token,
            };

            context.Data = _randomDataGenerator.GetRandomData(10000).ToList();

            // Construct responses.
            // 1). Refuse first packet twice before accepting.
            // 2). Refuse second packet.
            // 3). Send non-sensical byte.
            // 4). Send cancellation request.
            var nakCollection = new List<byte> { context.Options.NAK };
            var ackCollection = new List<byte> { context.Options.ACK };
            var canCollection = Enumerable.Repeat(context.Options.CAN, context.Options.CancellationBytesRequired);
            communicator.CollectionToSend = new List<List<byte>> {
                nakCollection,
                nakCollection,
                ackCollection,
                nakCollection,
                new List<byte> { context.Options.SUB },
                canCollection.ToList()
            };

            // Construct expected data to be received.
            // 1). First packet should be sent thrice.
            // 2). Second packet should be sent twice.
            var expectedData = new List<List<byte>>();
            for (int i = 0; i < 3; i++)
                expectedData.Add(context.Packets[0]);
            for (int i = 0; i < 2; i++)
                expectedData.Add(context.Packets[1]);

            bool excThrown = false;
            try {
                communicator.BytesRead = new List<List<byte>>();
                invoker.Invoke(context);
            }
            catch (XModemProtocolException ex) {
                excThrown = ex.AbortArgs.Reason == XModemAbortReason.CancellationRequestReceived;
            }
            Assert.IsTrue(excThrown);
            Assert.AreEqual(expectedData, communicator.BytesRead);
        }

        [Test]
        public void InvokeReceive_Invoke_SenderNeverResponds_XModemProtocolExceptionThrownAfterConsecutiveNAKsSent() {
            var cts = new CancellationTokenSource();
            var communicator = new ComSendCollection();
            var invoker = new InvokeReceive();
            var options = new XModemProtocol.Options.XModemProtocolOptions() {
                ReceiverTimeoutDuringPacketReception = 500,
                ReceiverConsecutiveNAKsRequiredForCancellation = 3,
            };
            var context = new Context {
                Communicator = communicator,
                Token = cts.Token,
                Options = options,
            };

            var expectedData = new List<List<byte>>();
            for (int i = 0; i < options.ReceiverConsecutiveNAKsRequiredForCancellation; i++) {
                expectedData.Add(new List<byte> { options.NAK });
            }

            Assert.Throws<XModemProtocolException>(() => invoker.Invoke(context));
            for (int i = 0; i < expectedData.Count; i++) {
                Assert.IsTrue(communicator.BytesRead[i].SequenceEqual(expectedData[i]));
            }
        }

        [Test]
        public void InvokeReceive_Invoke_SenderRespondsOnlyTwice_XModemProtocolExceptionThrownAfterConsecutiveNAKsSent() {
            var cts = new CancellationTokenSource();
            var communicator = new ComSendCollection();
            var invoker = new InvokeReceive();
            var options = new XModemProtocol.Options.XModemProtocolOptions() {
                ReceiverTimeoutDuringPacketReception = 500,
                ReceiverConsecutiveNAKsRequiredForCancellation = 3,
            };
            var context = new Context {
                Communicator = communicator,
                Token = cts.Token,
                Options = options,
            };
            List<byte> data = _randomDataGenerator.GetRandomData(2000).ToList();
            var packets = context.Tools.Builder.GetPackets(data, context.Options);
            communicator.CollectionToSend = packets;

            var expectedData = new List<List<byte>>();
            expectedData.Add(new List<byte> { options.ACK });
            expectedData.Add(new List<byte> { options.ACK });
            for (int i = 0; i < options.ReceiverConsecutiveNAKsRequiredForCancellation; i++) {
                expectedData.Add(new List<byte> { options.NAK });
            }

            Assert.Throws<XModemProtocolException>(() => invoker.Invoke(context));
            for (int i = 0; i < expectedData.Count; i++) {
                Assert.IsTrue(communicator.BytesRead[i].SequenceEqual(expectedData[i]));
            }
        }
    }
}
