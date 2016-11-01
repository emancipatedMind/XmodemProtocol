using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using XModemProtocol.Detectors;
using XModemProtocol.Options;

namespace XModemProtocolTester
{
    [TestFixture] 
    public class TestCancellationDetector {

        static XModemProtocolOptions _options = new XModemProtocolOptions();
        static List<byte> _message;
        static ICancellationDetector _detector = CancellationDetector.Instance;

        [Test]
        public void TestDetector() {

            _options.CancellationBytesRequired = 10;

            _message = new List<byte>();
            _message.Add(0x43);

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.AddRange(Enumerable.Repeat((byte) 0x43, 50));

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));
            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));
            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _options.CancellationBytesRequired = 5;

            Assert.IsTrue(_detector.CancellationDetected(_message, _options));

            _options.CancellationBytesRequired = 10;

            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 10));

            Assert.IsTrue(_detector.CancellationDetected(_message, _options));
        }

    }

}
