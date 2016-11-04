using NUnit.Framework;
using System.Linq;
using XModemProtocol.Calculators;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestChecksumCalculator {

        [Test] 
        public void NormalChecksumCalculatorTest() {
            ISummationChecksumCalculator _calculator = new NormalChecksumCalculator();
            byte[] collection = new byte[] { 0x00, 0x19, 0x88, 0x33, 0x72, };
            byte actualChecksum = (byte)collection.Sum(currentByte => currentByte);
            byte calculatedChecksum = _calculator.CalculateChecksum(collection).ElementAt(0);
            // Test to see if calculator properly calculates checksum.
            Assert.AreEqual(actualChecksum, calculatedChecksum);
        }

        [Test]
        public void CRCChecksumCalculatorTest() {
            ICRCChecksumCalculator _calculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
            // Test to see if calculator properly calculates checksum.
            Assert.True(Enumerable.SequenceEqual(_calculator.CalculateChecksum(new byte[] { 0xD9 }), new byte[] { 0x5A, 0x54 }));
            Assert.True(Enumerable.SequenceEqual(_calculator.CalculateChecksum(new byte[] { 0xA5 }), new byte[] { 0xE5, 0x4F }));
            Assert.True(Enumerable.SequenceEqual(_calculator.CalculateChecksum(new byte[] { 0xE5, 0xAD, 0x8B }), new byte[] { 0x00, 0x00 }));
        }

    }

}
