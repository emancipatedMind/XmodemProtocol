namespace XModemProtocol.Configuration.Elements {
    using System.Configuration;
    public class ReceiverConsecutiveNAKsRequiredForCancellationElement : ConfigurationElement  {
        private ReceiverConsecutiveNAKsRequiredForCancellationElement() { }

        [ConfigurationProperty("value", DefaultValue = 5)]
        public int Value => (int) this["value"];

        public override string ToString() => $"ReceiverConsecutiveNAKsRequiredForCancellation : {Value};";
        public override int GetHashCode() => ToString().GetHashCode();
        public override bool Equals(object obj) =>
            obj is ReceiverConsecutiveNAKsRequiredForCancellationElement && GetHashCode() == obj.GetHashCode();
    }
}