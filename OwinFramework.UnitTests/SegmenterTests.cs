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
            _segmenter = new Segmenter(new DependencyGraphFactory());
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

            var nodeASegments = _segmenter.GetNodeSegments("A");
            var nodeBSegments = _segmenter.GetNodeSegments("B");
            var nodeCSegments = _segmenter.GetNodeSegments("C");
            var nodeDSegments = _segmenter.GetNodeSegments("D");
            var nodeESegments = _segmenter.GetNodeSegments("E");
            var nodeFSegments = _segmenter.GetNodeSegments("F");

            Assert.AreEqual(1, nodeASegments.Count, "Number of segments node A assigned to");
            Assert.AreEqual(1, nodeBSegments.Count, "Number of segments node B assigned to");
            Assert.AreEqual(1, nodeCSegments.Count, "Number of segments node C assigned to");
            Assert.AreEqual(1, nodeDSegments.Count, "Number of segments node D assigned to");
            Assert.AreEqual(1, nodeESegments.Count, "Number of segments node E assigned to");
            Assert.AreEqual(1, nodeFSegments.Count, "Number of segments node F assigned to");

            Assert.AreEqual("S2", nodeASegments[0], "Node A segment assignment");
            Assert.AreEqual("S0", nodeBSegments[0], "Node B segment assignment");
            Assert.AreEqual("S0", nodeCSegments[0], "Node C segment assignment");
            Assert.AreEqual("S1", nodeDSegments[0], "Node D segment assignment");
            Assert.AreEqual("S2", nodeESegments[0], "Node E segment assignment");
            Assert.AreEqual("S3", nodeFSegments[0], "Node F segment assignment");
        }

        [Test]
        public void Should_segment_dependency_graph_for_real_world_example()
        {
            _segmenter.AddSegment("root", new[] { "api", "ui" });
            _segmenter.AddSegment("ui", new[] { "secure", "public" });

            // REST on api route requires identification
            _segmenter.AddNode(
                "REST",
                new[] { new List<string> { "certId", "formsId", "anonymousId" } },
                new[] { "api" });

            // MVC on both secure and public route needs session
            _segmenter.AddNode(
                "MVC",
                new[] { new List<string> { "session" } },
                new[] { "secure", "public" });

            // Session requires requires identification
            _segmenter.AddNode(
                "session",
                new[] { new List<string> { "certId", "formsId", "anonymousId" } });

            // Secure route must have login forms identification
            _segmenter.AddNode(
                "formsId",
                null,
                new[] { "secure" });

            // API route must have certificate identification
            _segmenter.AddNode(
                "certId",
                null,
                new[] { "api" });

            // Public route must have anonymous identification
            _segmenter.AddNode(
                "anonymousId",
                null,
                new[] { "public" });

            var restSegments = _segmenter.GetNodeSegments("REST");
            var mvcSegments = _segmenter.GetNodeSegments("MVC");
            var certIdSegments = _segmenter.GetNodeSegments("certId");
            var formsIdSegments = _segmenter.GetNodeSegments("formsId");
            var anonymousIdSegments = _segmenter.GetNodeSegments("anonymousId");
            var sessionSegments = _segmenter.GetNodeSegments("session");

            Assert.AreEqual(1, restSegments.Count, "Number of segments REST assigned to");
            Assert.AreEqual("api", restSegments[0], "REST on API route");

            Assert.AreEqual(2, mvcSegments.Count, "Number of segments MVC assigned to");
            Assert.IsTrue(mvcSegments.Contains("secure"), "MVC on secure route");
            Assert.IsTrue(mvcSegments.Contains("public"), "MVC on public route");

            Assert.AreEqual(1, certIdSegments.Count, "Number of segments cert Id assigned to");
            Assert.AreEqual("api", certIdSegments[0], "Cert Id on api route");

            Assert.AreEqual(1, formsIdSegments.Count, "Number of segments forms Id assigned to");
            Assert.AreEqual("secure", formsIdSegments[0], "Forms Id on secure route");

            Assert.AreEqual(1, anonymousIdSegments.Count, "Number of segments anonymous Id assigned to");
            Assert.AreEqual("public", anonymousIdSegments[0], "Anonymous Id on secure route");

            Assert.AreEqual(2, sessionSegments.Count, "Number of segments session assigned to");
            Assert.IsTrue(sessionSegments.Contains("secure"), "Session on secure route");
            Assert.IsTrue(sessionSegments.Contains("public"), "Session on public route");
        }
    }
}
