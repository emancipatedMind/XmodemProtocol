using NUnit.Framework;
using System.Linq;
using XModemProtocol.Calculators;

namespace XModemProtocolTester
{
    [TestFixture] 
    public class TestICRCChecksumCalculator {

        ICRCChecksumCalculator _crc = new CRCChecksumCalculator(new LookUpTable(0x1021));

        [Test]
        public void TestValuesD9AndA5InICRCChecksumCalculator() {
            Assert.True(Enumerable.SequenceEqual(_crc.CalculateChecksum(new byte[] { 0xD9 }), new byte[] { 0x5A, 0x54 }));
            Assert.True(Enumerable.SequenceEqual(_crc.CalculateChecksum(new byte[] { 0xA5 }), new byte[] { 0xE5, 0x4F }));
        }

        [Test]
        public void TestE5AD8BInICRCChecksumCalculator() {
            Assert.True(Enumerable.SequenceEqual(_crc.CalculateChecksum(new byte[] { 0xE5, 0xAD, 0x8B }), new byte[] { 0x00, 0x00 }));
        }
    }
}
