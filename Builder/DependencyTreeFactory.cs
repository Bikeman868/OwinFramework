using System;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Builder
{
    public class DependencyTreeFactory : IDependencyTreeFactory
    {
        public IDependencyTree<TKey, TValue> Create<TKey, TValue>() where TKey: IEquatable<TKey>
        {
            return new DependencyTree<TKey, TValue>();
        }
    }

}
