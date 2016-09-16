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
using XModemProtocol;
using XModemProtocol.CRC;
using NUnit.Framework;

namespace XModemProtocolTester
{
    partial class Program {
        static void Main(string[] args) {
        }
    }

    [TestFixture] 
    public class TestCRCLookupTable {
        List<int[]> _indicesToCheck = new List<int[]> {
            new int[] { 0, 1, 187, 254, 255, },
            new int[] { 0, 4129, 5808, 3793, 7920, },
        };

        [Test] 
        public void TestSetOfValuesOnCRCLookUpTable() {
            var table = new LookUpTable(0x1021);

            for(int i = 0; i < _indicesToCheck[0].Length; i++) {
                Assert.AreEqual(table.QueryTable(_indicesToCheck[0][i]), _indicesToCheck[1][i]); 
            }
        }
    }

    [TestFixture] 
    public class TestICRCChecksumCalculator {
        [Test]
        public void TestValueD9InICRCChecksumCalculator() {
            var crc = new CRC16LTE();
            Assert.True(Enumerable.SequenceEqual(crc.CalculateChecksum(new byte[] { 0xD9 }), new byte[] { 0x5A, 0x54 }));
        }
        [Test]
        public void TestValueA5InICRCChecksumCalculator() {
            var crc = new CRC16LTE();
            Assert.True(Enumerable.SequenceEqual(crc.CalculateChecksum(new byte[] { 0xA5 }), new byte[] { 0xE5, 0x4F }));
        }
        [Test]
        public void TestE5AD8BInICRCChecksumCalculator() {
            var crc = new CRC16LTE();
            Assert.True(Enumerable.SequenceEqual(crc.CalculateChecksum(new byte[] { 0xE5, 0xAD, 0x8B }), new byte[] { 0x00, 0x00 }));
        }
    }
}