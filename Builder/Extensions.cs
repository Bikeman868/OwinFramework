using System;
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
    }
}
