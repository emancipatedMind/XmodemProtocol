namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class SOHElement : ConfigurationElement  {
        private SOHElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x01)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"SOH : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is SOHElement && GetHashCode() == obj.GetHashCode();
    }
}