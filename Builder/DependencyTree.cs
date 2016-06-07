using System;
using System.Collections.Generic;
using System.Linq;

namespace OwinFramework.Builder
{
    public class DependencyTree<TKey, TData> : IDependencyTree<TKey, TData> where TKey: IEquatable<TKey>
    {
        private readonly IDictionary<TKey, GraphNode> _nodeIndex;
        private bool _graphBuilt;

        public DependencyTree()
        {
            _nodeIndex = new Dictionary<TKey, GraphNode>();
        }

        public void Add(TKey key, TData data, IEnumerable<TKey> dependentKeys)
        {
            GraphNode treeNode;
            if (_nodeIndex.TryGetValue(key, out treeNode))
            {
                treeNode.Data = data;
                if (dependentKeys != null)
                {
                    foreach (var dependant in dependentKeys)
                    {
                        if (!treeNode.DependentKeys.Contains(dependant))
                            treeNode.DependentKeys.Add(dependant);
                    }
                }
            }
            else
            {
                treeNode = new GraphNode
                   {
                       Data = data,
                       Key = key,
                       DependentKeys = dependentKeys == null ? new List<TKey>() : dependentKeys.ToList(),
                   };
                _nodeIndex.Add(key, treeNode);
            }
            _graphBuilt = false;
        }

        public IEnumerable<TKey> GetDecendents(TKey key, bool topDown)
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

        public TData GetData(TKey key)
        {
            return _nodeIndex[key].Data;
        }

        public IEnumerable<TData> GetAllData(bool topDown)
        {
            BuildGraph();

            var sortedNodes = GetSortedList();

            if (topDown) sortedNodes = sortedNodes.Reverse().ToList();

            return sortedNodes.Select(n => n.Data);
        }

        public IEnumerable<TKey> GetAllKeys(bool topDown)
        {
            BuildGraph();

            var sortedNodes = GetSortedList();

            if (topDown) sortedNodes = sortedNodes.Reverse().ToList();

            return sortedNodes.Select(n => n.Key);
        }

        /// <summary>
        /// Implements depth first topological sort
        /// </summary>
        /// <see cref="https://en.wikipedia.org/wiki/Topological_sorting"/>
        private IList<GraphNode> GetSortedList()
        {
            var nodes = _nodeIndex.Values.ToList();

            foreach (var node in nodes)
                node.VisitStatus = VisitStatus.Unvisited;

            var sorted = new List<GraphNode>();

            if (nodes.Count > 1)
            {
                var unvisitedNode = nodes[0];
                while (unvisitedNode != null)
                {
                    Visit(sorted, unvisitedNode);
                    unvisitedNode = nodes.FirstOrDefault(n => n.VisitStatus == VisitStatus.Unvisited);
                }
            }

            return sorted;
        }

        private void Visit(ICollection<GraphNode> sortedList, GraphNode node)
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
                {
                    node.VisitStatus = VisitStatus.MarkTemporary;
                    foreach (var m in node.OutgoingEdges)
                        Visit(sortedList, m);
                    node.VisitStatus = VisitStatus.MarkPermenant;
                    sortedList.Add(node);
                    break;
                }
            }
        }

        private void BuildGraph()
        {
            if (_graphBuilt) return;

            var nextId = 1;
            foreach (var node in _nodeIndex.Values)
            {
                node.Id = nextId++;
                node.IncommingEdges = new List<GraphNode>();
            }

            foreach (var node in _nodeIndex.Values)
            {
                node.OutgoingEdges = node.DependentKeys.Select(k =>
                {
                    GraphNode dependent;
                    if (!_nodeIndex.TryGetValue(k, out dependent))
                        throw new Exception("Dependent node '" + k + "' does not exist in the dependecy tree");
                    return dependent;
                }).ToList();
                foreach (var edge in node.OutgoingEdges)
                    edge.IncommingEdges.Add(node);
            }

            _graphBuilt = true;
        }

        private enum VisitStatus { Unvisited, MarkTemporary, MarkPermenant }

        private class GraphNode
        {
            public int Id;
            public TData Data;
            public TKey Key;
            public IList<TKey> DependentKeys;
            public IList<GraphNode> OutgoingEdges;
            public IList<GraphNode> IncommingEdges;
            public VisitStatus VisitStatus;
        }
    }
}
