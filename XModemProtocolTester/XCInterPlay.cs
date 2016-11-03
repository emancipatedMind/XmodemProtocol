//#define External

using System.Collections.Generic;
using System.Threading.Tasks;

namespace XModemProtocolTester {
    using static System.Console;
    public class XCInterPlay {
        RandomDataGenerator _rdg = new RandomDataGenerator {
            Domain = 0x5E,
            Offset = 0x20
        };
        IEnumerable<byte> _data;
        public void PlayCatch() {

            var _sender = new XModemProtocol.XModemCommunicator();
            var _receiver = new XModemProtocol.XModemCommunicator();

#if External
            var Port1 = new SerialPort {
                BaudRate = 230400,
                ReadTimeout = 10000,
                WriteTimeout = 10000,
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                PortName = "COM14" 
            };
            var Port2 = new SerialPort {
                BaudRate = 230400,
                ReadTimeout = 10000,
                WriteTimeout = 10000,
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                PortName = "COM8" 
            };
            Port1.Open();
            Port2.Open();
            _sender.Port = Port1;
            _receiver.Port = Port2;
#else
            InterPlayComm _senderCom = new InterPlayComm { Name = "Port 1" };
            InterPlayComm _receiverCom = new InterPlayComm { Name = "Port 2" };

            _senderCom.Partner = _receiverCom;
            _receiverCom.Partner = _senderCom;

            _sender.Communicator = _senderCom;
            _receiver.Communicator = _receiverCom;
#endif

            _data = _rdg.GetRandomData(10000);
            _receiver.Polynomial = 0x1021;
            _sender.Polynomial = 0x1021;

            SetEvents(_sender, "Port 1");
            SetEvents(_receiver, "Port 2");

            _sender.PacketsBuilt += (s, e) => WriteLine($"Port 1 : {e.Packets.Count}");

            _sender.Data = _data;

            var tasks = new Task[] {
                Task.Run(() => _sender.Send()),
                Task.Run(() => _receiver.Receive()),
            };

            Task.WaitAll(tasks);
            ReadLine();
        }

        private void SetEvents(XModemProtocol.XModemCommunicator xmp, string name) {
            xmp.StateUpdated += (s, e) => WriteLine($"{name} : State        => {e.State}"); 
            xmp.ModeUpdated += (s, e) => WriteLine($"{name} : Mode         => {e.Mode}"); 
            xmp.Aborted += (s, e) => WriteLine($"{name} : Abort Reason => {e.Reason}"); 
            xmp.PacketToSend += (s, e) => { if (e.PacketNumber % 100 == 0) WriteLine(e.PacketNumber); };
        }
    }
}