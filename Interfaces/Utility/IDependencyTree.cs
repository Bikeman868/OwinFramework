using System;
using System.Collections.Generic;

namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyTree<T>
    {
        void Add(string key, T data, IEnumerable<ITreeDependency> dependencies);
        IEnumerable<string> GetDecendents(string key, bool topDown = false);
        T GetData(string key);
        IEnumerable<string> GetAllKeys(bool topDown = false);
        IEnumerable<T> GetAllData(bool topDown = false);
    }
}
