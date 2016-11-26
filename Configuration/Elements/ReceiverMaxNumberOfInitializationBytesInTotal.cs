namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ReceiverMaxNumberOfInitializationBytesInTotalElement : ConfigurationElement  {
        private ReceiverMaxNumberOfInitializationBytesInTotalElement() { }

        [ConfigurationProperty("value", DefaultValue = 10)]
        public int Value => (int) this["value"];

        public override string ToString() => $"ReceiverMaxNumberOfInitializationBytesInTotal : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ReceiverMaxNumberOfInitializationBytesInTotalElement && GetHashCode() == obj.GetHashCode();
    }
}