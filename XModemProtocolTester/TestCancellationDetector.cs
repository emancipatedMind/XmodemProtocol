namespace XModemProtocolTester {
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using XModemProtocol.Detectors;
    using XModemProtocol.Options;
    [TestFixture]
    public class Cancellation_detector_returns_false {

        XModemProtocolOptions _options = new XModemProtocolOptions {
            CancellationBytesRequired = 10
        };

        List<ICancellationDetector> detectors = new List<ICancellationDetector> {
            new CancellationDetector(),
        };
        RandomDataGenerator _rdg = new RandomDataGenerator();

        [Test]
        public void when_message_is_too_short() {
            Enumerable.Range(0, 10)
                .ToList()
                .ForEach(n =>
                    detectors.ForEach(d =>
                        Assert.IsFalse(d.CancellationDetected(Enumerable.Repeat(_options.C, n), _options))
                    )
                );
        }

        [Test]
        public void when_message_is_long_enough_but_contains_no_cancellation_bytes() {
            Enumerable.Range(0, 10)
                .ToList()
                .ForEach(n =>
                    detectors.ForEach(d =>
                        Assert.IsFalse(d.CancellationDetected(_rdg.GetRandomData(50 * n) , _options))
                    )
                );
        }

        [Test]
        public void when_message_does_not_contain_enough_cancellation_bytes() {

            Func<int, IEnumerable<byte>> RandomDataWithCancellationBytesEmbedded = n =>
                _rdg.GetRandomData(50)
                .Concat(Enumerable.Repeat(_options.CAN, n))
                .Concat(_rdg.GetRandomData(50));

            Enumerable.Range(0, 10)
                .ToList()
                .ForEach(n =>
                    detectors.ForEach(d =>
                        Assert.IsFalse(d.CancellationDetected(RandomDataWithCancellationBytesEmbedded(n), _options))
                    )
                );

        }

        [Test]
        public void when_message_does_not_contain_enough_cancellation_bytes_contiguously_but_does_overall() {

            Func<int, IEnumerable<byte>> RandomDataWithCancellationBytesEmbedded = n =>
                Enumerable.Range(0, 5)
                .SelectMany(x =>
                    _rdg.GetRandomData(50)
                    .Concat(Enumerable.Repeat(_options.CAN, n))
                    );

            Enumerable.Range(0, 10)
                .ToList()
                .ForEach(n =>
                    detectors.ForEach(d =>
                        Assert.IsFalse(d.CancellationDetected(RandomDataWithCancellationBytesEmbedded(n), _options))
                    )
                );

        }

    }
    [TestFixture]
    public class Cancellation_detector_returns_true {

        XModemProtocolOptions _options = new XModemProtocolOptions {
            CancellationBytesRequired = 10
        };

        List<ICancellationDetector> detectors = new List<ICancellationDetector> {
            new CancellationDetector(),
        };
        RandomDataGenerator _rdg = new RandomDataGenerator();

        [Test]
        public void when_message_contains_only_enough_cancellation_bytes_contiguously() {
            detectors.ForEach(d =>
                Assert.IsTrue(d.CancellationDetected(Enumerable.Repeat(_options.CAN, _options.CancellationBytesRequired), _options))
            );
        }

        [Test]
        public void when_message_contains_enough_cancellation_bytes_contiguously_embedded_in_data() {
            detectors.ForEach(d =>
                Assert.IsTrue(
                    d.CancellationDetected(
                        _rdg.GetRandomData(50)
                        .Concat(Enumerable.Repeat(_options.CAN, _options.CancellationBytesRequired)
                        .Concat(_rdg.GetRandomData(50))
                    ),
                    _options)
                )
            );
        }
    }
}