using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using XModemProtocol.Calculators;
using XModemProtocol.Options;
using XModemProtocol.Validators.Checksum;
using XModemProtocol.Validators.Packet;

namespace XModemProtocolTester {
    [TestFixture]
    public class TestPacketValidator {
        static IXModemProtocolOptions _options = new XModemProtocolOptions();

        static ISummationChecksumCalculator _calculator = new NormalChecksumCalculator();
        static IChecksumValidator _normalChecksumValidator = new NormalChecksumValidator(_calculator);
        IPacketValidator _validator = new PacketValidator(_normalChecksumValidator);

        static ICRCChecksumCalculator _cRCCalculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
        static ICRCChecksumValidator _crcChecksumValidator = new CRCChecksumValidator(_cRCCalculator);
        IPacketValidator _crcValidator = new PacketValidator(_crcChecksumValidator);

        IEnumerable<byte> _shortPacketHeader = new byte[] { 0x01, 0x01, 0xFE };
        IEnumerable<byte> _shortPacketMessage = Enumerable.Repeat((byte)0x43,128);

        IEnumerable<byte> _longPacketHeader = new byte[] { 0x02, 0x01, 0xFE };
        IEnumerable<byte> _longPacketMessage = Enumerable.Repeat((byte)0x43,1024);

        byte _shortPacketChecksum = 0x80;
        IEnumerable<byte> _shortPacketCRCChecksum = new byte[] { 0x9E, 0xB0 };
        IEnumerable<byte> _longPacketCRCChecksum = new byte[] { 0x83, 0xE9 };

        List<byte> _shortPacket;
        List<byte> _longPacket;

        [Test]
        public void OneKPacketValidatorTest() {
            _longPacket = new List<byte>(_longPacketHeader);
            _longPacket.AddRange(_longPacketMessage);
            _longPacket.AddRange(_longPacketCRCChecksum);
            // Test normal validator.
            Assert.AreEqual(ValidationResult.Valid, _crcValidator.ValidatePacket(_longPacket, _options));
            // Test resend of packet.
            Assert.AreEqual(ValidationResult.Duplicate, _crcValidator.ValidatePacket(_longPacket, _options));
            _longPacket[131] = 0x73;
            _crcValidator.Reset();
            // Test incorrect checksum.
            Assert.AreEqual(ValidationResult.Invalid, _crcValidator.ValidatePacket(_longPacket, _options));
        }

        [Test]
        public void NormalPacketValidatorTest() {
            _shortPacket = new List<byte>(_shortPacketHeader);
            _shortPacket.AddRange(_shortPacketMessage);
            _shortPacket.Add(_shortPacketChecksum);
            // Test normal validator.
            Assert.AreEqual(ValidationResult.Valid, _validator.ValidatePacket(_shortPacket, _options));
            // Test resend of packet.
            Assert.AreEqual(ValidationResult.Duplicate, _validator.ValidatePacket(_shortPacket, _options));
            _shortPacket[131] = 0x73;
            _validator.Reset();
            // Test incorrect checksum.
            Assert.AreEqual(ValidationResult.Invalid, _validator.ValidatePacket(_shortPacket, _options));
        }

        [Test]
        public void CRCPacketValidatorTest() {
            _shortPacket = new List<byte>(_shortPacketHeader);
            _shortPacket.AddRange(_shortPacketMessage);
            _shortPacket.AddRange(_shortPacketCRCChecksum);
            // Test normal validator.
            Assert.AreEqual(ValidationResult.Valid, _crcValidator.ValidatePacket(_shortPacket, _options));
            // Test resend of packet.
            Assert.AreEqual(ValidationResult.Duplicate, _crcValidator.ValidatePacket(_shortPacket, _options));
            _shortPacket[131] = 0x73;
            _crcValidator.Reset();
            // Test incorrect checksum.
            Assert.AreEqual(ValidationResult.Invalid, _crcValidator.ValidatePacket(_shortPacket, _options));
        }
    }
}