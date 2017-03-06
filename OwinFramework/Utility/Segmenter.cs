using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    internal class Segmenter: ISegmenter
    {
        private readonly IDependencyGraphFactory _dependencyGraphFactory;

        private Dictionary<string, Segment> _segments;
        private Dictionary<string, Node> _nodes;
        private bool _modified;
        
        #region Lifetime management

        public Segmenter(IDependencyGraphFactory dependencyGraphFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;
            Clear();
        }

        public void Clear()
        {
            _segments = new Dictionary<string, Segment>(StringComparer.InvariantCultureIgnoreCase);
            _nodes = new Dictionary<string, Node>(StringComparer.InvariantCultureIgnoreCase);
            _modified = true;
        }

        #endregion

        #region Public interface

        public void AddNode(string key, IEnumerable<IList<string>> dependencies, IEnumerable<string> segments)
        {
            if (_nodes.ContainsKey(key))
                throw new DuplicateKeyException("Node with key '" + key+"' already added to segmenter");

            var node = new Node
            {
                Key = key,
                Dependencies = dependencies == null ? new List<IList<string>>() : dependencies.ToList(),
                RequiredSegments = segments == null ? new List<string>() : segments.ToList()
            };

            _nodes[key] = node;

            _modified = true;
        }

        public void AddSegment(string name, IEnumerable<string> childSegments)
        {
            if (_segments.ContainsKey(name))
            {
                if (childSegments != null)
                {
                    var segment = _segments[name];
                    foreach (var child in childSegments)
                        segment.ChildSegmentNames.Add(child);
                }
            }
            else
            {
                _segments[name] = new Segment
                {
                    Name = name,
                    ChildSegmentNames = childSegments == null ? new List<string>() : childSegments.ToList()
                };
            }

            _modified = true;
        }

        public IList<string> GetSegmentChildren(string segmentName)
        {
            Recalculate();
            
            var parent = string.IsNullOrEmpty(segmentName) 
            ? _segments.Values.First(s => s.Parent == null)
            : _segments[segmentName];

            return parent.ChildSegmentNames;
        }

        public IList<string> GetSegmentNodes(string segmentName)
        {
            Recalculate();

            return _segments[segmentName]
                .Nodes
                .Select(n => n.Key)
                .ToList();
        }

        public IList<string> GetNodeSegments(string nodeKey)
        {
            Recalculate();

            return _nodes[nodeKey]
                .AssignedSegments
                .Select(s => s.Segment.Name)
                .ToList();
        }

        #endregion

        #region Initialization

        private void PopulateSegments()
        {
            foreach (var segment in _segments.Values.ToList())
            {
                foreach (var childName in segment.ChildSegmentNames)
                {
                    if (!_segments.ContainsKey(childName))
                        _segments[childName] = new Segment
                        {
                            Name = childName,
                            ChildSegmentNames = new List<string>(),
                            Nodes = new List<Node>()
                        };
                }
                segment.Nodes = new List<Node>();
            }

            foreach (var segment in _segments.Values.ToList())
            {
                segment.Children = segment
                    .ChildSegmentNames
                    .Select(n => _segments[n])
                    .ToList();
                segment.Parent = _segments.Values.FirstOrDefault(s => s.ChildSegmentNames.Contains(segment.Name));
            }
        }

        private void PopulateNodes()
        {
            foreach (var node in _nodes.Values)
            {
                node.DependentNodes = node
                    .Dependencies
                    .Select(nl => (IList<Node>)nl.Where(n => n != null).Select(n => _nodes[n]).ToList())
                    .ToList();
                node.AssignedSegments = new List<NodeSegmentAssignment>();
            }
        }

        #endregion

        #region Segmentation algorithm

        private void Recalculate()
        {
            if (!_modified) return;

            PopulateSegments();
            PopulateNodes();
            AssignRequiredSegments();
            ResolveMultiChoiceDependencies();
            ConsolidateCommonNodes();

            _modified = false;
        }

        /// <summary>
        /// Assigns nodes to the segments configured in the application.
        /// Adds nodes to segments that contain nodes with hard dependencies on them.
        /// Also adds hard dependencies between nodes and the other nodes
        /// that are in all of their ancestor segments.
        /// </summary>
        private void AssignRequiredSegments()
        {
            // Assign nodes to degments
            foreach (var node in _nodes.Values)
            {
                foreach (var segment in node.RequiredSegments)
                    Assign(node, _segments[segment]);
            }

            // Add dependencies to nodes where they have the same nodes
            // asigned to all ancestor segments
            foreach (var node in _nodes.Values)
            {
                var existingDependencies = node.Dependencies.SelectMany(d => d).ToList();
                List<Node> additionalDependants = null;
                foreach (var segment in _segments.Values.Where(s => s.Nodes.Contains(node)))
                {
                    var ancestorNodes = new List<Node>();
                    foreach (var ancestorSegment in SegmentAncestors(segment))
                    {
                        {
                            foreach (var ancestorNode in ancestorSegment.Nodes)
                            {
                                if (!ancestorNodes.Contains(ancestorNode))
                                    ancestorNodes.Add(ancestorNode);
                            }
                        }
                    }
                    
                    if (additionalDependants == null)
                    {
                        additionalDependants = ancestorNodes;
                        for (var i = 0; i < additionalDependants.Count; i++)
                        {
                            if (existingDependencies.Contains(additionalDependants[i].Key))
                            {
                                additionalDependants.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < additionalDependants.Count; i++)
                        {
                            if (!ancestorNodes.Contains(additionalDependants[i]))
                            {
                                additionalDependants.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                if (additionalDependants != null)
                    foreach (var additionalDependant in additionalDependants)
                        node.Dependencies.Add(new List<string> { additionalDependant.Key });
            }
        }

        /// <summary>
        /// Finds nodes with dependencies where any one of the dependant nodes
        /// will satisfy the dependency, and chooses which of those node choices
        /// will be used to satisfy the dependency.
        /// </summary>
        private void ResolveMultiChoiceDependencies()
        {
            foreach (var node in _nodes.Values)
            {
                var unresolvedList = new List<NodeSegmentAssignment>();
                var resolvedNodes = new List<Node>();
                foreach (var assignment in node.AssignedSegments)
                {
                    foreach (var nodeList in assignment.DependentNodes)
                    {
                        if (nodeList.Count > 1)
                        {
                            var dependant = ResolveMultiChoiceDependency(assignment, nodeList);
                            if (dependant == null)
                            {
                                if (nodeList.Contains(null))
                                    nodeList.Clear();
                                else
                                    unresolvedList.Add(assignment);
                            }
                            else
                            {
                                nodeList.Clear();
                                nodeList.Add(dependant);
                                resolvedNodes.Add(dependant);
                            }
                        }
                    }
                }
                foreach (var assignment in unresolvedList)
                {
                    foreach (var nodeList in assignment.DependentNodes)
                    {
                        if (nodeList.Count > 1)
                        {
                            var dependant = resolvedNodes
                                .FirstOrDefault(nodeList.Contains);
                            nodeList.Clear();
                            if (dependant != null)
                                nodeList.Add(dependant);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Recursively traverses the segmentation graph moving nodes into
        /// the parent segment where they are present in all child segments
        /// and where moving them would not break any of their dependency
        /// constraints.
        /// </summary>
        private void ConsolidateCommonNodes()
        {
            foreach (var segment in _segments.Values.Where(s => s.Parent == null))
                ConsolidateCommonNodes(segment);
        }
        
        /// <summary>
        /// Takes a node, and a list of alternate dependencies and chooses which
        /// of the dependencies will be satisfied by the segmentation graph for
        /// this particular segment assignment
        /// </summary>
        private Node ResolveMultiChoiceDependency(
            NodeSegmentAssignment assignment, 
            IList<Node> choices)
        {
            // First look for a dependant in the same segment
            var result = assignment.Segment.Nodes.FirstOrDefault(choices.Contains);

            // Second look for a dependant in an ancestor segment
            if (result == null)
            {
                var ancestor = SegmentAncestors(assignment.Segment)
                    .FirstOrDefault(s => s.Nodes.Any(choices.Contains));
                if (ancestor != null)
                    result = ancestor.Nodes.FirstOrDefault(choices.Contains);
            }

            return result;
        }

        private void ConsolidateCommonNodes(Segment segment)
        {
            // Recursively traverse the tree bottom up
            foreach (var child in segment.Children)
                ConsolidateCommonNodes(child);

            // Sort the nodes by their dependencies. You can't move a node up to
            // the parent unless you also move its dependents
            if (segment.Nodes.Count > 1)
            {
                var dependencyTree = _dependencyGraphFactory.Create<Node>();
                foreach (var node in segment.Nodes)
                {
                    var dependents = node.DependentNodes
                        .SelectMany(nl => nl)
                        .Where(n => segment.Nodes.Contains(n))
                        .Select(n => new DependencyGraphEdge { Key = n.Key });
                    dependencyTree.Add(
                        node.Key, 
                        node, 
                        dependents,
                        PipelinePosition.Middle);
                }
                segment.Nodes = dependencyTree.GetBuildOrderData().ToList();
            }

            if (segment.Children.Count < 2) return;

            var commonNodes = segment.Children[0].Nodes.ToList();

            var nodesRemoved = true;
            while (nodesRemoved)
            {
                nodesRemoved = false;
                for (var i = 1; i < segment.Children.Count; i++)
                {
                    var child = segment.Children[i];
                    foreach (var node in commonNodes.ToList())
                    {
                        if (child.Nodes.Contains(node))
                        {
                            var blockingDependantCount = node.DependentNodes
                                .SelectMany(d => d)
                                .Where(d => child.Nodes.Contains(d))
                                .Where(d => !commonNodes.Contains(d))
                                .Count();
                            if (blockingDependantCount > 0)
                            {
                                commonNodes.Remove(node);
                                nodesRemoved = true;
                                break;
                            }
                        }
                        else
                        {
                            commonNodes.Remove(node);
                            nodesRemoved = true;
                            break;
                        }
                    }
                }
            }

            foreach (var node in commonNodes)
                MoveFromChildrenToParent(segment, node);
        }

        #endregion

        #region Segmentation graph manipulation

        /// <summary>
        /// Assigns a node to a segment if it is not already assigned to
        /// it, and adds any hard dependencies to the same segment
        /// </summary>
        private void Assign(Node node, Segment segment)
        {
            if (node.AssignedSegments.All(sa => sa.Segment != segment))
            {
                AddAssignment(node, segment);
                foreach (var dependantList in node.DependentNodes)
                {
                    if (dependantList.Count == 1)
                        Assign(dependantList[0], segment);
                }
            }
        }
        
        /// <summary>
        /// Adds a node to a segment in the graph
        /// </summary>
        private void AddAssignment(Node node, Segment segment)
        {
            var assignment = new NodeSegmentAssignment
            {
                Node = node,
                Segment = segment,
                DependentNodes = new List<IList<Node>>()
            };

            foreach (var dependentNode in node.DependentNodes)
                assignment.DependentNodes.Add(dependentNode.ToList());

            node.AssignedSegments.Add(assignment);
            segment.Nodes.Add(node);
        }

        /// <summary>
        /// Deletes a node from a segment
        /// </summary>
        private void RemoveAssignment(Node node, Segment segment)
        {
            segment.Nodes.Remove(node);
            node.AssignedSegments = node.AssignedSegments
                .Where(s => s.Node != node || s.Segment != segment)
                .ToList();
        }

        /// <summary>
        /// Removes a specific node from all child segments and adds it into 
        /// the parent segment. Note that is the node has segment assignment
        /// specific dependencies they will be reset back to the node 
        /// dependencies.
        /// </summary>
        private void MoveFromChildrenToParent(Segment parentSegment, Node node)
        {
            // TODO: Combine segment assignment depdendencies
            foreach (var child in parentSegment.Children)
            {
                if (child.Nodes.Contains(node))
                    RemoveAssignment(node, child);
            }
            AddAssignment(node, parentSegment);
        }

        /// <summary>
        /// Returns a list of all nodes that appear before the specified node
        /// in ther segment graph
        /// </summary>
        private IEnumerable<Node> NodeAncestors(Node node)
        {
            var result = new List<Node>();
            foreach (var segment in _segments.Values.Where(s => s.Nodes.Contains(node)))
            {
                foreach (var ancestorSegment in SegmentAncestors(segment))
                {
                    foreach (var ancestorNode in ancestorSegment.Nodes)
                    {
                        if (!result.Contains(ancestorNode))
                            result.Add(ancestorNode);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all the segments that preceed the specified segment
        /// </summary>
        private IEnumerable<Segment> SegmentAncestors(Segment segment)
        {
            while (segment.Parent != null)
            {
                segment = segment.Parent;
                yield return segment;
            }
        }

        #endregion

        #region Segment graph components

        private class Segment : IEquatable<Segment>
        {
            public string Name;
            public IList<string> ChildSegmentNames;
            public IList<Segment> Children;
            public Segment Parent;
            public IList<Node> Nodes;

            public bool Equals(Segment other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Name, other.Name);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Segment);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public static bool operator ==(Segment s1, Segment s2)
            {
                if (ReferenceEquals(s1, null)) return ReferenceEquals(s2, null);
                return s1.Equals(s2);
            }

            public static bool operator !=(Segment s1, Segment s2)
            {
                if (ReferenceEquals(s1, null)) return !ReferenceEquals(s2, null);
                return !s1.Equals(s2);
            }
        }

        private class NodeSegmentAssignment
        {
            public Node Node;
            public Segment Segment;
            public IList<IList<Node>> DependentNodes;
        }

        private class Node: IEquatable<Node>
        {
            public string Key;
            public IList<IList<string>> Dependencies;
            public IList<string> RequiredSegments;
            public IList<NodeSegmentAssignment> AssignedSegments;
            public IList<IList<Node>> DependentNodes;

            public bool Equals(Node other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(Key, other.Key);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as Node);
            }

            public override int GetHashCode()
            {
                return Key.GetHashCode();
            }

            public static bool operator ==(Node n1, Node n2)
            {
                if (ReferenceEquals(n1, null)) return ReferenceEquals(n2, null);
                return n1.Equals(n2);
            }

            public static bool operator !=(Node n1, Node n2)
            {
                if (ReferenceEquals(n1, null)) return !ReferenceEquals(n2, null);
                return !n1.Equals(n2);
            }
        }

        #endregion
    }
}
