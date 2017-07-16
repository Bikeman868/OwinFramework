﻿using Owin;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// Defines the component that is responsible for examining the
    /// dependencied between OWIN middleware compoennts and building
    /// an OWIN chain that will work.
    /// </summary>
    public interface IBuilder
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

        /// <summary>
        /// Turns tracing on. This should only be used in a development environment.
        /// If your production environment is very low volume you could also choose
        /// to leave it on in production but this is not the use case it was designed 
        /// for.
        /// </summary>
        IBuilder EnableTracing(RequestsToTrace traceOption = RequestsToTrace.All);
    }
}
