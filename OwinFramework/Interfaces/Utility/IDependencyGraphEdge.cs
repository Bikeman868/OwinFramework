using System;

namespace OwinFramework.Interfaces.Utility
{
    /// <summary>
    /// Represents an edge in the dependency graph
    /// </summary>
    public interface IDependencyGraphEdge: IEquatable<IDependencyGraphEdge>
    {
        /// <summary>
        /// The unique key of the node that this edge connects to
        /// </summary>
        string Key { get; }

        /// <summary>
        /// True if this dependency is mandatory, i.e. the 'from' end of the 
        /// edge can not function without the 'to' end of the edge being present.
        /// False if this dependency is optional. In this case the 'to' node
        /// does not have to exits, but if it does exist it must be before in
        /// the build order.
        /// </summary>
        bool Required { get; }
    }
}
