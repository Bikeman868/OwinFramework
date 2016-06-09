using System;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class TreeDependency: ITreeDependency
    {
        public string Key { get; set; }
        public bool Required { get; set; }

        public bool Equals(ITreeDependency other)
        {
            if (ReferenceEquals(other, null)) return false;
            return string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);
        }
    }
}
