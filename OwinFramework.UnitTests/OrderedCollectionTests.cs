using System.Collections.Generic;
using NUnit.Framework;
using OwinFramework.Utility;
using OwinFramework.Utility.Containers;

namespace UnitTests
{
    [TestFixture]
    public class OrderedCollectionTests
    {
        private OrderedCollection<int> _collection;

        [SetUp]
        public void SetUp()
        {
            _collection = new OrderedCollection<int>(new ArrayPool<int>(20));
        }

        [Test]
        public void Should_add_and_enumerate_items()
        {
            const int start = 100;
            const int end = 5000;

            for (var i = start; i < end; i++)
            {
                _collection.Add(i);

                var expected = start;
                foreach (var v in _collection)
                {
                    Assert.AreEqual(expected, v);
                    expected++;
                }

                Assert.AreEqual(i + 1, expected);
            }
        }
    }
}
