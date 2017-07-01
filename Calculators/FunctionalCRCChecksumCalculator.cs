namespace XModemProtocol.Calculators {
    using Support;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class FunctionalCRCChecksumCalculator : ICRCChecksumCalculator {

        Func<int, byte, int> CalculationFunction;
        IEnumerable<byte> _initial = new byte[2];

        public FunctionalCRCChecksumCalculator(ICRCLookUpTable lookUpTable) =>
            CalculationFunction = GenerateCalculationFunction(lookUpTable);

        public IEnumerable<byte> InitialCRCValue {
            get => _initial;
            set => _initial = value?.Concat(Enumerable.Repeat((byte)0, 2)).Take(2) ?? new byte[2];
        }

        public IEnumerable<byte> CalculateChecksum(IEnumerable<byte> input) =>
            BitConverter.GetBytes(
                input
                    .Aggregate(
                        BitConverter.ToInt32(
                            InitialCRCValue
                            .Concat(Enumerable.Repeat((byte)0, 2))
                            .ToArray(),
                            0
                        ),
                        CalculationFunction
                    )
                )
            .Take(2)
            .Reverse();

        Func<int, byte, int> GenerateCalculationFunction(ICRCLookUpTable lookUpTable) =>
            (runningCheckSum, nextByte) =>
                (lookUpTable.QueryTable(((runningCheckSum >> 8) ^ nextByte))) ^ (runningCheckSum << 8).ApplyMask(0xFFFF);

    }
}