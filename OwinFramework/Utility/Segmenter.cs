﻿using System;
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
                .AssignedNodes
                .Select(a => a.Node.Key)
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

        public IList<string> GetNodeSegmentDependencies(string nodeKey, string segmentName)
        {
            Recalculate();

            var assignment = _nodes[nodeKey]
                .AssignedSegments
                .FirstOrDefault(a => a.Segment.Name == segmentName);
            if (assignment == null) return null;

            var segmentNodes = GetSegmentNodes(segmentName);

            return assignment
                .DependentNodes
                .Select(n => n[0].Key)
                .Where(segmentNodes.Contains)
                .ToList();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Evaluate all parent/child relationships, create missing segments
        /// and add references between parents and children
        /// </summary>
        private void PopulateSegments()
        {
            foreach (var segment in _segments.Values.ToList())
            {
                foreach (var childName in segment.ChildSegmentNames)
                {
                    if (!_segments.ContainsKey(childName))
                    {
                        _segments[childName] = new Segment
                        {
                            Name = childName,
                            ChildSegmentNames = new List<string>(),
                            AssignedNodes = new List<NodeSegmentAssignment>()
                        };
                    }
                }
                segment.AssignedNodes = new List<NodeSegmentAssignment>();
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

        /// <summary>
        /// Evaluate node dependencies and store references to dependant nodes
        /// </summary>
        private void PopulateNodes()
        {
            foreach (var node in _nodes.Values)
            {
                node.DependentNodes = node
                    .Dependencies
                    .Select(nl => (IList<Node>)nl.Select(n => n == null ? null : _nodes[n]).ToList())
                    .Where(nl => nl.Count > 0)
                    .ToList();
                node.AssignedSegments = new List<NodeSegmentAssignment>();
            }
        }

        #endregion

        #region Segmentation algorithm

        private void Recalculate()
        {
            if (!_modified) return;

            // Initialization
            PopulateSegments();
            PopulateNodes();

            // Segmentation algorithm
            FixMissingDependencies();
            AssignRequiredSegments();
            DuplicateHardDependencies();
            AssignUnassignedNodes();
            ResolveMultiChoiceDependencies();
            ConsolidateCommonNodes();
            CheckOptionalDependancies();

            _modified = false;
        }

        /// <summary>
        /// Assigns nodes to the segments configured in the application.
        /// </summary>
        private void AssignRequiredSegments()
        {
            foreach (var node in _nodes.Values)
            {
                foreach (var segment in node.RequiredSegments)
                    Assign(node, _segments[segment]);
            }
        }

        /// <summary>
        /// Find nodes that are not assigned to any segment and add them as close
        /// to the root segment as possible with all of their dependencies met
        /// </summary>
        private void AssignUnassignedNodes()
        {
            var count = -1;
            while (count != 0)
            {
                count = 0;
                foreach (var node in _nodes.Values.Where(n => n.AssignedSegments.Count == 0))
                {
                    var segment = FindHighestSegment(node);
                    if (segment != null)
                    {
                        AddAssignment(node, segment);
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// Where there are optional dependencies thatt are not being satisfied,
        /// try to rearange nodes so that as many optional dependencies as possible
        /// are satisfied.
        /// </summary>
        private void CheckOptionalDependancies()
        {
            int count;
            do
            {
                count = 0;
                TraverseBottomUp(RootSegment(), s => count += CheckOptionalDependancies(s));
            }
            while (count != 0) ;
        }

        private int CheckOptionalDependancies(Segment segment)
        {
            var assignmentsToMove = new List<NodeSegmentAssignment>();
            Action<NodeSegmentAssignment> move = null;
            move = assignment =>
            {
                if (!assignmentsToMove.Contains(assignment))
                {
                    assignmentsToMove.Add(assignment);

                    var dependantsToMove = segment.AssignedNodes
                        .Where(
                            an => an.DependentNodes
                            .Select(n => n[0])
                            .Where(n => n != null)
                            .Contains(assignment.Node));
                    foreach (var dm in dependantsToMove) 
                        move(dm);
                }
            };

            var descendants = NodeDescendants(segment).ToList();
            foreach (var assignment in segment.AssignedNodes)
            {
                var softDependencies = assignment.DependentNodes
                    .SelectMany(n => n)
                    .Where(n => n != null)
                    .ToList();
                if (descendants.Any(softDependencies.Contains))
                {
                    move(assignment);
                }
            }

            foreach (var assignment in assignmentsToMove)
            {
                MoveFromParentToChildren(segment, assignment.Node);
            }

            return assignmentsToMove.Count;
        }

        /// <summary>
        /// Add dependencies to nodes where they have the same nodes
        /// asigned to all ancestor segments. If the application configured
        /// nodes in all of the parent segments it must intend these to run first.
        /// </summary>
        private void FixMissingDependencies()
        {
            foreach (var node in _nodes.Values)
            {
                var nodeDependencies = node
                    .DependentNodes
                    .SelectMany(d => d)
                    .Where(d => d != null)
                    .Distinct()
                    .ToList();

                var ancestorSegments = new List<Segment>();
                foreach (var requiredSegmentName in node.RequiredSegments)
                {
                    foreach (var ancestor in SegmentAncestors(_segments[requiredSegmentName]))
                    {
                        if (!ancestorSegments.Contains(ancestor))
                            ancestorSegments.Add(ancestor);
                    }
                }

                foreach (var ancestorSegment in ancestorSegments)
                {
                    var additionalDependencies = _nodes.Values
                        .Where(n => n.RequiredSegments.Contains(ancestorSegment.Name))
                        .Where(n => !nodeDependencies.Contains(n))
                        .Where(n => n != node);
                    foreach (var additionalDependency in additionalDependencies)
                    {
                        nodeDependencies.Add(additionalDependency);
                        node.DependentNodes.Add(new List<Node> { additionalDependency });
                    }
                }
            }
        }

        /// <summary>
        /// Where the application defines a hard dependency that is not met by
        /// segment assigmnents, duplicate the dependant nodes onto the same
        /// segments as the nodes that depend on them.
        /// </summary>
        private void DuplicateHardDependencies()
        {
            int count = -1;
            while (count != 0)
            {
                count = 0;
                foreach (var segment in _segments.Values)
                    count += DuplicateHardDependencies(segment);
            }
        }

        /// <summary>
        /// Where the application defines a hard dependency that is not met by
        /// segment assigmnents, duplicate the dependant nodes onto the same
        /// segments as the nodes that depend on them.
        /// </summary>
        private int DuplicateHardDependencies(Segment segment)
        {
            var ancestorNodes = NodeAncestors(segment);
            var segmentNodes = segment.AssignedNodes.Select(a => a.Node).ToList();
            var nodesToAdd = new List<Node>();

            foreach(var node in segment
                .AssignedNodes
                .SelectMany(n => n.DependentNodes)
                .Where(d => d.Count == 1)
                .Select(d => d[0]))
            {
                if (!ancestorNodes.Contains(node) &&
                    !segmentNodes.Contains(node) &&
                    !nodesToAdd.Contains(node))
                    nodesToAdd.Add(node);
            }

            foreach (var node in nodesToAdd)
                Assign(node, segment);

            return nodesToAdd.Count;
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
                                if (!nodeList.Contains(null))
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
        /// the parent segment where they are present more than one child segment
        /// and where moving them would not break any of their dependencies
        /// </summary>
        private void ConsolidateCommonNodes()
        {
            var rootSegment = RootSegment();
            TraverseBottomUp(rootSegment, SortNodesByDependencies);
            TraverseBottomUp(rootSegment, ConsolidateCommonNodes);
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
            var result = assignment.Segment.AssignedNodes.Select(a => a.Node).FirstOrDefault(choices.Contains);

            // Second look for a dependant in an ancestor segment
            if (result == null)
            {
                var ancestor = SegmentAncestors(assignment.Segment)
                    .FirstOrDefault(s => s.AssignedNodes.Select(a => a.Node).Any(choices.Contains));
                if (ancestor != null)
                    result = ancestor.AssignedNodes.Select(a => a.Node).FirstOrDefault(choices.Contains);
            }

            return result;
        }

        /// <summary>
        /// Arranges the nodes within the segment so that dependant nodes
        /// are after the nodes that they depend on
        /// </summary>
        private void SortNodesByDependencies(Segment segment)
        {
            if (segment.AssignedNodes.Count < 2) return;

            var dependencyTree = _dependencyGraphFactory.Create<NodeSegmentAssignment>();
            foreach (var assignment in segment.AssignedNodes)
            {
                var dependents = assignment.DependentNodes
                    .SelectMany(nl => nl)
                    .Where(n => segment.AssignedNodes.Select(a => a.Node).Contains(n))
                    .Select(n => new DependencyGraphEdge { Key = n.Key });
                dependencyTree.Add(
                    assignment.Node.Key,
                    assignment, 
                    dependents,
                    PipelinePosition.Middle);
            }
            segment.AssignedNodes = dependencyTree.GetBuildOrderData().ToList();
        }

        /// <summary>
        /// Move nodes into the parent segment where they are present multiple 
        /// child segments and where moving them would not break any of their 
        /// dependencies
        /// </summary>
        private void ConsolidateCommonNodes(Segment segment)
        {
            // You have to have at least two children for the children to share
            // common nodes.
            if (segment.Children.Count < 2) return;

            var commonAssignments = segment.Children[0].AssignedNodes.ToList();

            var nodesRemoved = true;
            while (nodesRemoved)
            {
                nodesRemoved = false;
                for (var i = 1; i < segment.Children.Count; i++)
                {
                    var child = segment.Children[i];
                    foreach (var assignment in commonAssignments.ToList())
                    {
                        if (child.AssignedNodes.Select(a => a.Node).Contains(assignment.Node))
                        {
                            var blockingDependantCount = assignment
                                .DependentNodes
                                .SelectMany(d => d)
                                .Where(d => child
                                    .AssignedNodes
                                    .Select(a => a.Node)
                                    .Contains(d))
                                .Count(d => 
                                    !commonAssignments
                                    .Select(a => a.Node)
                                    .Contains(d));
                            if (blockingDependantCount > 0)
                            {
                                commonAssignments.Remove(assignment);
                                nodesRemoved = true;
                                break;
                            }
                        }
                        else
                        {
                            commonAssignments.Remove(assignment);
                            nodesRemoved = true;
                            break;
                        }
                    }
                }
            }

            foreach (var assignment in commonAssignments)
                MoveFromChildrenToParent(segment, assignment.Node);
        }

        #endregion

        #region Segmentation graph manipulation

        private void RemoveAllDuplicates()
        {
            var removedCount = -1;
            while (removedCount != 0)
            {
                removedCount = 0;
                foreach (var segment in _segments.Values)
                    removedCount += RemoveDuplicates(segment);
            }
        }

        /// <summary>
        /// Removes nodes from child segments if these nodes are already
        /// included in an ancestor segment.
        /// </summary>
        private int RemoveDuplicates(Segment segment)
        {
            var ancestorNodes = NodeAncestors(segment);
            var nodesToRemove = segment
                .AssignedNodes
                .Select(a => a.Node)
                .Where(ancestorNodes.Contains)
                .ToList();

            foreach (var node in nodesToRemove)
                RemoveAssignment(node, segment);

            return nodesToRemove.Count;
        }

        /// <summary>
        /// Assigns a node to a segment if it is not already assigned to
        /// it, and adds any hard dependencies to the same segment unless
        /// the dependants are already in a parent segment
        /// </summary>
        private void Assign(Node node, Segment segment)
        {
            if (node.AssignedSegments.All(sa => sa.Segment != segment))
            {
                AddAssignment(node, segment);
            }
        }

        /// <summary>
        /// Finds the segment closest to the root where all of a nodes
        /// dependencies have been met.
        /// </summary>
        private Segment FindHighestSegment(Node node)
        {
            
            var unsatisfiedDependencies = node.DependentNodes.Where(d => !d.Contains(null)).ToList();
            var rootSegment = _segments.Values.First(s => s.Parent == null);

            int depth;
            return FindHighestSegment(unsatisfiedDependencies, rootSegment, out depth);
        }

        /// <summary>
        /// Recursively traverses the segment tree finding the segment closest to
        /// the start segment that satisfies all of the dependancies
        /// </summary>
        private Segment FindHighestSegment(IList<IList<Node>> dependencies, Segment segment, out int depth)
        {
            depth = 0;
            if (dependencies.Count == 0)
                return segment;

            var segmentNodes = segment
                .AssignedNodes
                .Select(a => a.Node)
                .ToList();

            var unsatisfiedDependencies = dependencies
                .Where(d => !d.Any(segmentNodes.Contains))
                .ToList();

            if (unsatisfiedDependencies.Count == 0)
                return segment;

            depth = int.MaxValue;
            Segment shallowestChild = null;
            foreach(var child in segment.Children)
            {
                int childDepth;
                var childSegment = FindHighestSegment(unsatisfiedDependencies, child, out childDepth);
                if (childSegment != null && (childDepth + 1) < depth)
                {
                    shallowestChild = childSegment;
                    depth = childDepth + 1;
                }
            }

            return shallowestChild;
        }
        
        /// <summary>
        /// Adds a node to a segment in the graph
        /// </summary>
        private NodeSegmentAssignment AddAssignment(Node node, Segment segment)
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
            segment.AssignedNodes.Add(assignment);

            return assignment;
        }

        /// <summary>
        /// Deletes a node from a segment
        /// </summary>
        private void RemoveAssignment(Node node, Segment segment)
        {
            segment.AssignedNodes = segment.AssignedNodes
                .Where(s => s.Node != node || s.Segment != segment)
                .ToList();

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
        private void MoveFromChildrenToParent(Segment segment, Node node)
        {
            foreach (var child in segment.Children)
            {
                if (child.AssignedNodes.Select(a => a.Node).Contains(node))
                    RemoveAssignment(node, child);
            }
            AddAssignment(node, segment);
        }

        /// <summary>
        /// Removes a node from a parent segment and duplicates it in all the
        /// child segments.
        /// </summary>
        private void MoveFromParentToChildren(Segment segment, Node node)
        {
            var parentAssignment = segment.AssignedNodes.FirstOrDefault(a => a.Node == node);
            if (parentAssignment == null) return;

            foreach (var child in segment.Children)
            {
                if (!child.AssignedNodes.Select(a => a.Node).Contains(node))
                {
                    var childAssignment = AddAssignment(node, child);

                    childAssignment.DependentNodes = new List<IList<Node>>();
                    foreach (var dependentNode in parentAssignment.DependentNodes)
                        childAssignment.DependentNodes.Add(dependentNode.ToList());
                }
            }
            RemoveAssignment(node, segment);
        }

        /// <summary>
        /// Returns the segment at the root of the segmentation graph
        /// </summary>
        private Segment RootSegment()
        {
            return _segments.Values.FirstOrDefault(s => s.Parent == null);
        }

        /// <summary>
        /// Performs an action in all decendants of the given segment. The tree
        /// is traversed top down
        /// </summary>
        private void TraverseTopDown(Segment segment, Action<Segment> action)
        {
            action(segment);
            foreach (var child in segment.Children)
                TraverseTopDown(child, action);
        }

        /// <summary>
        /// Performs an action in all decendants of the given segment. The tree
        /// is traversed top down
        /// </summary>
        private void TraverseBottomUp(Segment segment, Action<Segment> action)
        {
            foreach (var child in segment.Children)
                TraverseBottomUp(child, action);
            action(segment);
        }

        /// <summary>
        /// Returns a list of all nodes that appear before the specified node
        /// in any part of the segment graph
        /// </summary>
        private IList<Node> NodeAncestors(Node node)
        {
            var result = new List<Node>();
            foreach (var segment in _segments.Values.Where(s => s.AssignedNodes.Select(a => a.Node).Contains(node)))
            {
                foreach (var ancestorSegment in SegmentAncestors(segment))
                {
                    foreach (var ancestorNode in ancestorSegment.AssignedNodes.Select(a => a.Node))
                    {
                        if (!result.Contains(ancestorNode))
                            result.Add(ancestorNode);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all nodes that appear before the specified segment
        /// </summary>
        private IList<Node> NodeAncestors(Segment segment)
        {
            var result = new List<Node>();
            foreach (var ancestorSegment in SegmentAncestors(segment))
            {
                foreach (var assignment in ancestorSegment.AssignedNodes)
                {
                    if (!result.Contains(assignment.Node))
                        result.Add(assignment.Node);
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

        /// <summary>
        /// Returns a bottom up list of all descentants of the specified segment
        /// </summary>
        private IEnumerable<Node> NodeDescendants(Segment segment)
        {
            foreach (var childSegment in segment.Children)
            {
                foreach (var grandchild in NodeDescendants(childSegment))
                    yield return grandchild;
                foreach (var assignment in childSegment.AssignedNodes)
                    yield return assignment.Node;
            }
        }

        #endregion

        #region Segment graph components

        private class NodeSegmentAssignment
        {
            public Node Node;
            public Segment Segment;
            public IList<IList<Node>> DependentNodes;
        }

        private class Segment : IEquatable<Segment>
        {
            public string Name;
            public IList<string> ChildSegmentNames;
            public IList<Segment> Children;
            public Segment Parent;
            public IList<NodeSegmentAssignment> AssignedNodes;

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
