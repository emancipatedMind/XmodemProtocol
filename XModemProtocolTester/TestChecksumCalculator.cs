namespace XModemProtocolTester {
    using NUnit.Framework;
    using System.Linq;
    using System.Collections.Generic;
    using XModemProtocol.Calculators;
    [TestFixture]
    public class TestChecksumCalculator {

        [Test]
        public void NormalChecksumCalculatorTest() {
            PerformCheckSumCalculatorTest(
                new byte[][] {
                    new byte[] { 0x00, 0x19, 0x88, 0x33, 0x72, },
                },
                new byte[][] {
                        new byte[] { 0x46, },
                },
                new List<ISummationChecksumCalculator> {
                    new NormalChecksumCalculator(),
                    new FunctionalNormalChecksumCalculator(),
            });
        }

        [Test]
        public void CRCChecksumCalculatorTest() {
            PerformCheckSumCalculatorTest(
                new byte[][] {
                    new byte[] { 0xD9 },
                    new byte[] { 0xA5 },
                    new byte[] { 0xE5, 0xAD, 0x8B },
                },
                new byte[][] {
                        new byte[] { 0x5A, 0x54 },
                        new byte[] { 0xE5, 0x4F },
                        new byte[] { 0x00, 0x00 },
                },
                new List<ICRCChecksumCalculator> {
                    new CRCChecksumCalculator(new FunctionalLookUpTable(0x1021)),
                    new FunctionalCRCChecksumCalculator(new FunctionalLookUpTable(0x1021)),
            });
        }

        private void PerformCheckSumCalculatorTest(
            IEnumerable<IEnumerable<byte>> collection,
            IEnumerable<IEnumerable<byte>> checksum,
            IEnumerable<IChecksumCalculator> calculators
        ) {
            collection
            .Zip(
                checksum,
                CombineCollectionWithCheckSum
            )
            .ToList()
            .ForEach(test =>
                calculators
                .ToList()
                .ForEach(c =>
                    Assert.AreEqual(test.CheckSum, c.CalculateChecksum(test.Collection))
                ));
        }

        private (IEnumerable<byte> Collection, IEnumerable<byte> CheckSum) CombineCollectionWithCheckSum(IEnumerable<byte> coll, IEnumerable<byte> checksum) =>
            (coll, checksum);

    }
}