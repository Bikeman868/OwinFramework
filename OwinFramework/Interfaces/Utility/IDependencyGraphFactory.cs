namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyGraphFactory
    {
        IDependencyGraph<T> Create<T>();
    }
}
