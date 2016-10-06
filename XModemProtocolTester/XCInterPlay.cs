using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol;
using XModemProtocol.Communication;

namespace XModemProtocolTester
{
    public class XCInterPlay {
        RandomDataGenerator _rdg = new RandomDataGenerator {
            Domain = 0x5E,
            Offset = 0x20
        };
        InterPlayComm _receiverCom;
        InterPlayComm _senderCom;
        IEnumerable<byte> _data;
        XModemCommunicator _receiver;
        XModemCommunicator _sender;
        public void PlayCatch() {

            _senderCom = new InterPlayComm { Name = "Port 1" };
            _receiverCom = new InterPlayComm { Name = "Port 2" };

            _senderCom.Partner = _receiverCom;
            _receiverCom.Partner = _senderCom;

            _senderCom.WriteDebug = false; 
            _receiverCom.WriteDebug = false; 

            SerialPort Port1 = new SerialPort {
                BaudRate = 230400,
                ReadTimeout = 10000,
                WriteTimeout = 10000,
                DataBits = 8,
                Parity = Parity.Even,
                StopBits = StopBits.One,
                PortName = "COM14" 
            };
            SerialPort Port2 = new SerialPort {
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

            //_sender = new XModemCommunicator(Port1);
            //_receiver = new XModemCommunicator(Port2);
            
            _sender = new XModemCommunicator(_senderCom);
            _receiver = new XModemCommunicator(_receiverCom);

            _receiver.Options = new XModemProtocol.Options.XModemProtocolOptions {
                Mode = XModemMode.Checksum,
            };
            _sender.Options = new XModemProtocol.Options.XModemProtocolOptions {
            };
            _data = _rdg.GetRandomData(1000000);
            //_data = File.ReadAllBytes(@"C:\Users\ptowensf\Desktop\Workbooks\Lab Upgrade\Dongle\Hex Files\42EF_DP_v1.009_12F91EEC_20150210.hex");

            SetEvents(_sender, "Port 1");
            SetEvents(_receiver, "Port 2");

            _sender.PacketsBuilt += (s, e) => Console.WriteLine($"Port 1 : {e.Packets.Count}");

            _sender.Data = _data;
            Task[] tasks = new Task[] {
                Task.Run(() => _sender.Send()),
                Task.Run(() => _receiver.Receive()),
            };

            Task.WaitAll(tasks);
            Console.ReadLine();
            //Debug.WriteLine(_data.SequenceEqual(_receiver.Data)); 
            //Debug.WriteLine(Encoding.ASCII.GetString(_receiver.Data.ToArray()));
        }

        private void SetEvents(XModemCommunicator xmp, string name) {
            xmp.StateUpdated += (s, e) => {
                Console.WriteLine($"{name} : State        => {e.State}"); 
            };
            xmp.ModeUpdated += (s, e) => {
                Console.WriteLine($"{name} : Mode         => {e.Mode}"); 
            };
            xmp.Aborted += (s, e) => {
                Console.WriteLine($"{name} : Abort Reason => {e.Reason}"); 
            };
            xmp.PacketToSend += (s, e) => {
                if (e.PacketNumber % 100 == 0) {
                    Console.WriteLine(e.PacketNumber);
                };
            };
        }
    }
}