using System;
using System.Collections.Generic;
using System.Linq;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class Segmenter: ISegmenter
    {
        private Dictionary<string, Segment> _segments;
        private Dictionary<string, Node> _nodes;
        private bool _modified;

        public Segmenter()
        {
            Clear();
        }

        public void Clear()
        {
            _segments = new Dictionary<string, Segment>(StringComparer.InvariantCultureIgnoreCase);
            _nodes = new Dictionary<string, Node>(StringComparer.InvariantCultureIgnoreCase);
            _modified = true;
        }

        public void AddNode(string key, IEnumerable<IList<string>> dependencies, IEnumerable<string> segments)
        {
            if (_nodes.ContainsKey(key))
                throw new DuplicateKeyException("Node with key '" + key+"' already added to segmenter");

            var node = new Node
            {
                Key = key,
                NodeDependencies = dependencies == null ? new List<IList<string>>() : dependencies.ToList(),
                RequiredSegments = segments == null ? new List<string>() : segments.ToList()
            };

            _nodes[key] = node;

            _modified = true;
        }

        public void AddSegment(string name, IEnumerable<string> childSegments)
        {
            if (_segments.ContainsKey(name))
                throw new DuplicateKeyException("Segment with name '" + name + "' already added to segmenter");

            _segments[name] = new Segment
            {
                Name = name,
                ChildSegmentNames = childSegments == null ? new List<string>() : childSegments.ToList()
            };

            _modified = true;
        }

        public IList<string> GetSegmentNodes(string segmentName)
        {
            Recalculate();
            return _segments[segmentName].Nodes.Select(n => n.Key).ToList();
        }

        public IList<string> GetNodeSegments(string nodeKey)
        {
            Recalculate();
            return _nodes[nodeKey].AssignedSegments.Select(s => s.Name).ToList();
        }

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
                    .NodeDependencies
                    .Select(nl => (IList<Node>)nl.Select(n => _nodes[n]).ToList())
                    .ToList();
                node.AssignedSegments = new List<Segment>();
            }
        }

        private void AssignRequiredSegments()
        {
            foreach (var node in _nodes.Values)
            {
                foreach (var segment in node.RequiredSegments)
                    Assign(node, _segments[segment]);
            }
        }

        private void ResolveMultiChoiceDependencies()
        {
            foreach (var node in _nodes.Values)
            {
                foreach (var nodeList in node.DependentNodes)
                    if (nodeList.Count > 1)
                        ResolveMultiChoiceDependency(node, nodeList);
            }
        }

        private void ResolveMultiChoiceDependency(Node node, IList<Node> choices)
        {
            var segments = _segments.Values
                .Where(s => s.Nodes.Contains(node))
                .Where(s => s.Nodes.Any(choices.Contains))
                .ToList();
            if (segments.Count == 0)
            {
                foreach (var segment in  _segments.Values
                        .Where(s => s.Nodes.Contains(node))
                        .Where(s => s.Nodes.Any(choices.Contains)))
                    Assign(node, segment);
            }
            else
            {
                foreach (var segment in segments)
                    Assign(node, segment);
            }
        }

        private void Assign(Node node, Segment segment)
        {
            if (!node.AssignedSegments.Contains(segment))
            {
                AddAssignment(node, segment);
                foreach (var dependantList in node.DependentNodes)
                {
                    if (dependantList.Count == 1)
                        Assign(dependantList[0], segment);
                }
            }
        }

        private void AddAssignment(Node node, Segment segment)
        {
            node.AssignedSegments.Add(segment);
            segment.Nodes.Add(node);
        }

        private void RemoveAssignment(Node node, Segment segment)
        {
            segment.Nodes.Remove(node);
            node.AssignedSegments.Remove(segment);
        }

        private void ConsolidateCommonNodes()
        {
            foreach (var segment in _segments.Values.Where(s => s.Parent == null))
                ConsolidateCommonNodes(segment);
        }

        private void ConsolidateCommonNodes(Segment segment)
        {
            foreach (var child in segment.Children)
                ConsolidateCommonNodes(child);

            if (segment.Children.Count < 2) return;

            var commonNodes = segment.Children[0].Nodes.ToList();

            for (var i = 1; i < segment.Children.Count; i++)
            {
                foreach(var node in commonNodes.ToList())
                {
                    if (!segment.Children[i].Nodes.Contains(node))
                    {
                        commonNodes.Remove(node);
                        break;
                    }
                }
            }

            foreach (var node in commonNodes)
                Consolidate(segment, node);
        }

        private void Consolidate(Segment segment, Node node)
        {
            foreach (var child in segment.Children)
            {
                if (child.Nodes.Contains(node))
                    RemoveAssignment(node, child);
            }
            AddAssignment(node, segment);
        }

        private class Segment
        {
            public string Name;
            public IList<string> ChildSegmentNames;
            public IList<Segment> Children;
            public Segment Parent;
            public IList<Node> Nodes;
        }

        private class Node
        {
            public string Key;
            public IList<IList<string>> NodeDependencies;
            public IList<string> RequiredSegments;
            public IList<Segment> AssignedSegments;
            public IList<IList<Node>> DependentNodes;
        }
    }
}
