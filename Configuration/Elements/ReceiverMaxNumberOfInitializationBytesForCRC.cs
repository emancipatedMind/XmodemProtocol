namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ReceiverMaxNumberOfInitializationBytesForCRCElement : ConfigurationElement  {
        private ReceiverMaxNumberOfInitializationBytesForCRCElement() { }

        [ConfigurationProperty("value", DefaultValue = 3)]
        public int Value => (int) this["value"];

        public override string ToString() => $"ReceiverMaxNumberOfInitializationBytesForCRC : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ReceiverMaxNumberOfInitializationBytesForCRCElement && GetHashCode() == obj.GetHashCode();
    }
}