using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XModemProtocol.Operations;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestOperationcs {

        IOperation _operation;
        IRequirements _req;
        IContext _context = new Context();

        [Test] 
        public void TestSendOperation() {
            _operation = new SendOperation();
            _req = new Requirements {

            };
        }
    }
}
