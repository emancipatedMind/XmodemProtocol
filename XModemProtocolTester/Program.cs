using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Detectors;
using XModemProtocol.Options;
using XModemProtocol.Validators.Checksum;
using XModemProtocol.Validators.Packet;
using NUnit.Framework;

namespace XModemProtocolTester {
    partial class Program {
        static void Main(string[] args) {
        }
    }

    [TestFixture] 
    public class TestCancellationDetector {

        static XModemProtocolOptions _options = new XModemProtocolOptions();
        static List<byte> _message;
        static ICancellationDetector _detector = new CancellationDetector();

        [Test]
        public void TestDetector() {

            _options.CancellationBytesRequired = 10;

            _message = new List<byte>();
            _message.Add(0x43);

            _detector = new CancellationDetector();

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.AddRange(Enumerable.Repeat((byte) 0x43, 50));

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));
            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));
            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 9));

            Assert.IsFalse(_detector.CancellationDetected(_message, _options));

            _message.Add(0x43);
            _message.AddRange(Enumerable.Repeat(_options.CAN, 10));

            Assert.IsTrue(_detector.CancellationDetected(_message, _options));
        }

    }

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
        public void TestOneKPacketValidator() {
            _longPacket = new List<byte>(_longPacketHeader);
            _longPacket.AddRange(_longPacketMessage);
            _longPacket.AddRange(_longPacketCRCChecksum);
            // Test normal validator.
            Assert.IsTrue(_crcValidator.ValidatePacket(_longPacket, _options));
            // Test resend of packet.
            Assert.IsTrue(_crcValidator.ValidatePacket(_longPacket, _options));
            _longPacket[131] = 0x73;
            _crcValidator.Reset();
            // Test incorrect checksum.
            Assert.IsFalse(_crcValidator.ValidatePacket(_longPacket, _options));
        }

        [Test]
        public void TestNormalPacketValidator() {
            _shortPacket = new List<byte>(_shortPacketHeader);
            _shortPacket.AddRange(_shortPacketMessage);
            _shortPacket.Add(_shortPacketChecksum);
            // Test normal validator.
            Assert.IsTrue(_validator.ValidatePacket(_shortPacket, _options));
            // Test resend of packet.
            Assert.IsTrue(_validator.ValidatePacket(_shortPacket, _options));
            _shortPacket[131] = 0x73;
            _validator.Reset();
            // Test incorrect checksum.
            Assert.IsFalse(_validator.ValidatePacket(_shortPacket, _options));
        }

        [Test]
        public void TestCRCPacketValidator() {
            _shortPacket = new List<byte>(_shortPacketHeader);
            _shortPacket.AddRange(_shortPacketMessage);
            _shortPacket.AddRange(_shortPacketCRCChecksum);
            // Test normal validator.
            Assert.IsTrue(_crcValidator.ValidatePacket(_shortPacket, _options));
            // Test resend of packet.
            Assert.IsTrue(_crcValidator.ValidatePacket(_shortPacket, _options));
            _shortPacket[131] = 0x73;
            _crcValidator.Reset();
            // Test incorrect checksum.
            Assert.IsFalse(_crcValidator.ValidatePacket(_shortPacket, _options));
        }

    }

    [TestFixture]
    public class TestPacketBuilder {
        IXModemProtocolOptions _options = new XModemProtocolOptions();
        ISummationChecksumCalculator _calculator = new NormalChecksumCalculator();
        ICRCChecksumCalculator _CRCCalculator = new CRCChecksumCalculator(new LookUpTable(0x1021));
        IPacketBuilder _builder;
        IEnumerable<byte> shortMessage = Enumerable.Repeat((byte)0x34,127);
        IEnumerable<byte> longMessage = Enumerable.Repeat((byte)0x34,129);

        [Test]
        public void TestNormalPacketBuilder() {
            _builder = new NormalPacketBuilder(_calculator);

            var packets = _builder.GetPackets(shortMessage, _options);
            Assert.AreEqual(packets[0].Count, 132);
            Assert.AreEqual(packets.Count, 1);

            packets = _builder.GetPackets(longMessage, _options);
            Assert.AreEqual(packets[0].Count, 132);
            Assert.AreEqual(packets.Count, 2);
        }
        [Test] 
        public void TestCRCPacketBuilder() {
            _builder = new CRCPacketBuilder(_CRCCalculator);

            var packets = _builder.GetPackets(shortMessage, _options);
            Assert.AreEqual(packets[0].Count, 133);
            Assert.AreEqual(packets.Count, 1);

            packets = _builder.GetPackets(longMessage, _options);
            Assert.AreEqual(packets[0].Count, 133);
            Assert.AreEqual(packets.Count, 2);
        }
        [Test] 
        public void TestOneKPacketBuilder() {
            _builder = new OneKPacketBuilder(_CRCCalculator);

            var packets = _builder.GetPackets(shortMessage, _options);
            Assert.AreEqual(packets[0].Count, 133);
            Assert.AreEqual(packets.Count, 1);
            Assert.AreEqual(packets[0][0] , 0x01);

            packets = _builder.GetPackets(longMessage, _options);
            Assert.AreEqual(packets[0].Count, 1029);
            Assert.AreEqual(packets.Count, 1);
            Assert.AreEqual(packets[0][0] , 0x02);
        }

    }

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

    [TestFixture] 
    public class TestNormalValidator {
        
        byte[] _collection = new byte[] { 0x00, 0x19, 0x88, 0x33, 0x72, };

        [Test] 
        public void TestIChecksumValidator() {
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

    [TestFixture] 
    public class TestICRCLookupTable {
        List<int[]> _indicesToCheck = new List<int[]> {
            new int[] { 0, 1, 187, 254, 255, },
            new int[] { 0, 4129, 5808, 3793, 7920, },
        };

        [Test] 
        public void TestSetOfValuesOnCRCLookUpTable() {
            ICRCLookUpTable table = new LookUpTable(0x1021);

            for(int i = 0; i < _indicesToCheck[0].Length; i++) {
                Assert.AreEqual(table.QueryTable(_indicesToCheck[0][i]), _indicesToCheck[1][i]); 
            }
        }
    }

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