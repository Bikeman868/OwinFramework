using Owin;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// Defines the component that is responsible for examining the
    /// dependencied between OWIN middleware compoennts and building
    /// an OWIN chain that will work.
    /// </summary>
    public interface IBuilder : IRequestTracer
    {
        /// <summary>
        /// Adds a middleware component to the list of middleware to build into
        /// the OWIN ippeline
        /// </summary>
        /// <typeparam name="T">The type of middleware or 'object' if this is generic middleware</typeparam>
        /// <param name="middleware">The middleware instance to include in the OWIN pipeline</param>
        /// <returns>The middleware for fluid syntax</returns>
        IMiddleware<T> Register<T>(IMiddleware<T> middleware);

        /// <summary>
        /// Figures out middleware dependencies and route assigmnents and builds
        /// an OWIN pipeline
        /// </summary>
        void Build(IAppBuilder app);
    }
}
