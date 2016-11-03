using System;
using System.Collections.Generic;

namespace XModemProtocolTester {
    public class RandomDataGenerator {
        static Random rand = new Random();
        public int Offset { get; set; } = 0x20;
        public int Domain { get; set; } = 0x5E;

        public IEnumerable<byte> GetRandomData(int length) {
            var data = new List<byte>();
            for (int i = 0; i < length; i++) 
                data.Add((byte)(rand.Next(Domain) + Offset));
            return data;
        }
    }
}