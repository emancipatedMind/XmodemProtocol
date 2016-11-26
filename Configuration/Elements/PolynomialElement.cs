namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class PolynomialElement : ConfigurationElement  {
        private PolynomialElement() { }

        [ConfigurationProperty("value", DefaultValue = 0x1021)]
        public int Value => (int) this["value"];

        public override string ToString() => $"Polynomial : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is PolynomialElement && GetHashCode() == obj.GetHashCode();
    }
}