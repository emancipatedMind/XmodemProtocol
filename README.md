# XModem Protocol
## Implementation of the XMODEM Protocol compatible with .NET written in C#.

Supports XModem, XModem-1k & XModem-CRC.

Can be used to send or receive bytes across a serial line.

##Events supported

* _ModeUpdated_

 Fires when the mode of the instance is updated.
* _StateUpdated_ 

 Fires when the state of the instance is updated.
* _PacketsBuilt_ 

 Fires when the instance builds packets.
* _PacketToSend_ 

 Fires when the instance is ready to send a packet.
* _PacketReceived_ 

 Fires when a packet is received.
* _Aborted_

 Fires if the operation is aborted
* _Completed_ 

 Fires when the operation completes successfully.
* _OperationPending_ 

 Fires before the operation begins. The return value is a bool which determines if operation will run or not.

## Author
Peter T. Owens-Finch
powensfinch@gmail.com
