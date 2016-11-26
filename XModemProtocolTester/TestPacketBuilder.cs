using System;
using XModemProtocol.Builders;
using XModemProtocol.Calculators;
using XModemProtocol.Options;
using NUnit.Framework;

namespace XModemProtocolTester {
    [TestFixture]
    public class TestPacketBuilder {

        static RandomDataGenerator _rdg = new RandomDataGenerator();

        [Test]
        public void NormalPacketBuilderTest() {
            IPacketBuilder _builder = new NormalPacketBuilder(new NormalChecksumCalculator());
            IXModemProtocolOptions _options = new XModemProtocolOptions();
            RunTest(Tuple.Create(_builder, _options, 127)); 
            RunTest(Tuple.Create(_builder, _options, 129)); 
        }

        [Test] 
        public void CRCPacketBuilderTest() {
            var table = new LookUpTable(0x1021);
            var calculator = new CRCChecksumCalculator(table);
            IPacketBuilder _builder = new CRCPacketBuilder(calculator);
            IXModemProtocolOptions _options = new XModemProtocolOptions();
            RunTest(Tuple.Create(_builder, _options, 127)); 
            RunTest(Tuple.Create(_builder, _options, 129)); 
        }

        [Test] 
        public void OneKPacketBuilderTest() {
            var table = new LookUpTable(0x1021);
            var calculator = new CRCChecksumCalculator(table);
            IPacketBuilder _builder = new OneKPacketBuilder(calculator);
            IXModemProtocolOptions _options = new XModemProtocolOptions();
            RunTest(Tuple.Create(_builder, _options, 127)); 
            RunTest(Tuple.Create(_builder, _options, 129)); 
            _options = new XModemProtocolOptions {
                SenderOneKPacketSize = OneKPacketSize.Mixed
            };
            RunTest(Tuple.Create(_builder, _options, 127)); 
            RunTest(Tuple.Create(_builder, _options, 129)); 
        }

        void RunTest(Tuple<IPacketBuilder,IXModemProtocolOptions, int> options) {
            var builder = options.Item1;
            var testOptions = options.Item2;
            int dataLength = options.Item3;
            int packetSize;
            int payloadSize = 128;
            byte header = testOptions.SOH;
            if (builder is NormalPacketBuilder) {
                packetSize = 132;
            }
            else if (builder is CRCPacketBuilder) {
                packetSize = 133;
            }
            else {
                if (testOptions.SenderOneKPacketSize == OneKPacketSize.Mixed
                    && dataLength < 129) {
                        packetSize = 133;
                }
                else {
                    packetSize = 1029;
                    payloadSize = 1024;
                    header = testOptions.STX;
                }
            }
            var packets = builder.GetPackets(_rdg.GetRandomData(dataLength), testOptions);
            int packetCount = (int) Math.Ceiling((double) dataLength/payloadSize);
            Assert.AreEqual(packetCount, packets.Count);
            Assert.AreEqual(packetSize, packets[0].Count);
            Assert.AreEqual(header, packets[0][0]);
        }
    }
}