using NUnit.Framework;
using System.Collections.Generic;
using XModemProtocol.Calculators;
using XModemProtocol.Validators.Checksum;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestNormalValidator {
        
        byte[] _collection = new byte[] { 0x00, 0x19, 0x88, 0x33, 0x72, };

        [Test] 
        public void TestNormalChecksumValidator() {
            byte validLSB = 0x46;
            byte invalidLSB = 0x95;
            List<byte> validatedMessage = new List<byte>();
            validatedMessage.AddRange(_collection);
            validatedMessage.Add(validLSB);

            List<byte> invalidatedMessage = new List<byte>();
            invalidatedMessage.AddRange(_collection);
            invalidatedMessage.Add(invalidLSB);

            IChecksumValidator validator = new NormalChecksumValidator(new NormalChecksumCalculator());

            Assert.IsTrue(validator.ValidateChecksum(validatedMessage)); 
            Assert.IsFalse(validator.ValidateChecksum(invalidatedMessage)); 
        }
    }
}
