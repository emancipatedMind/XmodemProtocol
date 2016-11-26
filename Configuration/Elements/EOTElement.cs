namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class EOTElement : ConfigurationElement  {
        private EOTElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x04)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"EOT : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is EOTElement && GetHashCode() == obj.GetHashCode();
    }
}