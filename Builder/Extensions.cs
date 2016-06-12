using System;
using System.Linq;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;

namespace OwinFramework.Builder
{
    public static class Extensions
    {
        public static IMiddleware As(this IMiddleware middleware, string name)
        {
            middleware.Name = name;
            return middleware;
        }

        public static IMiddleware RunAfter<T>(this IMiddleware middleware, string name = null, bool required = true)
        {
            var existingDependency = middleware.Dependencies.FirstOrDefault(dep => dep.DependentType == typeof(T));
            if (existingDependency != null)
                middleware.Dependencies.Remove(existingDependency);

            middleware.Dependencies.Add(new Dependency<T>
            {
                Position = PipelinePosition.Middle,
                DependentType = typeof (T),
                Name = name,
                Required = required
            });
            return middleware;
        }

        public static IMiddleware RunFirst(this IMiddleware middleware)
        {
            middleware.Dependencies.Add(new Dependency<object>
            {
                Position = PipelinePosition.Front
            });
            return middleware;
        }

        public static IMiddleware RunLast(this IMiddleware middleware)
        {
            middleware.Dependencies.Add(new Dependency<object>
            {
                Position = PipelinePosition.Back
            });
            return middleware;
        }

        private class Dependency<T> : IDependency<T>
        {
            public PipelinePosition Position { get; set; }
            public Type DependentType { get; set; }
            public string Name { get; set; }
            public bool Required { get; set; }
        }

        public static IMiddleware ConfigureWith(
            this IMiddleware middleware, 
            IConfiguration configuration,
            string configurationPath)
        {
            var configurable = middleware as IConfigurable;
            if (configurable != null)
                configurable.Configure(configuration, configurationPath);
            return middleware;
        }

        public static IMiddleware<IRoute> AddRoute(
            this IMiddleware<IRoute> middleware,
            string routeName,
            Func<IOwinContext, bool> filterExpression)
        {
            var router = middleware as IRouter;
            if (router == null)
                throw new BuilderException("You can only add routes to a router");

            router.Add(routeName, filterExpression);

            return router;
        }

        public static IAppBuilder UseBuilder(this IAppBuilder appBuilder, IBuilder builder)
        {
            builder.Build(appBuilder);
            return appBuilder;
        }

        public static T GetFeature<T>(this IOwinContext owinContext) where T : class
        {
            return owinContext.Get<T>(typeof(T).Name);
        }

        public static void SetFeature<T>(this IOwinContext owinContext, T feature) where T : class
        {
            owinContext.Set(typeof(T).Name, feature);
        }

    }
}
