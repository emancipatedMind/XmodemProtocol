using NUnit.Framework;
using XModemProtocol.Calculators;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocolTester
{
    [TestFixture] 
    public class TestICRCChecksumValidator {

        [Test] 
        public void TestCRCChecksumValidator() {

            ICRCChecksumCalculator calculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
            ICRCChecksumValidator validator = new CRCChecksumValidator(calculator);
            validator.ChecksumReference = new byte[2];

            Assert.IsTrue(validator.ValidateChecksum(new byte[] { 0xE5, 0xAD, 0x8B })); 
            Assert.IsFalse(validator.ValidateChecksum(new byte[] { 0xE5, 0xAA, 0x8B })); 
        }

    }

}
