using System;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class DependencyTreeFactory : IDependencyTreeFactory
    {
        public IDependencyTree<T> Create<T>() 
        {
            return new DependencyTree<T>();
        }
    }

}
