using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using XModemProtocol.Factories;
using XModemProtocol.Operations.Initialize;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    partial class Program {
        static void Main(string[] args) {
            var tester = new TestInvoke();
            tester.TestInvokeReceive();
        }
    }
}