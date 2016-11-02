using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XModemProtocol.Communication;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Operations;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestOperation {

        IOperation _operation;
        IContext _context = new Context();
        ICommunicator _com;
        RandomDataGenerator _rdg = new RandomDataGenerator {
            Domain = 0x5E,
            Offset = 0x20,
        };

        [Test]
        public void TestSendOperation() {
            _context.Mode = XModemProtocol.XModemMode.CRC;
            _context.Data = _rdg.GetRandomData(40000).ToList();
            List<List<byte>> dts = new List<List<byte>> {
                new List<byte> { _context.Options.C },
            };
            dts.AddRange(Enumerable.Repeat(new List<byte> { _context.Options.ACK }, (int)Math.Ceiling(_context.Data.Count / 128.0) + 1));
            _com = new ComSendCollection {
                CollectionToSend = dts,
            };
            _context.Communicator = _com;
            _operation = new SendOperation();
            _operation.Go(_context);
        }
    }
}