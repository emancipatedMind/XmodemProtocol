# XModem Protocol
## Implementation of the XMODEM Protocol compatible with .NET written in C#.

Supports XModem, XModem-1k & XModem-CRC.

Can be used to send or receive bytes across a serial line.

### Properties
* _**Data**_

 Data Property.
* _**Communicator**_

 Write only property that accepts a class that implements the _ICommunicator_ interface. This class will be used to facilitate the transfer of bytes.
* _**Port**_

 Write only property that accepts a _SerialPort_ class. This will be passed to a generic class that implements the _ICommunicator_ interface.
* _**Options**_

 Write only property that accepts a class that implements the _IXModemProtocolOptions_ interface. This class contains the bytes that _XModemCommunicator_ will use to facilitate along with some other options to customize how _XModemCommunicator_ operates.
 
### Methods
* _**Send**_

 Send Operation.
* _**Receive**_

 Receive Operation.
 
* _**CancelOperation**_

 Cancel Operation.
 
### Events supported
* _**ModeUpdated**_

 Fires when the mode of the instance is updated.
* _**StateUpdated**_ 

 Fires when the state of the instance is updated.
* _**PacketsBuilt**_ 

 Fires when the instance builds packets.
* _**PacketToSend**_ 

 Fires when the instance is ready to send a packet. A blocking method will prevent packet from being sent.
* _**PacketReceived**_ 

 Fires when a packet is received.
* _**Aborted**_

 Fires if the operation is aborted. Instance will not return to being idle until event completes.
* _**Completed**_ 

 Fires when the operation completes successfully. Instance will not return to being idle until event completes.
* _**OperationPending**_ 

 Fires before the operation begins. The return value is a bool which determines if operation will run or not. _**Will not fire if Data contains no bytes, and performing send operation.**_

### Author
Peter T. Owens-Finch
powensfinch@gmail.com
