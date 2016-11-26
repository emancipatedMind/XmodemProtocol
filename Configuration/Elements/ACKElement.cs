namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ACKElement : ConfigurationElement  {
        private ACKElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x06)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"ACK : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ACKElement && GetHashCode() == obj.GetHashCode();
    }
}