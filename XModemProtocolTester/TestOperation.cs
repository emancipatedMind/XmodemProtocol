using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using XModemProtocol.Communication;
using XModemProtocol.Factories;
using XModemProtocol.Factories.Tools;
using XModemProtocol.Operations;
using XModemProtocol.Options;

namespace XModemProtocolTester {
    [TestFixture] 
    public class TestOperation {

        IToolFactory _toolFactory = new XModemToolFactory();
        IXModemTools _tools;
        IOperation _operation;
        IRequirements _req;
        IContext _context = new Context();
        IXModemProtocolOptions _options = new XModemProtocolOptions();
        ICommunicator _com;
        RandomDataGenerator _rdg = new RandomDataGenerator {
            Domain = 0x5E,
            Offset = 0x20,
        };

        [Test]
        public void TestSendOperation() {
            //_context.Mode = XModemProtocol.XModemMode.CRC;
            _context.Data = _rdg.GetRandomData(40000).ToList();
            _tools = _toolFactory.GetToolsFor(_context.Mode);
            _context.Packets = _tools.Builder.GetPackets(_context.Data, _options);
            List<List<byte>> dts = new List<List<byte>> {
                new List<byte> { _options.C },
            };
            dts.AddRange(Enumerable.Repeat(new List<byte> { _options.ACK }, (int)Math.Ceiling(_context.Data.Count / 128.0) + 1));
            _com = new ComSendCollection {
                CollectionToSend = dts,
            };
            _req = new Requirements {
                Context = _context,
                Options = _options,
                Communicator = _com,
            };
            _operation = new SendOperation();
            _operation.Go(_req);
        }
    }
}