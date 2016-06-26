using System;

namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyGraphEdge: IEquatable<IDependencyGraphEdge>
    {
        string Key { get; }
        bool Required { get; }
    }
}
