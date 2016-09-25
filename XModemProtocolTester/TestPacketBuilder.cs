using System.Collections.Generic;
using System.Linq;
using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Options;
using NUnit.Framework;

namespace XModemProtocolTester {
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
}