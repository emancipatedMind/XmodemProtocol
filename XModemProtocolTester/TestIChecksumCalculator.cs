using NUnit.Framework;
using System.Linq;
using XModemProtocol.Calculators;

namespace XModemProtocolTester
{
    [TestFixture] 
    public class TestIChecksumCalculator {

        byte[] _collection = new byte[] { 0x00, 0x19, 0x88, 0x33, 0x72, };

        [Test] 
        public void TestChecksumCalculator() {
            IChecksumCalculator calculator = new NormalChecksumCalculator();
            int checksum = _collection.Sum(currentByte => currentByte);
            byte lsb = (byte)(checksum & 0xFF);
            Assert.AreEqual(lsb, calculator.CalculateChecksum(_collection).ElementAt(0));
        }

    }

}
