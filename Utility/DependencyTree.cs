using System;
using System.Collections.Generic;
using System.Linq;
using Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class DependencyTree<T> : IDependencyTree<T> 
    {
        private readonly IDictionary<string, GraphNode> _nodeIndex;
        private bool _graphBuilt;

        public DependencyTree()
        {
            _nodeIndex = new Dictionary<string, GraphNode>();
        }

        public void Add(string key, T data, IEnumerable<ITreeDependency> dependentKeys, PipelinePosition position)
        {
            GraphNode graphNode;
            if (_nodeIndex.TryGetValue(key, out graphNode))
                throw new DuplicateKeyException("The key " + key + " has already been added to this dependency graph");

            graphNode = new GraphNode
                {
                    Data = data,
                    Key = key,
                    DependentKeys = dependentKeys == null ? new List<ITreeDependency>() : dependentKeys.ToList(),
                    Position = position
                };
            _nodeIndex.Add(key, graphNode);

            _graphBuilt = false;
        }

        public IEnumerable<string> GetDecendents(string key, bool topDown)
        {
            BuildGraph();

            var node = _nodeIndex[key];
            if (topDown)
            {
                foreach (var edge in node.OutgoingEdges)
                    yield return edge.Key;
            }

            foreach (var edge in node.OutgoingEdges)
                foreach (var decendant in GetDecendents(edge.Key, topDown))
                    yield return decendant;
            
            if (!topDown)
            {
                foreach (var edge in node.OutgoingEdges)
                    yield return edge.Key;
            }
        }

        public T GetData(string key)
        {
            return _nodeIndex[key].Data;
        }

        public IEnumerable<T> GetBuildOrderData(bool topDown)
        {
            BuildGraph();

            var sortedNodes = GetSortedList();

            if (topDown) sortedNodes = sortedNodes.Reverse().ToList();

            return sortedNodes.Select(n => n.Data);
        }

        public IEnumerable<string> GetBuildOrderKeys(bool topDown)
        {
            BuildGraph();

            var sortedNodes = GetSortedList();

            if (topDown) sortedNodes = sortedNodes.Reverse().ToList();

            return sortedNodes.Select(n => n.Key);
        }

        /// <summary>
        /// Implements depth first topological sort. There is a small variation in this
        /// version because nodes can be defined as being fin first or last
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Topological_sorting"/>
        private IList<GraphNode> GetSortedList()
        {
            var nodes = _nodeIndex.Values.OrderBy(n => n.Position).ToList();

            foreach (var node in nodes)
                node.VisitStatus = VisitStatus.Unvisited;

            var sorted = new List<GraphNode>();

            if (nodes.Count > 1)
            {
                var unvisitedNode = nodes[0];
                while (unvisitedNode != null)
                {
                    Visit(sorted, unvisitedNode);
                    unvisitedNode = nodes.FirstOrDefault(n => n.VisitStatus == VisitStatus.Unvisited) ??
                                    nodes.FirstOrDefault(n => n.VisitStatus == VisitStatus.Deferred);
                }
            }

            return sorted;
        }

        private VisitStatus Visit(ICollection<GraphNode> sortedList, GraphNode node)
        {
            switch (node.VisitStatus)
            {
                case VisitStatus.MarkTemporary:
                {
                    var message = "There are circular dependencies.";
                    message += "\rThis problem was detected for  ";
                    message += node.Key + " which depends on " + string.Join(", ", node.DependentKeys);
                    message += " and has " + string.Join(", ", node.IncommingEdges.Select(e => e.Key));
                    message += " depending on it";
                    throw new CircularDependencyException(message);
                }
                case VisitStatus.Unvisited:
                case VisitStatus.Deferred:
                {
                    if (node.VisitStatus == VisitStatus.Unvisited 
                        && node.Position == PipelinePosition.Back)
                    {
                        node.VisitStatus = VisitStatus.Deferred;
                        break;
                    }

                    var finalStatus = VisitStatus.MarkPermenant;
                    node.VisitStatus = VisitStatus.MarkTemporary;
                    foreach (var m in node.OutgoingEdges)
                    {
                        if (Visit(sortedList, m) == VisitStatus.Deferred)
                            finalStatus = VisitStatus.Deferred;
                    }
                    node.VisitStatus = finalStatus;

                    if (finalStatus == VisitStatus.MarkPermenant)
                        sortedList.Add(node);
                    break;
                }
            }
            return node.VisitStatus;
        }

        private void BuildGraph()
        {
            if (_graphBuilt) return;

            foreach (var node in _nodeIndex.Values)
            {
                node.IncommingEdges = new List<GraphNode>();
            }

            foreach (var node in _nodeIndex.Values)
            {
                node.OutgoingEdges = node.DependentKeys.Select(
                    dep =>
                    {
                        GraphNode dependent;
                        if (!_nodeIndex.TryGetValue(dep.Key, out dependent))
                        {
                            if (dep.Required)
                                throw new MissingDependencyException("'" + node.Key + "' is dependent on missing '" + dep.Key + "'");
                        }
                        return dependent;
                    })
                .Where(dep => dep != null)
                .ToList();

                foreach (var edge in node.OutgoingEdges)
                    edge.IncommingEdges.Add(node);
            }

            _graphBuilt = true;
        }

        private enum VisitStatus { Unvisited, MarkTemporary, MarkPermenant, Deferred }

        private class GraphNode
        {
            public T Data;
            public string Key;
            public PipelinePosition Position;
            public IList<ITreeDependency> DependentKeys;
            public IList<GraphNode> OutgoingEdges;
            public IList<GraphNode> IncommingEdges;
            public VisitStatus VisitStatus;
        }

    }
}
