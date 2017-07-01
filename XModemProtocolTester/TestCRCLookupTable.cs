using NUnit.Framework;
using System.Collections.Generic;
using XModemProtocol.Calculators;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestCRCLookupTable {
        [Test] 
        public void LookupTableTest() {

            int polynomial = 0x1021;

            List<int[]> indicesToCheck = new List<int[]> {
                new int[] { 0, 1, 187, 254, 255, },
                new int[] { 0, 4129, 5808, 3793, 7920, },
            };

            ICRCLookUpTable[] tables = new ICRCLookUpTable[] {
                new LookUpTable(polynomial),
            };

            foreach (var t in tables) 
                for(int i = 0; i < indicesToCheck[0].Length; i++) {
                    // Test Lookup table by testing random indices to be sure value is correct.
                    Assert.AreEqual(t.QueryTable(indicesToCheck[0][i]), indicesToCheck[1][i]); 
                }
        }
    }
}