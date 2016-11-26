namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ReceiverInitializationTimeoutElement : ConfigurationElement  {
        private ReceiverInitializationTimeoutElement() { }

        [ConfigurationProperty("value", DefaultValue = 3000)]
        public int Value => (int) this["value"];

        public override string ToString() => $"ReceiverInitializationTimeout : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ReceiverInitializationTimeoutElement && GetHashCode() == obj.GetHashCode();
    }
}