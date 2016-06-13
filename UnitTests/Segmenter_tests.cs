using System.Collections.Generic;
using NUnit.Framework;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Utility;

namespace UnitTests
{
    [TestFixture]
    public class Segmenter_tests
    {
        private ISegmenter _segmenter;

        [SetUp]
        public void SetUp()
        {
            _segmenter = new Segmenter();
        }

        [Test]
        public void Should_segment_empty_graph()
        {
            _segmenter.AddSegment("S0");
            var nodes = _segmenter.GetSegmentNodes("S0");

            Assert.IsNotNull(nodes);
            Assert.AreEqual(0, nodes.Count);
        }

        [Test]
        public void Should_segment_with_no_routing()
        {
            _segmenter.AddSegment("S0");

            _segmenter.AddNode(
                "A",
                new[] { new List<string> { "B" }, new List<string> { "D" } },
                new[] { "S0" });
            _segmenter.AddNode("B");
            _segmenter.AddNode("C");
            _segmenter.AddNode("D");

            var nodes = _segmenter.GetSegmentNodes("S0");

            Assert.IsNotNull(nodes);
            Assert.AreEqual(3, nodes.Count);
        }

        [Test]
        public void Should_segment_dependency_graph_with_one_exact_solution()
        {
            // S0 has children S1 and S2
            _segmenter.AddSegment("S0", new[] { "S1", "S2" });

            // S2 has children S3 and S4
            _segmenter.AddSegment("S2", new[] { "S3", "S4" }); 

            // A depends on B or D
            _segmenter.AddNode(
                "A", 
                new[] { new List<string> { "B", "D" } });

            // B depends on C
            _segmenter.AddNode(
                "B", 
                new[] { new List<string> { "C" } });

            // C has no dependencies
            _segmenter.AddNode("C");

            // D depends on B and must be in S1
            _segmenter.AddNode(
                "D", 
                new[] { new List<string> { "B" } }, 
                new[] { "S1" });

            // E depends on A and B and must be in S4
            _segmenter.AddNode(
                "E", 
                new[] { new List<string> { "A" }, new List<string> { "B" } }, 
                new[] { "S4" }); 

            // F depends on A and E and must be in S3
            _segmenter.AddNode(
                "F",
                new[] { new List<string> { "A" }, new List<string> { "E" } },
                new[] { "S3" });

            var nodeA_Segments = _segmenter.GetNodeSegments("A");
            var nodeB_Segments = _segmenter.GetNodeSegments("B");
            var nodeC_Segments = _segmenter.GetNodeSegments("C");
            var nodeD_Segments = _segmenter.GetNodeSegments("D");
            var nodeE_Segments = _segmenter.GetNodeSegments("E");
            var nodeF_Segments = _segmenter.GetNodeSegments("F");

            Assert.AreEqual(1, nodeA_Segments.Count, "Number of segments node A assigned to");
            Assert.AreEqual(1, nodeB_Segments.Count, "Number of segments node B assigned to");
            Assert.AreEqual(1, nodeC_Segments.Count, "Number of segments node C assigned to");
            Assert.AreEqual(1, nodeD_Segments.Count, "Number of segments node D assigned to");
            Assert.AreEqual(1, nodeE_Segments.Count, "Number of segments node E assigned to");
            Assert.AreEqual(1, nodeF_Segments.Count, "Number of segments node F assigned to");

            Assert.AreEqual("S2", nodeA_Segments[0], "Node A segment assignment");
            Assert.AreEqual("S0", nodeB_Segments[0], "Node B segment assignment");
            Assert.AreEqual("S0", nodeC_Segments[0], "Node C segment assignment");
            Assert.AreEqual("S1", nodeD_Segments[0], "Node D segment assignment");
            Assert.AreEqual("S2", nodeE_Segments[0], "Node E segment assignment");
            Assert.AreEqual("S3", nodeF_Segments[0], "Node F segment assignment");
        }
    }
}
