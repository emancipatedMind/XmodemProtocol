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
    public class XCInterPlay {

        public class XCInterPlayOptions : IPlayCatchOptions {
            public string PortOne { get; set; }
            public string PortTwo { get; set; }
            public IEnumerable<byte> Data { get; set; }
        }

        public void PlayCatch(IPlayCatchOptions settings) {

            Console.WriteLine("***** Test XModemProtocol DLL *****\n");

            var names = new List<string> {
                settings.PortOne,
                settings.PortTwo,
            };

            var ports = new List<SerialPort>();
            for (int i = 0; i < names.Count; i++)
            {
                ports.Add(
                    new SerialPort
                    {
                        BaudRate = 230400,
                        DataBits = 8,
                        Parity = Parity.Even,
                        PortName = names[i],
                        ReadTimeout = 10000,
                        StopBits = StopBits.One,
                        WriteTimeout = 10000,
                    }
                );
            }

        }
    }
}