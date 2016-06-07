using System;

namespace OwinFramework.Builder
{
    public interface IDependencyTreeFactory
    {
        IDependencyTree<TKey, TValue> Create<TKey, TValue>() where TKey: IEquatable<TKey>;
    }
}
