using System;
using System.Collections.Generic;

namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyGraphEdge: IEquatable<IDependencyGraphEdge>
    {
        string Key { get; }
        bool Required { get; }
    }
}
