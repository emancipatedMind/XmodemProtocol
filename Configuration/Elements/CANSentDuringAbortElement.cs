namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class CANSentDuringAbortElement : ConfigurationElement  {
        private CANSentDuringAbortElement() { }

        [ConfigurationProperty("value", DefaultValue = 5)]
        public int Value => (int) this["value"];

        public override string ToString() => $"CANSentDuringAbort : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is CANSentDuringAbortElement && GetHashCode() == obj.GetHashCode();
    }
}