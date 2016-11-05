using NUnit.Framework;
using System.Collections.Generic;
using XModemProtocol.Calculators;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestNormalValidator {

        [Test] 
        public void NormalChecksumValidatorTest() {
            byte validLSB = 0x46;
            byte invalidLSB = 0x95;
            var data = new byte[] { 0x00, 0x19, 0x88, 0x33, 0x72, };

            var validatedMessage = new List<byte>();
            validatedMessage.AddRange(data);
            validatedMessage.Add(validLSB);

            var invalidatedMessage = new List<byte>();
            invalidatedMessage.AddRange(data);
            invalidatedMessage.Add(invalidLSB);

            IChecksumValidator validator = new NormalChecksumValidator(new NormalChecksumCalculator());

            Assert.IsTrue(validator.ValidateChecksum(validatedMessage)); 
            Assert.IsFalse(validator.ValidateChecksum(invalidatedMessage)); 
        }
    }
}