using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XModemProtocol;

namespace XModemProtocolTester
{
    class XCInterPlay {
        static void PlayCatch() {
            Console.WriteLine("***** Test XModemProtocol DLL *****\n");

            var desktop = new DirectoryInfo(@"C:\Users\ptowensf\Desktop");

            string directory = desktop + @"\Workbooks\Lab Upgrade\Dongle\Hex Files\";
            string TBRFile = directory + "42EF_TB-RECEIVE_V0.8_FCA20D29_11032015.hex";
            string PRetroDiffuseFile = directory + "42EF_DP_v1.009_12F91EEC_20150210.hex";

            var names = new List<string> {
                "COM14",
                "COM8",
            };

            var ports = new List<SerialPort>();

            for (int i = 0; i < names.Count; i++)
                ports.Add(
                    new SerialPort {
                        BaudRate = 230400,
                        DataBits = 8,
                        Parity = Parity.Even,
                        PortName = names[i],
                        ReadTimeout = 10000,
                        StopBits = StopBits.One,
                        WriteTimeout = 10000,
                    }
                );

            var receiver = new XModemCommunicator(ports[0]);
            var sender = new XModemCommunicator(ports[1]) {
                //Mode = XModemMode.CRC,
                //Data = System.Text.Encoding.UTF8.GetBytes("Hello, how are you?"),
                Data = File.ReadAllBytes(PRetroDiffuseFile),
            };

            var communicators = new XModemCommunicator[] {
                sender,
                receiver,
            };

            // sender.BuildPackets();

            Action<XModemCommunicator, int> setup = (c, i) => {
                c.PacketsBuilt += (s, e) => Debug.WriteLine($"Instance {i} : Count of packets built : {e.Packets.Count}.");
                c.PacketReceived += (s, e) => {
                    Debug.WriteLineIf(e.PacketNumber % 100 == 0, $"Instance {i} : Packet Received : {e.PacketNumber} which {(e.PacketVerified ? "was verified" : "failed verification")}.");
                };
                //c.PacketToSend += (s, e) => {
                //    Debug.WriteLineIf(e.PacketNumber % 100 == 0, $"Instance {i} : Packet to be sent : {e.PacketNumber}.");
                //};
                //c.Aborted += (s, e) => Debug.WriteLine($"Instance {i} : {e.Reason}");
                //c.StateUpdated += (s, e) => Debug.WriteLine($"Instance {i} : " +  e.State);
                //c.ModeUpdated += (s, e) => Debug.WriteLine($"Instance {i} : " +  e.Mode);
                //c.RoleUpdated += (s, e) => Debug.WriteLine($"Instance {i} : " +  e.Role);
                //c.OperationPending += () => Debug.WriteLine($"Instance {i} operation pending.");
            };

            for (int i = 0; i < communicators.Length; i++)
                setup(communicators[i], i + 1);

            //receiver.Completed += (s, e) => {
            //    Console.WriteLine("Receiver done!");
            //    File.WriteAllBytes(desktop + @"\file.hex", e.Data.ToArray());
            //};
            //sender.Completed += (s, e) => Console.WriteLine("Sender done!") ;

            var sw = new Stopwatch();
            var tsw = new Stopwatch();

            //var modes = (XModemMode[]) typeof(XModemMode).GetEnumValues();
            var modes = Enumerable.Repeat(XModemMode.CRC, 4).ToArray();
            var options = new XModemProtocolOptions {
                ReceiverMaxNumberOfInitializationBytesInTotal = 5000,
            };

            //var modes = new XModemMode[] {
            //    XModemMode.CRC,
            //    XModemMode.OneK,
            //    XModemMode.CRC,
            //    XModemMode.Checksum,
            //};

            var sb = new StringBuilder();

            tsw.Start();
            for (int i = 0; i < modes.Length; i++) {
                options.Mode = modes[i];
                sw.Restart();
                Task.Factory.StartNew(
                    () => {
                        Task.Factory.StartNew(() => receiver.Receive(options), TaskCreationOptions.AttachedToParent);
                        Task.Factory.StartNew(() => sender.Send(options), TaskCreationOptions.AttachedToParent);
                    },
                    TaskCreationOptions.None
                ).Wait();
                sw.Stop();
                Console.WriteLine($"Mode : {modes[i]}");
                Console.WriteLine(sw.Elapsed);
                Console.WriteLine(sw.ElapsedMilliseconds);
                Console.WriteLine(sw.ElapsedTicks);
                Console.WriteLine();
                // Quick swap.
                var temp = sender;
                sender = receiver;
                receiver = temp;
                sb.AppendLine($"{sw.ElapsedMilliseconds}");
            }
            tsw.Stop();
            Console.WriteLine("***** Total *****");
            Console.WriteLine(tsw.Elapsed);
            Console.WriteLine(tsw.ElapsedMilliseconds);
            Console.WriteLine(tsw.ElapsedTicks);
            Console.ReadLine();
            sb.AppendLine($"=AVERAGE(A1:A{modes.Length})");
            File.WriteAllText(desktop + @"\times.csv" ,sb.ToString());

        }
    }
}
