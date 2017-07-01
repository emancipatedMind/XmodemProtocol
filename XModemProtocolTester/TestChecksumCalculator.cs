namespace XModemProtocolTester {
    using NUnit.Framework;
    using System.Linq;
    using System.Collections.Generic;
    using XModemProtocol.Calculators;
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
            new byte[][] {
                new byte[] { 0xD9 },
                new byte[] { 0xA5 },
                new byte[] { 0xE5, 0xAD, 0x8B },
            }
            .Zip(new byte[][] {
                    new byte[] { 0x5A, 0x54 },
                    new byte[] { 0xE5, 0x4F },
                    new byte[] { 0x00, 0x00 },
                },
                CombineCollectionWithCheckSum
            )
            .ToList()
            .ForEach(test =>
                new List<ICRCChecksumCalculator> {
                    new CRCChecksumCalculator(new FunctionalLookUpTable(0x1021)),
                }
                .ForEach(c =>
                    Assert.AreEqual(test.CheckSum, c.CalculateChecksum(test.Collection))
                ));
        }

        private (byte[] Collection, byte[] CheckSum) CombineCollectionWithCheckSum(byte[] coll, byte[] checksum) =>
            (coll, checksum);

    }

}
