using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using XModemProtocol.Environment;
using XModemProtocol.Operations.Finalize;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestFinalizer {

        static RandomDataGenerator _rdg = new RandomDataGenerator();

        [Test] 
        public void FinalizeReceiveTest() {
            var data = _rdg.GetRandomData(20).ToList(); 
            var expectedData = new List<byte>(data);
            IContext context = new Context();
            data.AddRange(Enumerable.Repeat(context.Options.SUB, 15));
            context.Data = data;
            IFinalizer finalizer = new FinalizeReceive();
            // Test to see if finalizer strips away SUB.
            finalizer.Finalize(context);
            Assert.AreEqual(expectedData, context.Data);
            context.Data = new List<byte>();
            // Test to see if finalizer ends safely if no data present.
            finalizer.Finalize(context);
        }
        [Test] 
        public void FinalizeSendTest() {
            // No test necessary.
        }
    }
}