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
            if (typeof (T) == typeof (IRoute))
            {
                if (name == null)
                    throw new BuilderException("When adding a dependency on a route the name of the route must be specified");

                var frontDependency =
                    middleware.Dependencies.FirstOrDefault(dep => dep.Position == PipelinePosition.Front);
                if (frontDependency != null)
                    throw new BuilderException(
                        "It does not make sense to add this middleware to the '" 
                        + name + "' route when it is already configured to run before any routing.");
            }

            if (name == null)
            {
                var existingDependency = middleware.Dependencies.FirstOrDefault(dep => dep.DependentType == typeof (T));
                if (existingDependency != null)
                    middleware.Dependencies.Remove(existingDependency);
            }
            else
            {
                var existingDependency = middleware.Dependencies.FirstOrDefault(dep => string.Equals(dep.Name, name, StringComparison.OrdinalIgnoreCase));
                if (existingDependency != null)
                    middleware.Dependencies.Remove(existingDependency);
            }

            middleware.Dependencies.Add(new Dependency<T>
            {
                Position = PipelinePosition.Middle,
                DependentType = typeof (T),
                Name = name,
                Required = required
            });
            return middleware;
        }

        public static IMiddleware RunAfter(this IMiddleware middleware, string name, bool required = true)
        {
            if (string.IsNullOrEmpty(name))
                throw new BuilderException("When you add a middleware dependency you must either provide a name or a type or middleware that it depends on.");

            var existingDependency = middleware.Dependencies.FirstOrDefault(dep => string.Equals(dep.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existingDependency != null)
                middleware.Dependencies.Remove(existingDependency);

            middleware.Dependencies.Add(new Dependency
            {
                Position = PipelinePosition.Middle,
                DependentType = null,
                Name = name,
                Required = required
            });
            return middleware;
        }

        public static IMiddleware RunOnRoute(this IMiddleware middleware, string routeName)
        {
            return RunAfter<IRoute>(middleware, routeName);
        }

        public static IMiddleware RunFirst(this IMiddleware middleware)
        {
            var routeDependency = middleware.Dependencies.FirstOrDefault(dep => dep.DependentType == typeof(IRoute));
            if (routeDependency != null)
                throw new BuilderException("It does not make sense to configure this middleware to run before any routing when it is already configured to run on the '" + routeDependency.Name + "' route.");
                
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

        private class Dependency : IDependency
        {
            public PipelinePosition Position { get; set; }
            public Type DependentType { get; set; }
            public string Name { get; set; }
            public bool Required { get; set; }
        }

        private class Dependency<T> : Dependency, IDependency<T>
        {
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

        private static readonly char[] ShortStringMixedCaseChars = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h','i','j','k','l','m','n','o','p','q','r','s','t',
            'u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P',
            'Q','R','S','T','U','V','W','X','Y','Z','0','1','2','3','4','5','6','7','8','9'
        };

        private static readonly char[] ShortStringLowerCaseChars = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h','i','j','k','l','m','n','o','p','q','r','s','t',
            'u','v','w','x','y','z','0','1','2','3','4','5','6','7','8','9'
        };

        public static string ToShortString(this ulong value, bool mixedCase = true)
        {
            var chars = mixedCase ? ShortStringMixedCaseChars : ShortStringLowerCaseChars;
            if (value == 0) return chars[0] + "";

            var numberBase = (ulong)chars.Length;
            var result = "";
            while (value > 0)
            {
                var remainder = value % numberBase;
                value = value/numberBase;
                result = chars[remainder] + result;
            }
            return result;
        }

        public static string ToShortString(this Guid guid, bool mixedCase = true)
        {
            var bytes = guid.ToByteArray();
            var left = BitConverter.ToUInt64(bytes, 0);
            var right = BitConverter.ToUInt64(bytes, 8);
            var maxLength = mixedCase ? 11 : 13;
            return left.ToShortString(mixedCase).PadLeft(maxLength, 'a') 
                + right.ToShortString(mixedCase).PadLeft(maxLength, 'a');
        }

    }
}
