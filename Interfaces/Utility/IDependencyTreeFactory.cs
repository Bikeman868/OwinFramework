using System;

namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyTreeFactory
    {
        IDependencyTree<T> Create<T>();
    }
}
