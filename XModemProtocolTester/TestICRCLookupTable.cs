using NUnit.Framework;
using System.Collections.Generic;
using XModemProtocol.Calculators;

namespace XModemProtocolTester
{
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

}
