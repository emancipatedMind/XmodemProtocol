# XModem Protocol
## Implementation of the XMODEM Protocol compatible with .NET written in C#.
Supports XModem, XModem-1k & XModem-CRC.  
This library can be used to send or receive bytes across a serial line.

### Properties
* _**Polynomial**_
 * Read/Write
 * _int_  
 Polynomial used by CRC lookup table. Defaulted to the polynomial X<sup>16</sup> + X<sup>12</sup> + X<sup>5</sup> + 1 (0x1021).
 
* _**Data**_
 * Read/Write
 * _System.Collections.Generic.IEnumerable&lt;byte&gt;_  
 Data received from transfer or data to be sent.  
 
* _**Communicator**_
 * Write Only
 * _XModemProtocol.Communication.ICommunicator_  
 Accepts an instance of a class that implements the _XModemProtocol.Communication.ICommunicator_ interface. Object will be used to facilitate the transfer of bytes.

* _**Port**_
 * Write Only
 * _System.IO.Ports.SerialPort_  
 _SerialPort_ to be used to create an instance of the _XModemProtocol.Communication.Communicator_ class.

* _**Options**_
 * Read/Write
 * _XModemProtocol.Options.IXModemProtocolOptions_  
 Accepts an instance of a class that implements the _XModemProtocol.Options.IXModemProtocolOptions_ interface. This contains the bytes that _XModemProtocol.XModemCommunicator_ will use to facilitate transfer along with some other options to customize how _XModemProtocol.XModemCommunicator_ operates. By default, is instance of _XModemProtocol.Options.XModemProtocolOptions_.

* _**State**_
 * Read Only
 * _XModemProtocol.XModemStates_  
 Returns the current state of _XModemProtocol.XModemCommunicator_.
 
* _**Mode**_
 * Read/Write
 * _XModemProtocol.XModemMode_  
 Mode to be used by _XModemProtocol.XModemCommunicator_. If using _Receive_ operation, CRC will upgrade to OneK automatically.

### Methods
* _**Send**_
 * _public void Send()_  
 Puts _XModemProtocol.XModemCommuniator_ in the sender role awaiting initialization byte from receiver.

* _**Receive**_
 * _public void Receive()_  
 Puts _XModemProtocol.XModemCommunicator_ in the receiver role sending the initialization byte.

* _**CancelOperation**_
 * _public void CancelOperation()_  
 Cancels operation currently running. No effect if no operation running.

### Events supported
* _**ModeUpdated**_  
 Fires when the mode of _XModemProtocol.XModemCommunicator_ is updated.

* _**StateUpdated**_  
 Fires when the state of _XModemProtocol.XModemCommunicator_ is updated.

* _**PacketsBuilt**_  
 Fires asynchronously whenever _XModemProtocol.XModemCommunicator_ finishes building packets.

* _**PacketToSend**_
 Fires when _XModemProtocol.XModemCommunicator_ is ready to send a packet. A blocking method will prevent packet from being sent. Does not fire when sending _IXModemProtocolOptions.EOT_.

* _**PacketReceived**_  
 Fires after a successful packet has been received by _XModemProtocol.XModemCommunicator_. This event must complete before _XModemProtocol.XModemCommunicator_ will send _IXModemProtocolOptions.ACK_. Does not fire when _IXModemProtocolOptions.EOT_ is received.

* _**Aborted**_  
 Fires if the operation is aborted. _XModemProtocol.XModemCommunicator_ will not return to being idle until event completes.

* _**Completed**_  
 Fires when the operation completes successfully. _XModemProtocol.XModemCommunicator_ will not return to being idle until event completes.

* _**OperationPending**_  
 Fires before the operation begins, and determines whether operation will run or not. _**Will not fire if Data contains no bytes, and performing _Send_ operation.**_
 
### Configuration

XModemProtocol.XModemCommunicator.Mode, XModemProtocol.XModemCommunicator.Polynomial, and all members of XModemProtocol.Options.IXModemProtocolOptions may now be set in the configuration file.

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <configSections>
        <section name="XModemProtocolConfiguration" type="XModemProtocol.Configuration.XModemProtocolConfigurationSection, XModemProtocol" />
      </configSections>
      <XModemProtocolConfiguration>
        <Polynomial value="4129"/>
        <Mode value="OneK"/>
        <CANSentDuringAbort value="5"/>
        <CancellationBytesRequired value="5"/>
        <SenderOneKPacketSize value="OneKOnly"/>
        <SenderInitializationTimeout value="10000"/>
        <SOH value="1"/>
        <STX value="2"/>
        <ACK value="6"/>
        <NAK value="21"/>
        <C value="67"/>
        <EOT value="4"/>
        <SUB value="26"/>
        <CAN value="24"/>
        <ReceiverConsecutiveNAKsRequiredForCancellation value="5"/>
        <ReceiverInitializationTimeout value="3000"/>
        <ReceiverTimeoutDuringPacketReception value="10000"/>
        <ReceiverMaxNumberOfInitializationBytesForCRC value="3"/>
        <ReceiverMaxNumberOfInitializationBytesInTotal value="10"/>
      </XModemProtocolConfiguration>
    </configuration>

 
### Simple Send Example

    using XModemProtocol;
    using System;
    using System.IO.Ports;  
    using System.IO;  

    namespace XModemProtocolExample {

      class Program {
    
        static void Main(string[] args) {
    
          Console.WriteLine("XModemProtocol Send Example\n");
      
          // Set up Port.
          var port = new SerialPort{
            BaudRate = 230400,
            DataBits = 8,
            Parity = Parity.Even,
            StopBits = StopBits.One,
            PortName = "COM5",
          };
      
          // Instantiate XModemCommunicator.
          var xmodem = new XModemCommunicator();
      
          // Attach port.
          xmodem.Port = port;
      
          // Pass in Data.
          xmodem.Data = File.ReadAllBytes(@"C:\filetosend.hex");
      
          // Subscribe to events.
          xmodem.Completed += (s,e) => {
            Console.WriteLine($"Operation completed.\nPress enter to exit.");
          };
          xmodem.Aborted += (s,e) => {
            Console.WriteLine("Operation Aborted.\nPress enter to exit.");
          };
          
          // Open port for use.
          port.Open();

          Console.WriteLine("Awaiting Receiver. Press enter to cancel.");
          // Send Data.
          xmodem.Send();
      
          // Await user.
          Console.ReadLine();
      
          if (xmodem.State != XModemStates.Idle) {
            xmodem.CancelOperation();
            Console.ReadLine();
          }
          
          // Dispose of port.
          port.Close();
        }
      }
    }

### Simple Receive Example

    using XModemProtocol;
    using System;
    using System.IO.Ports;
    using System.IO;
    using System.Linq;

    namespace XModemProtocolExample {

      class Program {
    
        static void Main(string[] args) {
      
          Console.WriteLine("XModemProtocol Receive Example\n");
      
          // Set up Port.
          var port = new SerialPort{
            BaudRate = 230400,
            DataBits = 8,
            Parity = Parity.Even,
            StopBits = StopBits.One,
            PortName = "COM5",
          };
      
          // Instantiate XModemCommunicator.
          var xmodem = new XModemCommunicator();
      
          // Attach port.
          xmodem.Port = port;
      
          // Subscribe to events.
          xmodem.Completed += (s,e) => {
            string message = "";
            try {
              File.WriteAllBytes(@"C:\fileReceived.hex", e.Data.ToArray());
              message = "Operation completed. Bytes written to file.";
            }
            catch {
              message = "Problem writing to file.";
            }
            Console.WriteLine($"{message}\nPress enter to exit.");
          };
          xmodem.Aborted += (s,e) => {
            Console.WriteLine("Operation Aborted.\nPress enter to exit.");
          };
          
          // Open port for use.
          port.Open();
      
          // Receive Data.
          Console.WriteLine("Receive Operation beginning. Press enter to cancel.");
          xmodem.Receive();
      
          // Await user.
          Console.ReadLine();
      
          if (xmodem.State != XModemStates.Idle) {
            xmodem.CancelOperation();
            Console.ReadLine();
          }
          
          // Dispose of port.
          port.Close();
        }
      }
    }

### Author
Peter T. Owens-Finch  
powensfinch@gmail.com
