namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class NAKElement : ConfigurationElement  {
        private NAKElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x15)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"NAK : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is NAKElement && GetHashCode() == obj.GetHashCode();
    }
}