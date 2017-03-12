namespace XModemProtocol.Configuration {
    using System.Configuration;
    using Elements;
    public class XModemProtocolConfigurationSection : ConfigurationSection {

        private static XModemProtocolConfigurationSection _instance;

        private XModemProtocolConfigurationSection() { }

        public static XModemProtocolConfigurationSection Settings { get; } =
            ConfigurationManager.GetSection("XModemProtocolConfiguration") as XModemProtocolConfigurationSection
            ?? (_instance ?? (_instance = new XModemProtocolConfigurationSection())); 

        [ConfigurationProperty("Polynomial")]
        public PolynomialElement Polynomial {
            get { return (PolynomialElement)this["Polynomial"]; }
            set { this["Polynomial"] = value; }
        }
        [ConfigurationProperty("Mode")]
        public ModeElement Mode {
            get { return (ModeElement)this["Mode"]; }
            set { this["Mode"] = value; }
        }
        [ConfigurationProperty("CANSentDuringAbort")]
        public CANSentDuringAbortElement CANSentDuringAbort {
            get { return (CANSentDuringAbortElement)this["CANSentDuringAbort"]; }
            set { this["CANSentDuringAbort"] = value; }
        }
        [ConfigurationProperty("CancellationBytesRequired")]
        public CancellationBytesRequiredElement CancellationBytesRequired {
            get { return (CancellationBytesRequiredElement)this["CancellationBytesRequired"]; }
            set { this["CancellationBytesRequired"] = value; }
        }
        [ConfigurationProperty("SenderOneKPacketSize")]
        public SenderOneKPacketSizeElement SenderOneKPacketSize {
            get { return (SenderOneKPacketSizeElement)this["SenderOneKPacketSize"]; }
            set { this["SenderOneKPacketSize"] = value; }
        }
        [ConfigurationProperty("SenderInitializationTimeout")]
        public SenderInitializationTimeoutElement SenderInitializationTimeout {
            get { return (SenderInitializationTimeoutElement)this["SenderInitializationTimeout"]; }
            set { this["SenderInitializationTimeout"] = value; }
        }
        [ConfigurationProperty("SOH")]
        public SOHElement SOH {
            get { return (SOHElement)this["SOH"]; }
            set { this["SOH"] = value; }
        }
        [ConfigurationProperty("STX")]
        public STXElement STX {
            get { return (STXElement)this["STX"]; }
            set { this["STX"] = value; }
        }
        [ConfigurationProperty("ACK")]
        public ACKElement ACK {
            get { return (ACKElement)this["ACK"]; }
            set { this["ACK"] = value; }
        }
        [ConfigurationProperty("NAK")]
        public NAKElement NAK {
            get { return (NAKElement)this["NAK"]; }
            set { this["NAK"] = value; }
        }
        [ConfigurationProperty("C")]
        public CElement C {
            get { return (CElement)this["C"]; }
            set { this["C"] = value; }
        }
        [ConfigurationProperty("EOT")]
        public EOTElement EOT {
            get { return (EOTElement)this["EOT"]; }
            set { this["EOT"] = value; }
        }
        [ConfigurationProperty("SUB")]
        public SUBElement SUB {
            get { return (SUBElement)this["SUB"]; }
            set { this["SUB"] = value; }
        }
        [ConfigurationProperty("CAN")]
        public CANElement CAN {
            get { return (CANElement)this["CAN"]; }
            set { this["CAN"] = value; }
        }
        [ConfigurationProperty("ReceiverConsecutiveNAKsRequiredForCancellation")]
        public ReceiverConsecutiveNAKsRequiredForCancellationElement ReceiverConsecutiveNAKsRequiredForCancellation {
            get { return (ReceiverConsecutiveNAKsRequiredForCancellationElement)this["ReceiverConsecutiveNAKsRequiredForCancellation"]; }
            set { this["ReceiverConsecutiveNAKsRequiredForCancellation"] = value; }
        }
        [ConfigurationProperty("ReceiverInitializationTimeout")]
        public ReceiverInitializationTimeoutElement ReceiverInitializationTimeout {
            get { return (ReceiverInitializationTimeoutElement)this["ReceiverInitializationTimeout"]; }
            set { this["ReceiverInitializationTimeout"] = value; }
        }
        [ConfigurationProperty("ReceiverTimeoutDuringPacketReception")]
        public ReceiverTimeoutDuringPacketReceptionElement ReceiverTimeoutDuringPacketReception {
            get { return (ReceiverTimeoutDuringPacketReceptionElement)this["ReceiverTimeoutDuringPacketReception"]; }
            set { this["ReceiverTimeoutDuringPacketReception"] = value; }
        }
        [ConfigurationProperty("ReceiverMaxNumberOfInitializationBytesForCRC")]
        public ReceiverMaxNumberOfInitializationBytesForCRCElement ReceiverMaxNumberOfInitializationBytesForCRC {
            get { return (ReceiverMaxNumberOfInitializationBytesForCRCElement)this["ReceiverMaxNumberOfInitializationBytesForCRC"]; }
            set { this["ReceiverMaxNumberOfInitializationBytesForCRC"] = value; }
        }
        [ConfigurationProperty("ReceiverMaxNumberOfInitializationBytesInTotal")]
        public ReceiverMaxNumberOfInitializationBytesInTotalElement ReceiverMaxNumberOfInitializationBytesInTotal {
            get { return (ReceiverMaxNumberOfInitializationBytesInTotalElement)this["ReceiverMaxNumberOfInitializationBytesInTotal"]; }
            set { this["ReceiverMaxNumberOfInitializationBytesInTotal"] = value; }
        }
    }
}