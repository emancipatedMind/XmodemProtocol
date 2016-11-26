namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class SUBElement : ConfigurationElement  {
        private SUBElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x1A)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"SUB : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is SUBElement && GetHashCode() == obj.GetHashCode();
    }
}