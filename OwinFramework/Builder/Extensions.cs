using System;
using System.Linq;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.InterfacesV1.Capability;

namespace OwinFramework.Builder
{
    /// <summary>
    /// Extension methods that provide a fluid syntax for configuring middleware in the OWIN pipeline builder
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Specifies a unique name for the middleware so that other middleware can depend on it.
        /// </summary>
        public static IMiddleware As(this IMiddleware middleware, string name)
        {
            middleware.Name = name;
            return middleware;
        }

        /// <summary>
        /// Specifies a dependency on another middleware of a specific type
        /// </summary>
        /// <typeparam name="T">The type of middleware that this middleware depends on</typeparam>
        /// <param name="middleware">The middleware that has a dependency</param>
        /// <param name="name">Optional name in case there are multiple middleware of the dependent type</param>
        /// <param name="required">True if this middleware can not function without the dependant middleware</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
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

        /// <summary>
        /// Specifies a dependency on another middleware with the specified name
        /// </summary>
        /// <param name="middleware">The middleware that has a dependency</param>
        /// <param name="name">The name of the other middleware that this one depends on</param>
        /// <param name="required">True if this middleware can not function without the dependant middleware</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
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

        /// <summary>
        /// Specifies that this middleware must run on a specific route
        /// </summary>
        /// <param name="middleware">The middleware to configure</param>
        /// <param name="routeName">The name of the route it must run on</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
        public static IMiddleware RunOnRoute(this IMiddleware middleware, string routeName)
        {
            return RunAfter<IRoute>(middleware, routeName);
        }

        /// <summary>
        /// Specifies that this middleware needs to handle every request and
        /// therefore runs before any routing takes place.
        /// </summary>
        /// <param name="middleware">The middleware to configure</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
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

        /// <summary>
        /// Specifies that this middleware should run after all other middleware
        /// has chosen not to handle the request
        /// </summary>
        /// <param name="middleware">The middleware to configure</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
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

        /// <summary>
        /// Specifies how the middleware should obtain its configuration
        /// </summary>
        /// <param name="middleware">The middleware to configure</param>
        /// <param name="configuration">The applications provider of configuration data</param>
        /// <param name="configurationPath">A path in the configuration file where
        ///  the configuration should be read from</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
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

        /// <summary>
        /// Adds a route to a router
        /// </summary>
        /// <param name="middleware">The router to add a route to</param>
        /// <param name="routeName">The name of the route to add to this router</param>
        /// <param name="filterExpression">An expression that will be used at runtime
        /// to decide if the incomming request should be processed by the middleware
        /// on this route. Routes are evaluated in the order they are added. It
        /// is often a good idea to have a catch all route as the last route
        /// configured</param>
        /// <returns>The middleware to facilitate fluid syntax</returns>
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

        /// <summary>
        /// Standard OWIN syntax for adding middleware. In this case it adds the OWIN
        /// pipeline builder to the OWIN pipeline
        /// </summary>
        public static IAppBuilder UseBuilder(this IAppBuilder appBuilder, IBuilder builder)
        {
            builder.Build(appBuilder);
            return appBuilder;
        }

        /// <summary>
        /// This is used by middleware to get features that are implemented by other
        /// middleware components that already executed against this OWIN context. For
        /// example if the session middleware already executed then other middleware
        /// can get the session that was added to the OWIN context using this extension 
        /// method
        /// </summary>
        /// <typeparam name="T">The interface type of the feature to get. For example ISession</typeparam>
        /// <param name="owinContext">The context of this OWIN request</param>
        /// <returns>The feature if it exists or null if there is no feature of this type in context</returns>
        public static T GetFeature<T>(this IOwinContext owinContext) where T : class
        {
            return owinContext.Get<T>(typeof(T).Name);
        }

        /// <summary>
        /// Stores a feature implementation in the OWIN context for retrieval by other
        /// middleware further down the pipeline.
        /// </summary>
        /// <typeparam name="T">The type of feature to store. This must be an interface, for example ISession</typeparam>
        /// <param name="owinContext">The context of this OWIN request</param>
        /// <param name="feature">The feature to make available to other middleware</param>
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

        /// <summary>
        /// Converts a 64-bit unsigned value to a string that is shorter than simply
        /// calling the ToString() method and is valid for inclusion in a URL
        /// </summary>
        /// <param name="value">The value to convert to short text</param>
        /// <param name="mixedCase">True to use both upper and lower case letters, false 
        /// to use lower case letters only</param>
        /// <returns>A short string representing this value</returns>
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

        /// <summary>
        /// Converts a GUID to a string that is shorter than simply
        /// calling the ToString() method and is valid for inclusion in a URL
        /// </summary>
        /// <param name="guid">The GUID to convert to short text</param>
        /// <param name="mixedCase">True to use both upper and lower case letters, false 
        /// to use lower case letters only</param>
        /// <returns>A short string representing this GUID</returns>
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
