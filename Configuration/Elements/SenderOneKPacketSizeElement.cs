namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    using Options;
    public class SenderOneKPacketSizeElement : ConfigurationElement {
        private SenderOneKPacketSizeElement() { }

        [ConfigurationProperty("value", DefaultValue = "OneKOnly")]
        private string ConfigValue => (string) this["value"];

        public OneKPacketSize Value {
            get {
                OneKPacketSize value;
                if (System.Enum.TryParse(ConfigValue, true, out value)) {
                    return value;
                }
                return OneKPacketSize.OneKOnly;
            }
        }

        public override string ToString() => $"SenderOneKPacketSize : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is SenderOneKPacketSizeElement && GetHashCode() == obj.GetHashCode();
    }
}