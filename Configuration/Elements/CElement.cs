namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class CElement : ConfigurationElement  {
        private CElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x43)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"C : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is CElement && GetHashCode() == obj.GetHashCode();
    }
}