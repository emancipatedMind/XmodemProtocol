namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class CancellationBytesRequiredElement : ConfigurationElement  {
        private CancellationBytesRequiredElement() { }

        [ConfigurationProperty("value", DefaultValue = 5)]
        public int Value => (int) this["value"];

        public override string ToString() => $"CancellationBytesRequired : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is CancellationBytesRequiredElement && GetHashCode() == obj.GetHashCode();
    }
}