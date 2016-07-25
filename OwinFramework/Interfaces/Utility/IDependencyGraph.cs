using System.Collections.Generic;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Interfaces.Utility
{
    /// <summary>
    /// Implements algorthims for constructing dependency graphs and resolving dependencies
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDependencyGraph<T>
    {
        /// <summary>
        /// Adds a node to the dependency graph
        /// </summary>
        /// <param name="key">A unique key for this node in the graph</param>
        /// <param name="data">Application data associated with this node</param>
        /// <param name="edges">A list of other nodes in the graph that are directly connected to this one</param>
        /// <param name="position">The position within the OWIN pipeline for this middleware</param>
        void Add(string key, T data, IEnumerable<IDependencyGraphEdge> edges, PipelinePosition position);

        /// <summary>
        /// Returns the data associated with a node in the graph
        /// </summary>
        /// <param name="key">The unique key provided when the graph node was added</param>
        T GetData(string key);

        /// <summary>
        /// Returns the unique IDs of the nodes beneath a specific node in the dependnecy tree.
        /// </summary>
        /// <param name="key">The unique ID of the node whose decendants are wanted</param>
        /// <param name="topDown">True to traverse the tree top-down and false to traverse bottom-up</param>
        IEnumerable<string> GetDecendents(string key, bool topDown = false);

        /// <summary>
        /// Returns all nodes in the graph ordered such that all nodes appear in the list after all of the
        /// nodes that they depend on.
        /// </summary>
        /// <param name="topDown">True to traverse the tree top-down and false to traverse bottom-up</param>
        /// <returns>Unique IDs for nodes in the graph in buid order</returns>
        IEnumerable<string> GetBuildOrderKeys(bool topDown = false);

        /// <summary>
        /// This is a convenience method that wraps a call to GetBuildOrderKeys, calling the GetData
        /// method for each key.
        /// </summary>
        /// <param name="topDown">True to traverse the tree top-down and false to traverse bottom-up</param>
        IEnumerable<T> GetBuildOrderData(bool topDown = false);
    }
}
