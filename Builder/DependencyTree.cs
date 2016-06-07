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
                foreach (var edge in node.Edges)
                    yield return edge.Key;
            }

            foreach (var edge in node.Edges)
                foreach (var decendant in GetDecendents(edge.Key, topDown))
                    yield return decendant;
            
            if (!topDown)
            {
                foreach (var edge in node.Edges)
                    yield return edge.Key;
            }
        }

        public TData GetData(TKey key)
        {
            return _nodeIndex[key].Data;
        }

        public IEnumerable<TKey> GetAllKeys(bool topDown)
        {
            BuildGraph();

            var resolved = new List<int>();
            var unresolved = new List<int>();

            return ResolveDependancies(_nodeIndex.Keys.First(), topDown, resolved, unresolved);
        }

        public IEnumerable<TData> GetAllData(bool topDown)
        {
            return GetAllKeys(topDown).Select(k => _nodeIndex[k].Data);
        }

        private IEnumerable<TKey> ResolveDependancies(TKey key, bool topDown, List<int> resolved, List<int> unresolved)
        {
            //unresolved.Add(key);
            //if (topDown)
            //{
            //    foreach (var edge in _edges
            //        .Where(e => e.From.Equals(key))
            //        .Where(e => resolved.Contains(e.To)))
            //        resolved.Add();
            //}

            //foreach (var edge in _edges.Where(e => e.From.Equals(key)))
            //    foreach (var decendant in GetDecendents(edge.To, topDown))
            //        yield return decendant;

            //if (!topDown)
            //{
            //    foreach (var edge in _edges.Where(e => e.From.Equals(key)))
            //        yield return edge.To;
            //}

            return null;
        }

        private void BuildGraph()
        {
            if (_graphBuilt) return;

            var nextId = 1;
            foreach (var node in _nodeIndex.Values)
                node.Id = nextId++;

            foreach (var node in _nodeIndex.Values)
                node.Edges = node.DependentKeys.Select(k => _nodeIndex[k]).ToList();

            _graphBuilt = true;
        }

        private class GraphNode
        {
            public int Id;
            public TData Data;
            public TKey Key;
            public IList<TKey> DependentKeys;
            public IList<GraphNode> Edges;
        }
    }
}
