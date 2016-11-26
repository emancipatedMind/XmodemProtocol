namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ReceiverTimeoutDuringPacketReceptionElement : ConfigurationElement  {
        private ReceiverTimeoutDuringPacketReceptionElement() { }

        [ConfigurationProperty("value", DefaultValue = 10000)]
        public int Value => (int) this["value"];

        public override string ToString() => $"ReceiverTimeoutDuringPacketReception : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ReceiverTimeoutDuringPacketReceptionElement && GetHashCode() == obj.GetHashCode();
    }
}