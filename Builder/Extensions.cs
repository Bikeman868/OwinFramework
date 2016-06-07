using System;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces;

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
            middleware.Dependencies.Add(new Dependency<T>
            {
                DependentType = typeof(T),
                Name = name,
                Required = required
            });
            return middleware;
        }

        private class Dependency<T> : IDependency<T>
        {
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
