namespace XModemProtocolTester {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class RandomDataGenerator {

        static Random rand = new Random();
        public int Offset { get; set; } = 0x20;
        public int Domain { get; set; } = 0x5E;

        public IEnumerable<byte> GetRandomData(int length) =>
            Enumerable.Range(0, length).Select(x => (byte)(rand.Next(Domain) + Offset)).ToArray();
    }
}