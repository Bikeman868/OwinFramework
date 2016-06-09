using System;
using System.Collections.Generic;

namespace OwinFramework.Interfaces.Utility
{
    public interface ITreeDependency: IEquatable<ITreeDependency>
    {
        string Key { get; }
        bool Required { get; }
    }
}
