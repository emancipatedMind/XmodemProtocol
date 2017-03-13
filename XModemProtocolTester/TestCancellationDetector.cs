using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using XModemProtocol.Detectors;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestCancellationDetector {

        [Test]
        public void DetectorTest() {
            var rand = new RandomDataGenerator();
            ICancellationDetector detector = new CancellationDetector();
            var message = new List<byte>();
            XModemProtocolOptions options = new XModemProtocolOptions {
                CancellationBytesRequired = 10
            };

            message.Add(0x43);
            // Test to see if detector returns false under the condition that the message is too short.
            Assert.IsFalse(detector.CancellationDetected(message, options));

            message.AddRange(rand.GetRandomData(50));
            // Test to see if detector returns false under the condition that the message does not contain a cancel bytes.
            Assert.IsFalse(detector.CancellationDetected(message, options));

            message.AddRange(Enumerable.Repeat(options.CAN, 9));
            // Test to see if detector returns false under the condition that the message does not contain enough cancel bytes.
            Assert.IsFalse(detector.CancellationDetected(message, options));

            for (int i = 0; i < 3; i++) {
                message.Add(0x43);
                message.AddRange(Enumerable.Repeat(options.CAN, 9));
            }
            // Test to see if detector returns false under the condition that the message does not contain enough cancel bytes
            // contiguously even though it contains enough overall.
            Assert.IsFalse(detector.CancellationDetected(message, options));

            options.CancellationBytesRequired = 5;
            // Test to see if detector returns true under the condition that the message contains enough cancel bytes contiguously.
            Assert.IsTrue(detector.CancellationDetected(message, options));

            options.CancellationBytesRequired = 10;
            message.Add(0x43);
            message.AddRange(Enumerable.Repeat(options.CAN, 10));
            // Test to see if detector returns true under the condition that the message contains enough cancel bytes contiguously.
            Assert.IsTrue(detector.CancellationDetected(message, options));
        }
    }
}