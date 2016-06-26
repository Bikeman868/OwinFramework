using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OwinFramework.Builder;

namespace UnitTests
{
    [TestFixture]
    public class ShortStringsTest
    {
        [Test]
        [TestCase(ulong.MinValue, "a")]
        [TestCase(1ul, "b")]
        [TestCase(2ul, "c")]
        [TestCase(3ul, "d")]
        [TestCase(61ul, "9")]
        [TestCase(62ul, "ba")]
        [TestCase(63ul, "bb")]
        [TestCase(64ul, "bc")]
        [TestCase(ulong.MaxValue, "v8QrKbgkrIp")]
        public void Should_shorten_ulong_mixed_case(ulong value, string expected)
        {
            var text = value.ToShortString();
            Assert.AreEqual(expected, text);
        }

        [Test]
        [TestCase(ulong.MinValue, "a")]
        [TestCase(1ul, "b")]
        [TestCase(2ul, "c")]
        [TestCase(3ul, "d")]
        [TestCase(35ul, "9")]
        [TestCase(36ul, "ba")]
        [TestCase(37ul, "bb")]
        [TestCase(38ul, "bc")]
        [TestCase(ulong.MaxValue, "d6fobbcge2q2p")]
        public void Should_shorten_ulong_lower_case(ulong value, string expected)
        {
            var text = value.ToShortString(false);
            Assert.AreEqual(expected, text);
        }

        [Test]
        public void Should_shorten_guid_mixed_case()
        {
            var text = Guid.NewGuid().ToShortString();
            Assert.IsTrue(text.Length == 22);
        }

        [Test]
        public void Should_shorten_guid_lower_case()
        {
            var text = Guid.NewGuid().ToShortString(false);
            Assert.IsTrue(text.Length == 26);
        }
    }
}
