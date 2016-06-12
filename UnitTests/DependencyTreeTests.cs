using System;
using System.Linq;
using NUnit.Framework;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Utility;

namespace UnitTests
{
    [TestFixture]
    public class Dependency_tree
    {
        private IDependencyTree<string> _dependencyTree;

        [SetUp]
        public void SetUp()
        {
            _dependencyTree = new DependencyTree<string>();
        }

        [Test]
        public void Should_find_added_items()
        {
            _dependencyTree.Add("1", "One", null, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", null, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", null, PipelinePosition.Middle);

            Assert.AreEqual("One", _dependencyTree.GetData("1"));
            Assert.AreEqual("Two", _dependencyTree.GetData("2"));
            Assert.AreEqual("Three", _dependencyTree.GetData("3"));
        }

        [Test]
        public void Should_recurse_dependencies_top_down()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", new[] { new TreeDependency { Key = "6" } }, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            var dependantsOfOne = _dependencyTree.GetDecendents("1", true).ToList();
            var dependantsOfThree = _dependencyTree.GetDecendents("3", true).ToList();

            Assert.IsTrue(dependantsOfOne.IndexOf("2") < dependantsOfOne.IndexOf("3"));
            Assert.IsTrue(dependantsOfOne.IndexOf("3") < dependantsOfOne.IndexOf("6"));

            Assert.IsTrue(dependantsOfOne.Contains("2"));
            Assert.IsTrue(dependantsOfOne.Contains("3"));
            Assert.IsTrue(dependantsOfOne.Contains("4"));
            Assert.IsTrue(dependantsOfOne.Contains("5"));
            Assert.IsTrue(dependantsOfOne.Contains("6"));

            Assert.IsTrue(dependantsOfThree.IndexOf("5") < dependantsOfOne.IndexOf("6"));

            Assert.IsTrue(dependantsOfThree.Contains("5"));
            Assert.IsTrue(dependantsOfThree.Contains("6"));

            Assert.IsFalse(dependantsOfThree.Contains("1"));
            Assert.IsFalse(dependantsOfThree.Contains("2"));
            Assert.IsFalse(dependantsOfThree.Contains("3"));
            Assert.IsFalse(dependantsOfThree.Contains("4"));
        }

        [Test]
        public void Should_recurse_dependencies_bottom_up()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", new[] { new TreeDependency { Key = "6" } }, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            var dependantsOfOne = _dependencyTree.GetDecendents("1").ToList();
            var dependantsOfThree = _dependencyTree.GetDecendents("3").ToList();

            Assert.IsTrue(dependantsOfOne.IndexOf("2") > dependantsOfOne.IndexOf("3"));
            Assert.IsTrue(dependantsOfOne.IndexOf("3") > dependantsOfOne.IndexOf("6"));

            Assert.IsTrue(dependantsOfOne.Contains("2"));
            Assert.IsTrue(dependantsOfOne.Contains("3"));
            Assert.IsTrue(dependantsOfOne.Contains("4"));
            Assert.IsTrue(dependantsOfOne.Contains("5"));
            Assert.IsTrue(dependantsOfOne.Contains("6"));

            Assert.IsTrue(dependantsOfThree.IndexOf("5") > dependantsOfOne.IndexOf("6"));

            Assert.IsTrue(dependantsOfThree.Contains("5"));
            Assert.IsTrue(dependantsOfThree.Contains("6"));

            Assert.IsFalse(dependantsOfThree.Contains("1"));
            Assert.IsFalse(dependantsOfThree.Contains("2"));
            Assert.IsFalse(dependantsOfThree.Contains("3"));
            Assert.IsFalse(dependantsOfThree.Contains("4"));
        }

        [Test]
        public void Should_calculate_build_order()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", new[] { new TreeDependency { Key = "6" } }, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            var buildOrder = _dependencyTree.GetBuildOrderKeys().ToList();

            Assert.AreEqual(6, buildOrder.Count, "number of items in the build order");

            Assert.IsTrue(buildOrder.Contains("1"), "build includes 1");
            Assert.IsTrue(buildOrder.Contains("2"), "build includes 2");
            Assert.IsTrue(buildOrder.Contains("3"), "build includes 3");
            Assert.IsTrue(buildOrder.Contains("4"), "build includes 4");
            Assert.IsTrue(buildOrder.Contains("5"), "build includes 5");
            Assert.IsTrue(buildOrder.Contains("6"), "build includes 6");

            Assert.IsTrue(buildOrder.IndexOf("1") > buildOrder.IndexOf("2"), "1 built after 2");
            Assert.IsTrue(buildOrder.IndexOf("2") > buildOrder.IndexOf("3"), "2 built after 3");
            Assert.IsTrue(buildOrder.IndexOf("2") > buildOrder.IndexOf("4"), "2 built after 4");
            Assert.IsTrue(buildOrder.IndexOf("3") > buildOrder.IndexOf("5"), "3 built after 5");
            Assert.IsTrue(buildOrder.IndexOf("4") > buildOrder.IndexOf("5"), "4 built after 5");
            Assert.IsTrue(buildOrder.IndexOf("5") > buildOrder.IndexOf("6"), "5 built after 6");
        }

        [Test]
        public void Should_detect_circular_references()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", new[] { new TreeDependency { Key = "3" } }, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            Assert.Throws<CircularDependencyException>(() => _dependencyTree.GetBuildOrderKeys());
        }

        [Test]
        public void Missing_optional_dependencies_should_not_error()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "99" } }, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", new[] { new TreeDependency { Key = "6" } }, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            var buildOrder = _dependencyTree.GetBuildOrderKeys().ToList();

            Assert.AreEqual(6, buildOrder.Count, "number of items in the build order");

            Assert.IsTrue(buildOrder.Contains("1"), "build includes 1");
            Assert.IsTrue(buildOrder.Contains("2"), "build includes 2");
            Assert.IsTrue(buildOrder.Contains("3"), "build includes 3");
            Assert.IsTrue(buildOrder.Contains("4"), "build includes 4");
            Assert.IsTrue(buildOrder.Contains("5"), "build includes 5");
            Assert.IsTrue(buildOrder.Contains("6"), "build includes 6");
        }

        [Test]
        public void Missing_required_dependencies_should_error()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "99", Required = true } }, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", new[] { new TreeDependency { Key = "6" } }, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            Assert.Throws<MissingDependencyException>(() => _dependencyTree.GetBuildOrderKeys());
        }

        [Test]
        public void Orphan_nodes_should_be_included()
        {
            _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
            _dependencyTree.Add("2", "Two", null, PipelinePosition.Middle);
            _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
            _dependencyTree.Add("4", "Four", null, PipelinePosition.Middle);
            _dependencyTree.Add("5", "Five", null, PipelinePosition.Middle);
            _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);

            var buildOrder = _dependencyTree.GetBuildOrderKeys().ToList();

            Assert.AreEqual(6, buildOrder.Count, "number of items in the build order");

            Assert.IsTrue(buildOrder.Contains("1"), "build includes 1");
            Assert.IsTrue(buildOrder.Contains("2"), "build includes 2");
            Assert.IsTrue(buildOrder.Contains("3"), "build includes 3");
            Assert.IsTrue(buildOrder.Contains("4"), "build includes 4");
            Assert.IsTrue(buildOrder.Contains("5"), "build includes 5");
            Assert.IsTrue(buildOrder.Contains("6"), "build includes 6");
        }

        [Test]
        public void Should_not_allow_duplicate_keys()
        {
            Assert.Throws<DuplicateKeyException>(() =>
            {
                _dependencyTree.Add("1", "One", new[] { new TreeDependency { Key = "2" } }, PipelinePosition.Middle);
                _dependencyTree.Add("2", "Two", new[] { new TreeDependency { Key = "3" }, new TreeDependency { Key = "4" } }, PipelinePosition.Middle);
                _dependencyTree.Add("3", "Three", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
                _dependencyTree.Add("4", "Four", new[] { new TreeDependency { Key = "5" } }, PipelinePosition.Middle);
                _dependencyTree.Add("1", "Five", new[] { new TreeDependency { Key = "3" } }, PipelinePosition.Middle);
                _dependencyTree.Add("6", "Six", null, PipelinePosition.Middle);
            });
        }

        [Test]
        [TestCase("4", PipelinePosition.Front, new[] { "6", "5", "4", "3", "2", "1" })]
        [TestCase("3", PipelinePosition.Front, new[] { "6", "5", "3" })]
        [TestCase("4", PipelinePosition.Back, new[] { "6", "5", "3", "2", "1", "4" })]
        [TestCase("3", PipelinePosition.Back, new[] { "6", "5", "4", "3", "2", "1" })]
        public void Should_put_nodes_in_position(string key, PipelinePosition keyPosition, string[] expectedOrder)
        {
            Action<string, string, string[]> add = (k,v,d) => 
                _dependencyTree.Add(
                    k, 
                    v, 
                    d == null ? null : d.Select(dk => new TreeDependency { Key = dk }),
                    k == key ? keyPosition : PipelinePosition.Middle);

            add("1", "One", new[] { "2" } );
            add("2", "Two", new[] { "3" });
            add("3", "Three", new[] { "5" });
            add("4", "Four", new[] { "5" });
            add("5", "Five", new[] { "6" });
            add("6", "Six", null);

            var buildOrder = _dependencyTree.GetBuildOrderKeys().ToList();

            Assert.AreEqual(6, buildOrder.Count, "number of items in the build order");

            for (var i = 0; i < expectedOrder.Length; i++)
                Assert.AreEqual(expectedOrder[i], buildOrder[i]);
        }

    }
}
