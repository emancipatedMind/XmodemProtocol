namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class SenderInitializationTimeoutElement : ConfigurationElement  {
        private SenderInitializationTimeoutElement() { }

        [ConfigurationProperty("value", DefaultValue = 10000)]
        public int Value => (int) this["value"];

        public override string ToString() => $"SenderInitializationTimeout : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is SenderInitializationTimeoutElement && GetHashCode() == obj.GetHashCode();
    }
}