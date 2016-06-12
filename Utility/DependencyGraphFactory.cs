using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class DependencyGraphFactory : IDependencyGraphFactory
    {
        public IDependencyGraph<T> Create<T>() 
        {
            return new DependencyGraph<T>();
        }
    }

}
