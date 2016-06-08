using System;
using System.Collections.Generic;

namespace OwinFramework.Interfaces.Builder
{
    public interface IDependencyTree<TKey, TData> where TKey: IEquatable<TKey>
    {
        void Add(TKey key, TData data, IEnumerable<TKey> dependentKeys);
        IEnumerable<TKey> GetDecendents(TKey key, bool topDown = false);
        TData GetData(TKey key);
        IEnumerable<TKey> GetAllKeys(bool topDown = false);
        IEnumerable<TData> GetAllData(bool topDown = false);
    }
}
