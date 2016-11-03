using System;
using System.Collections.Generic;

namespace XModemProtocolTester {
    public class RandomDataGenerator {
        Random rand = new Random();
        List<byte> data;
        public int Offset { get; set; }
        public int Domain { get; set; }

        public IEnumerable<byte> GetRandomData(int length) {
            data = new List<byte>();
            for (int i = 0; i < length; i++) 
                data.Add((byte)(rand.Next(Domain) + Offset));
            return data;
        }
    }
}