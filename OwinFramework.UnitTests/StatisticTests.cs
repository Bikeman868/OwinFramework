using System;
using NUnit.Framework;
using OwinFramework.MiddlewareHelpers.Analysable;

namespace UnitTests
{
    [TestFixture]
    public class StatisticTests
    {
        [Test]
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(10, "10")]
        [TestCase(5367, "5.4K")]
        [TestCase(3456875, "3.5M")]
        public void Should_format_int_statistics(int value, string expected)
        {
            var statistic = new IntStatistic(() => value);
            statistic.Refresh();
            Assert.AreEqual(expected, statistic.Formatted);
        }

        [Test]
        [TestCase(0, "0")]
        [TestCase(1, "1")]
        [TestCase(10, "10")]
        [TestCase(5367, "5.4K")]
        [TestCase(3456875, "3.5M")]
        [TestCase(0.1f, "0.1")]
        [TestCase(0.0043f, "4.3m")]
        [TestCase(0.00012f, "0.1m")]
        [TestCase(0.0000012f, "1.2u")]
        public void Should_format_float_statistics(float value, string expected)
        {
            var statistic = new FloatStatistic(() => value);
            statistic.Refresh();
            Assert.AreEqual(expected, statistic.Formatted);
        }

        [Test]
        [TestCase(0, "0us")]
        [TestCase(1.05, "~1s")]
        [TestCase(10, "10s")]
        [TestCase(5367, "89 minutes")]
        [TestCase(3456875, "40 days")]
        [TestCase(0.1f, "99ms")]
        [TestCase(0.0043f, "430us")]
        [TestCase(0.00012f, "11.9us")]
        [TestCase(0.000012f, "1.2us")]
        public void Should_format_timespan_statistics(double seconds, string expected)
        {
            var statistic = new TimeSpanStatistic(() =>
            {
                if (seconds < 0.1)
                    return new TimeSpan((long)(seconds * 1e6));
                return TimeSpan.FromSeconds(seconds);
            });
            statistic.Refresh();
            Assert.AreEqual(expected, statistic.Formatted);
        }
    }
}
