namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class CANElement : ConfigurationElement  {
        private CANElement() { }

        [ConfigurationProperty("value", DefaultValue = (byte) 0x18)]
        public byte Value => (byte) this["value"];

        public override string ToString() => $"CAN : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is CANElement && GetHashCode() == obj.GetHashCode();
    }
}