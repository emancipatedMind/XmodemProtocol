using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Exceptions;
using XModemProtocol.Factories;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestInitializer {
        [Test] 
        public void TestInitializeSend() {
            XModemProtocolOptions options = new XModemProtocolOptions();
            IContext context = new Context();
            ComStandIn com = new ComStandIn();
            CancellationTokenSource cts = new CancellationTokenSource();
            context.Token = cts.Token;

            IInitializer ini = new InitializeSend();
            Requirements requirements = new Requirements();
            requirements.Context = context;
            requirements.ToolFactory = new XModemToolFactory();
            requirements.Options = options;
            requirements.Communicator = com;

            Assert.AreEqual(XModemProtocol.XModemMode.OneK, options.Mode);

            com.BytesToSend = new List<byte> { options.C };

            ini.Initialize(requirements);
            Assert.AreEqual(XModemProtocol.XModemMode.OneK, options.Mode);

            options.Mode = XModemProtocol.XModemMode.CRC;
            ini.Initialize(requirements);
            Assert.AreEqual(XModemProtocol.XModemMode.CRC, options.Mode);

            com.BytesToSend = new List<byte> { options.NAK };

            ini.Initialize(requirements);
            Assert.AreEqual(XModemProtocol.XModemMode.Checksum, options.Mode);

            com.BytesToSend = new List<byte> { options.C };
            bool excThrown = false;
            try {
                options.SenderInitializationTimeout = 1000;
                ini.Initialize(requirements);
            }
            catch(XModemProtocolException ex)  {
                excThrown = 
                ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);

            com.BytesToSend = null;
            excThrown = false;
            try {
                ini.Initialize(requirements);
            }
            catch(XModemProtocolException ex)  {
                excThrown = 
                ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);

            excThrown = false;

            Task testRun = Task.Run(() => {
                try {
                    com.BytesToSend = new List<byte> { options.NAK };
                    ini.Initialize(requirements);
                }
                catch (XModemProtocolException ex) {
                    excThrown = 
                    ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
                }
            });
            Task.Run(() => cts.Cancel());
            testRun.Wait();
            Assert.IsTrue(excThrown);
            cts = new CancellationTokenSource();
            context.Token = cts.Token;
        }
    }
}