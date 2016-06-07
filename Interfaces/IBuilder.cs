using Owin;

namespace OwinFramework.Interfaces
{
    /// <summary>
    /// Defines the component that is responsible for examining the
    /// dependencied between OWIN middleware compoennts and building
    /// an OWIN chain that will work.
    /// </summary>
    public interface IBuilder
    {
        IMiddleware<T> Register<T>(IMiddleware<T> middleware);
        void Build(IAppBuilder app);
    }
}
