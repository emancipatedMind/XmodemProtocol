using System;
namespace XModemProtocol.Options {
    public interface IXModemProtocolOptions : ICloneable {
        byte ACK { get; }
        byte C { get; }
        byte CAN { get; }
        int CancellationBytesRequired { get; }
        int CANSentDuringAbort { get; }
        byte EOT { get; }
        XModemMode Mode { get; }
        byte NAK { get; }
        int ReceiverConsecutiveNAKsRequiredForCancellation { get; }
        int ReceiverInitializationTimeout { get; }
        int ReceiverMaxNumberOfInitializationBytesForCRC { get; }
        int ReceiverMaxNumberOfInitializationBytesInTotal { get; }
        int ReceiverTimeoutDuringPacketReception { get; }
        int SenderInitializationTimeout { get; }
        byte SOH { get; }
        byte STX { get; }
        byte SUB { get; }
    }
}