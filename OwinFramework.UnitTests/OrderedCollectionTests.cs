using System;
using System.Linq;
using NUnit.Framework;
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
            _collection = new OrderedCollection<int>(new ArrayPool<int>(5));
        }

        [Test]
        public void Should_add_and_enumerate_items()
        {
            const int start = 100;
            const int end = 500;

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

                for (var j = 0; j < _collection.Count; j++)
                {
                    Assert.AreEqual(start + j, _collection[j]);
                }
            }
        }

        [Test]
        public void Should_set_items_by_index()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(0);

            for (var i = 0; i < 100; i++)
                _collection[i] = 100 - i;

            for (var i = 0; i < 100; i++)
                Assert.AreEqual(100 - i, _collection[i]);
        }

        [Test]
        public void Should_clear_the_collection()
        {
            for (var i = 0; i < 100; i++)
                _collection.Add(i);

            Assert.AreEqual(100, _collection.Count);
            Assert.AreEqual(100, _collection.Count());

            _collection.Clear();

            Assert.AreEqual(0, _collection.Count);
            Assert.AreEqual(0, _collection.Count());
        }

        [Test]
        public void Should_add_other_collections()
        {
            var list1 = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var list2 = new[] { 18, 17, 16,15, 14, 13, 12, 11 };

            _collection.AddRange(list1);
            _collection.AddRange(list2);
            _collection.AddRange(list1);

            Assert.AreEqual(list1.Length * 2 + list2.Length, _collection.Count);

            var i = 0;

            foreach(var n in list1)
                Assert.AreEqual(n, _collection[i++]);

            foreach(var n in list2)
                Assert.AreEqual(n, _collection[i++]);

            foreach(var n in list1)
                Assert.AreEqual(n, _collection[i++]);
        }

        [Test]
        public void Should_remove_items_from_the_collection()
        {
            var items = new[] { 1, 12, 3, 8, 7, 3, 1, 19, 25, 87, 3, 7 };
            _collection.AddRange(items);

            Assert.AreEqual(items.Length, _collection.Count);

            Assert.IsFalse(_collection.Remove(21));
            Assert.AreEqual(items.Length, _collection.Count);

            Assert.IsTrue(_collection.Remove(19));
            Assert.AreEqual(items.Length - 1, _collection.Count);

            var i = 0;
            foreach (var value in items.Where(v => v != 19))
                Assert.AreEqual(value, _collection[i++]);

            Assert.IsTrue(_collection.Remove(3));
            Assert.AreEqual(items.Length - 4, _collection.Count);

            i = 0;
            foreach (var value in items.Where(v => v != 19 && v != 3))
                Assert.AreEqual(value, _collection[i++]);
        }

        [Test]
        public void Should_find_index()
        {
            var items = new[] { 1, 12, 3, 8, 7, 3, 1, 19, 25, 87, 3, 7 };
            _collection.AddRange(items);

            Assert.AreEqual(items.Length, _collection.Count);

            Assert.AreEqual(-1, _collection.IndexOf(99));

            for (var i = 0; i < items.Length; i++)
            {
                var value = items[i];
                Assert.AreEqual(Array.IndexOf(items, value), _collection.IndexOf(items[i]));
            }
        }

        [Test]
        public void Should_insert_into_the_list()
        {
            var items = new[] { 1, 3, 4, 5, 7, 8, 10 };
            _collection.AddRange(items);

            Assert.AreEqual(items.Length, _collection.Count);

            _collection.Insert(0, 0);

            Assert.AreEqual(items.Length + 1, _collection.Count);
            Assert.AreEqual(0, _collection[0]);
            Assert.AreEqual(1, _collection[1]);
            Assert.AreEqual(3, _collection[2]);

            _collection.Insert(2, 2);

            Assert.AreEqual(items.Length + 2, _collection.Count);
            Assert.AreEqual(0, _collection[0]);
            Assert.AreEqual(1, _collection[1]);
            Assert.AreEqual(2, _collection[2]);
            Assert.AreEqual(3, _collection[3]);

            _collection.Insert(_collection.Count, 11);

            Assert.AreEqual(items.Length + 3, _collection.Count);
            Assert.AreEqual(0, _collection[0]);
            Assert.AreEqual(1, _collection[1]);
            Assert.AreEqual(2, _collection[2]);
            Assert.AreEqual(3, _collection[3]);
            Assert.AreEqual(11, _collection[_collection.Count - 1]);
        }

        [Test]
        public void Should_remove_by_index()
        {
            var items = new[] { 1, 2, 3, 4, 5, 6, 7 };
            _collection.AddRange(items);

            Assert.AreEqual(items.Length, _collection.Count);

            _collection.RemoveAt(2);

            Assert.AreEqual(items.Length - 1, _collection.Count);
            Assert.AreEqual(1, _collection[0]);
            Assert.AreEqual(2, _collection[1]);
            Assert.AreEqual(4, _collection[2]);
            Assert.AreEqual(5, _collection[3]);

            _collection.RemoveAt(0);

            Assert.AreEqual(items.Length - 2, _collection.Count);
            Assert.AreEqual(2, _collection[0]);
            Assert.AreEqual(4, _collection[1]);
            Assert.AreEqual(5, _collection[2]);
            Assert.AreEqual(6, _collection[3]);
        }
    }
}
