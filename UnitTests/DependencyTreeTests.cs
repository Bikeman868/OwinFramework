using System;
using System.Linq;
using NUnit.Framework;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Utility;

namespace UnitTests
{
    [TestFixture]
    public class Dependency_tree
    {
        private IDependencyTree<int, string> _dependencyTree;

        #region Test set up

        [SetUp]
        public void SetUp()
        {
            _dependencyTree = new DependencyTree<int, string>();
        }

        #endregion

        [Test]
        public void Should_find_added_items()
        {
            _dependencyTree.Add(1, "One", null);
            _dependencyTree.Add(2, "Two", null);
            _dependencyTree.Add(3, "Three", null);

            Assert.AreEqual("One", _dependencyTree.GetData(1));
            Assert.AreEqual("Two", _dependencyTree.GetData(2));
            Assert.AreEqual("Three", _dependencyTree.GetData(3));
        }

        [Test]
        public void Should_recurse_dependencies_top_down()
        {
            _dependencyTree.Add(1, "One", new[] { 2 });
            _dependencyTree.Add(2, "Two", new[] { 3, 4 });
            _dependencyTree.Add(3, "Three", new[] { 5 });
            _dependencyTree.Add(4, "Four", new[] { 5 });
            _dependencyTree.Add(5, "Five", new[] { 6 });
            _dependencyTree.Add(6, "Six", null);

            var dependantsOfOne = _dependencyTree.GetDecendents(1, true).ToList();
            var dependantsOfThree = _dependencyTree.GetDecendents(3, true).ToList();

            Assert.IsTrue(dependantsOfOne.IndexOf(2) < dependantsOfOne.IndexOf(3));
            Assert.IsTrue(dependantsOfOne.IndexOf(3) < dependantsOfOne.IndexOf(6));

            Assert.IsTrue(dependantsOfOne.Contains(2));
            Assert.IsTrue(dependantsOfOne.Contains(3));
            Assert.IsTrue(dependantsOfOne.Contains(4));
            Assert.IsTrue(dependantsOfOne.Contains(5));
            Assert.IsTrue(dependantsOfOne.Contains(6));

            Assert.IsTrue(dependantsOfThree.IndexOf(5) < dependantsOfOne.IndexOf(6));

            Assert.IsTrue(dependantsOfThree.Contains(5));
            Assert.IsTrue(dependantsOfThree.Contains(6));

            Assert.IsFalse(dependantsOfThree.Contains(1));
            Assert.IsFalse(dependantsOfThree.Contains(2));
            Assert.IsFalse(dependantsOfThree.Contains(3));
            Assert.IsFalse(dependantsOfThree.Contains(4));
        }

        [Test]
        public void Should_recurse_dependencies_bottom_up()
        {
            _dependencyTree.Add(1, "One", new[] { 2 });
            _dependencyTree.Add(2, "Two", new[] { 3, 4 });
            _dependencyTree.Add(3, "Three", new[] { 5 });
            _dependencyTree.Add(4, "Four", new[] { 5 });
            _dependencyTree.Add(5, "Five", new[] { 6 });
            _dependencyTree.Add(6, "Six", null);

            var dependantsOfOne = _dependencyTree.GetDecendents(1).ToList();
            var dependantsOfThree = _dependencyTree.GetDecendents(3).ToList();

            Assert.IsTrue(dependantsOfOne.IndexOf(2) > dependantsOfOne.IndexOf(3));
            Assert.IsTrue(dependantsOfOne.IndexOf(3) > dependantsOfOne.IndexOf(6));

            Assert.IsTrue(dependantsOfOne.Contains(2));
            Assert.IsTrue(dependantsOfOne.Contains(3));
            Assert.IsTrue(dependantsOfOne.Contains(4));
            Assert.IsTrue(dependantsOfOne.Contains(5));
            Assert.IsTrue(dependantsOfOne.Contains(6));

            Assert.IsTrue(dependantsOfThree.IndexOf(5) > dependantsOfOne.IndexOf(6));

            Assert.IsTrue(dependantsOfThree.Contains(5));
            Assert.IsTrue(dependantsOfThree.Contains(6));

            Assert.IsFalse(dependantsOfThree.Contains(1));
            Assert.IsFalse(dependantsOfThree.Contains(2));
            Assert.IsFalse(dependantsOfThree.Contains(3));
            Assert.IsFalse(dependantsOfThree.Contains(4));
        }

        [Test]
        public void Should_calculate_build_order()
        {
            _dependencyTree.Add(1, "One", new[] { 2 });
            _dependencyTree.Add(2, "Two", new[] { 3, 4 });
            _dependencyTree.Add(3, "Three", new[] { 5 });
            _dependencyTree.Add(4, "Four", new[] { 5 });
            _dependencyTree.Add(5, "Five", new[] { 6 });
            _dependencyTree.Add(6, "Six", null);

            var buildOrder = _dependencyTree.GetAllKeys().ToList();

            Assert.AreEqual(6, buildOrder.Count, "number of items in the build order");

            Assert.IsTrue(buildOrder.Contains(1), "build includes 1");
            Assert.IsTrue(buildOrder.Contains(2), "build includes 2");
            Assert.IsTrue(buildOrder.Contains(3), "build includes 3");
            Assert.IsTrue(buildOrder.Contains(4), "build includes 4");
            Assert.IsTrue(buildOrder.Contains(5), "build includes 5");
            Assert.IsTrue(buildOrder.Contains(6), "build includes 6");

            Assert.IsTrue(buildOrder.IndexOf(1) > buildOrder.IndexOf(2), "1 built after 2");
            Assert.IsTrue(buildOrder.IndexOf(2) > buildOrder.IndexOf(3), "2 built after 3");
            Assert.IsTrue(buildOrder.IndexOf(2) > buildOrder.IndexOf(4), "2 built after 4");
            Assert.IsTrue(buildOrder.IndexOf(3) > buildOrder.IndexOf(5), "3 built after 5");
            Assert.IsTrue(buildOrder.IndexOf(4) > buildOrder.IndexOf(5), "4 built after 5");
            Assert.IsTrue(buildOrder.IndexOf(5) > buildOrder.IndexOf(6), "5 built after 6");
        }

        [Test]
        public void Should_detect_circular_references()
        {
            _dependencyTree.Add(1, "One", new[] { 2 });
            _dependencyTree.Add(2, "Two", new[] { 3, 4 });
            _dependencyTree.Add(3, "Three", new[] { 5 });
            _dependencyTree.Add(4, "Four", new[] { 5 });
            _dependencyTree.Add(5, "Five", new[] { 3 });
            _dependencyTree.Add(6, "Six", null);

            Assert.Throws<CircularDependencyException>(() => _dependencyTree.GetAllKeys());
        }

    }
}
