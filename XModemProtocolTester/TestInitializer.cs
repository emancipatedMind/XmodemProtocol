using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Exceptions;
using XModemProtocol.Factories;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestInitializer {

        XModemProtocolOptions _options = new XModemProtocolOptions();
        IContext _context = new Context();
        ComSendCollection _com = new ComSendCollection();
        Requirements _requirements = new Requirements();

        CancellationTokenSource _cts;
        IInitializer _ini;

        private void Setup() {
            _cts = new CancellationTokenSource();
            _context.Token = _cts.Token;

            _requirements.Context = _context;
            _requirements.Options = _options;
            _requirements.Communicator = _com;
        }

        [Test]
        public void TestInitializeReceive()
        {
            _ini = new InitializeReceive();
            Setup();
            _options.ReceiverInitializationTimeout = 0;
            _options.ReceiverMaxNumberOfInitializationBytesForCRC = 0;
            _options.ReceiverMaxNumberOfInitializationBytesInTotal = 0;
            bool excThrown = false;

            _com.BytesToSend = null;
            List<List<byte>> expectedData = new List<List<byte>>();
            expectedData.AddRange(Enumerable.Repeat(new List<byte> { _options.C }, _options.ReceiverMaxNumberOfInitializationBytesForCRC));
            expectedData.AddRange(Enumerable.Repeat(new List<byte> { _options.NAK }, _options.ReceiverMaxNumberOfInitializationBytesInTotal - _options.ReceiverMaxNumberOfInitializationBytesForCRC));
            try {
                _ini.Initialize(_requirements);
            }
            catch (XModemProtocolException ex) {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);
            Assert.AreEqual(_com.BytesRead, expectedData);

            _com.BytesToSend = null;
            _com.BytesRead = null;
            _context.Mode = XModemProtocol.XModemMode.CRC;
            excThrown = false;
            try {
                _ini.Initialize(_requirements);
            }
            catch (XModemProtocolException ex) {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);
            Assert.AreEqual(_com.BytesRead, expectedData);

            _com.BytesToSend = null;
            _com.BytesRead = null;
            expectedData = new List<List<byte>>(Enumerable.Repeat(new List<byte> { _options.NAK }, _options.ReceiverMaxNumberOfInitializationBytesInTotal));
            _context.Mode = XModemProtocol.XModemMode.Checksum;
            excThrown = false;
            try {
                _ini.Initialize(_requirements);
            }
            catch (XModemProtocolException ex) {
                excThrown = ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);
            Assert.AreEqual(_com.BytesRead, expectedData);

            _com.BytesToSend = null;
            _com.BytesRead = null;
            excThrown = false;
            Task testRun = Task.Run(() => {
                try {
                    _com.BytesToSend = null;
                    _ini.Initialize(_requirements);
                }
                catch (XModemProtocolException ex) {
                    excThrown = 
                    ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.Cancelled;
                }
            });
            Task.Run(() => _cts.Cancel());
            testRun.Wait();
            Assert.IsTrue(excThrown);
        }

        [Test] 
        public void TestInitializeSend() {
            _ini = new InitializeSend();
            Setup();
            _context.Mode = XModemProtocol.XModemMode.OneK;

            _com.BytesToSend = new List<byte> { _options.C };

            _ini.Initialize(_requirements);
            Assert.AreEqual(XModemProtocol.XModemMode.OneK, _context.Mode);

            _context.Mode = XModemProtocol.XModemMode.CRC;
            _ini.Initialize(_requirements);
            Assert.AreEqual(XModemProtocol.XModemMode.CRC, _context.Mode);

            _com.BytesToSend = new List<byte> { _options.NAK };

            _ini.Initialize(_requirements);
            Assert.AreEqual(XModemProtocol.XModemMode.Checksum, _context.Mode);

            _com.BytesToSend = new List<byte> { _options.C };
            bool excThrown = false;
            try {
                _options.SenderInitializationTimeout = 1000;
                _ini.Initialize(_requirements);
            }
            catch(XModemProtocolException ex)  {
                excThrown = 
                ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);

            _com.BytesToSend = null;
            excThrown = false;
            try {
                _ini.Initialize(_requirements);
            }
            catch(XModemProtocolException ex)  {
                excThrown = 
                ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.InitializationFailed;
            }
            Assert.IsTrue(excThrown);

            excThrown = false;

            _cts = new CancellationTokenSource();
            _context.Token = _cts.Token;
            Task testRun = Task.Run(() => {
                try {
                    _com.BytesToSend = null;
                    _ini.Initialize(_requirements);
                }
                catch (XModemProtocolException ex) {
                    excThrown = 
                    ex.AbortArgs.Reason == XModemProtocol.XModemAbortReason.Cancelled;
                }
            });
            Task.Run(() => _cts.Cancel());
            testRun.Wait();
            Assert.IsTrue(excThrown);
        }
    }
}