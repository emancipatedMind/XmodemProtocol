namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class STXElement : ConfigurationElement  {
        private STXElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x02)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"STX : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is STXElement && GetHashCode() == obj.GetHashCode();
    }
}