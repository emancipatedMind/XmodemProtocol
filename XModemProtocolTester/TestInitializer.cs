using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Environment;
using XModemProtocol.Exceptions;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestInitializer {

        [Test]
        public void InitializeReceiveTest() {
            IContext context = new Context();
            var communicator = new ComSendCollection();
            var cts = new CancellationTokenSource();
            IInitializer ini = new InitializeReceive();
            var options = new XModemProtocolOptions();
            bool excThrown = false;

            // Attach communicator.
            context.Communicator = communicator;

            // Drop options to minimums allowed.
            options.ReceiverInitializationTimeout = 0;
            options.ReceiverMaxNumberOfInitializationBytesForCRC = 0;
            options.ReceiverMaxNumberOfInitializationBytesInTotal = 0;
            context.Options = options;

            // Modes to test in succeeding loop.
            var modes = new XModemProtocol.XModemMode[] {
                XModemProtocol.XModemMode.OneK,
                XModemProtocol.XModemMode.CRC,
                XModemProtocol.XModemMode.Checksum,
            };

            foreach(var mode in modes) {
                // Update mode.
                context.Mode = mode;

                // Construct expected data from operation.
                var expectedData = new List<List<byte>>();
                switch (mode) {
                    case XModemProtocol.XModemMode.Checksum:
                        expectedData = new List<List<byte>>(Enumerable.Repeat(new List<byte> { context.Options.NAK }, context.Options.ReceiverMaxNumberOfInitializationBytesInTotal));
                        break;
                    default:
                        expectedData.AddRange(Enumerable.Repeat(new List<byte> { context.Options.C }, context.Options.ReceiverMaxNumberOfInitializationBytesForCRC));
                        expectedData.AddRange(Enumerable.Repeat(new List<byte> { context.Options.NAK }, context.Options.ReceiverMaxNumberOfInitializationBytesInTotal - context.Options.ReceiverMaxNumberOfInitializationBytesForCRC));
                        break;
                }

                // Run test, and catch inevitable XModemProtocolException.
                excThrown = false;
                try {
                    ini.Initialize(context);
                }
                catch (XModemProtocolException ex) {
                    excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
                }

                // Test to see if XModemProtocolException was thrown, and reason was XModemProtocol.XModemAbortReason.InitializationFailed.
                Assert.IsTrue(excThrown);

                // Test to see if communicator read expected data which was receiver attempting to begin communication. 
                Assert.AreEqual(expectedData, communicator.BytesRead);

                // Clear communicator buffers.
                communicator.BytesToSend = null;
                communicator.BytesRead = null;
            }

            // Test cancellation function of initializer.
            excThrown = false;
            context.Token = cts.Token;
            Task testRun = Task.Run(() => {
                try {
                    ini.Initialize(context);
                }
                catch (XModemProtocolException ex) {
                    excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.Cancelled;
                }
            });
            Task.Run(() => cts.Cancel());
            testRun.Wait();

            // Test to see if XModemProtocolException was thrown, and reason was XModemProtocol.XModemAbortReason.Cancelled.
            Assert.IsTrue(excThrown);
        }

        [Test] 
        public void InitializeSendTest() {
            IContext context = new Context();
            var communicator = new ComSendCollection();
            var cts = new CancellationTokenSource();
            IInitializer ini = new InitializeSend();
            var options = new XModemProtocolOptions();
            bool excThrown = false;

            // Attach communicator.
            context.Communicator = communicator;

            // Drop options to speed up test.
            options.SenderInitializationTimeout = 1000;
            context.Options = options;

            // 1). Test to see if CRC initialization is accepted by OneK, and CRC
            //     and if checksum initialization is accepted by Checksum.

            // Modes to test in succeeding loop.
            var modes = new XModemProtocol.XModemMode[] {
                XModemProtocol.XModemMode.OneK,
                XModemProtocol.XModemMode.CRC,
                XModemProtocol.XModemMode.Checksum,
            };

            foreach(var mode in modes) {
                context.Mode = mode;

                // Assign proper initialization byte by mode.
                switch (mode) {
                    case XModemProtocol.XModemMode.Checksum:
                        communicator.BytesToSend = new List<byte> { context.Options.NAK };
                        break;
                    default:
                        communicator.BytesToSend = new List<byte> { context.Options.C };
                        break;
                }

                ini.Initialize(context);

                // Test to see if mode is does not change.
                Assert.AreEqual(mode, context.Mode);
            }

            // 2). Test to see if checksum byte downgrades CRC and OneK to 
            //     Checksum.

            // Modes to test in succeeding loop.
            modes = new XModemProtocol.XModemMode[] {
                XModemProtocol.XModemMode.OneK,
                XModemProtocol.XModemMode.CRC,
            };

            communicator.BytesToSend = new List<byte> { context.Options.NAK };
            foreach(var mode in modes) {
                context.Mode = mode;

                // Test to see if mode changes.
                ini.Initialize(context);
                Assert.AreEqual(XModemProtocol.XModemMode.Checksum, context.Mode);
            }

            // 3). Test to see if Checksum mode will not respond to CRC initialization.

            communicator.BytesToSend = new List<byte> { context.Options.C };
            try {
                ini.Initialize(context);
            }
            catch(XModemProtocolException ex)  {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            // Test to see if XModemProtocolException was thrown, and reason was XModemProtocol.XModemAbortReason.InitializationFailed.
            Assert.IsTrue(excThrown);

            // 4). Test timeout function of initializer.

            communicator.BytesToSend = null;
            excThrown = false;
            try {
                ini.Initialize(context);
            }
            catch(XModemProtocolException ex)  {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            // Test to see if XModemProtocolException was thrown, and reason was XModemProtocol.XModemAbortReason.InitializationFailed.
            Assert.IsTrue(excThrown);

            // 5). Test cancellation function of initializer.
            excThrown = false;
            context.Token = cts.Token;
            Task testRun = Task.Run(() => {
                try {
                    ini.Initialize(context);
                }
                catch (XModemProtocolException ex) {
                    excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.Cancelled;
                }
            });
            Task.Run(() => cts.Cancel());
            testRun.Wait();

            // Test to see if XModemProtocolException was thrown, and reason was XModemProtocol.XModemAbortReason.Cancelled.
            Assert.IsTrue(excThrown);
        }
    }
}