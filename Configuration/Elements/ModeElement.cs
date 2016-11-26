namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ModeElement : ConfigurationElement {
        private ModeElement() { }

        [ConfigurationProperty("value", DefaultValue = "OneK")]
        private string ConfigValue => (string) this["value"];

        public XModemMode Value {
            get {
                XModemMode value;
                if (System.Enum.TryParse(ConfigValue, true, out value)) {
                    return value;
                }
                return XModemMode.OneK;
            }
        }

        public override string ToString() => $"Mode : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ModeElement && GetHashCode() == obj.GetHashCode();
    }
}