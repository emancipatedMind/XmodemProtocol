# XModem Protocol
## Implementation of the XMODEM Protocol compatible with .NET written in C#.
Supports XModem, XModem-1k & XModem-CRC.  
This library can be used to send or receive bytes across a serial line.

### Properties
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
 * Write Only
 * _XModemProtocol.Options.IXModemProtocolOptions_  
 Accepts an instance of a class that implements the _XModemProtocol.Options.IXModemProtocolOptions_ interface. This contains the bytes that _XModemProtocol.XModemCommunicator_ will use to facilitate transfer along with some other options to customize how _XModemProtocol.XModemCommunicator_ operates.

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
 Puts _XModemProtocol.XModemCommuniator_ in the sender role awaiting initialization byte from receiver.

* _**Receive**_  
 Puts _XModemProtocol.XModemCommuniator_ in the receiver role sending the initialization byte.

* _**CancelOperation**_  
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

### Author
Peter T. Owens-Finch  
powensfinch@gmail.com
