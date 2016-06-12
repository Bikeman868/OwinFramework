using System;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class DependencyGraphEdge: IDependencyGraphEdge
    {
        public string Key { get; set; }
        public bool Required { get; set; }

        public bool Equals(IDependencyGraphEdge other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }
    }
}
