using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Builder
{
    /// <summary>
    /// This class provides a wrapper around legacy middleware that was not designed to
    /// work with the Owin Framework.
    /// </summary>
    public class LegacyMiddlewareWrapper : IMiddleware<object>, IAppBuilder
    {
        string IMiddleware.Name { get; set; }
        IList<IDependency> IMiddleware.Dependencies { get { return _dependencies; } }

        private readonly IList<IDependency> _dependencies = new List<IDependency>();
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private Func<IOwinContext, Func<Task>, Task> _wrappedMiddleware;

        /// <summary>
        /// Constructs a wrapper around legacy middleware that does not implement IMiddleware
        /// </summary>
        public LegacyMiddlewareWrapper()
        {
            _wrappedMiddleware = (context, next) => next();
        }

        Task IMiddleware.Invoke(IOwinContext context, Func<Task> next)
        {
            return _wrappedMiddleware(context, next);
        }

        object IAppBuilder.Build(Type returnType)
        {
            // See https://msdn.microsoft.com/en-us/library/microsoft.owin.builder.appbuilder.build(v=vs.113).aspx#M:Microsoft.Owin.Builder.AppBuilder.Build(System.Type)
            throw new NotImplementedException();
        }

        IAppBuilder IAppBuilder.New()
        {
            return this;
        }

        IDictionary<string, object> IAppBuilder.Properties
        {
            get { return _properties; }
        }

        IAppBuilder IAppBuilder.Use(object middleware, params object[] args)
        {
            // See https://msdn.microsoft.com/en-us/library/microsoft.owin.builder.appbuilder.use(v=vs.113).aspx#M:Microsoft.Owin.Builder.AppBuilder.Use(System.Object,System.Object[])

            if (middleware == null)
                throw new BuilderException("LegacyMiddlewareWrapper.Use called with null pointer for the middleware to add");

            _wrappedMiddleware = middleware as Func<IOwinContext, Func<Task>, Task>;

            if (_wrappedMiddleware == null && middleware is Type)
                _wrappedMiddleware = GetMiddlewareFromType((Type)middleware, args);
            
            if (_wrappedMiddleware == null)
                throw new BuilderException("LegacyMiddlewareWrapper.Use called with an unsupported method of middleware invocation");

            return this;
        }

        private Func<IOwinContext, Func<Task>, Task> GetMiddlewareFromType(Type middlewareType, object[] args)
        {
            var constructors = middlewareType.GetConstructors()
                .Where(c =>
                {
                    var parameters = c.GetParameters();
                    if (parameters == null || parameters.Length == 0) return false;
                    return parameters[0].ParameterType == typeof (Func<IDictionary<string, object>, Task>);
                })
                .ToList();

            if (constructors.Count != 1)
                throw new BuilderException(
                    "LegacyMiddlewareWrapper.Use called with a Type which does not have a single constructor "+
                    "taking Func<Dictionary<string, object>, Task> as its first argument");

            var constructorArgs = args == null ? new object[1] : new object[args.Length + 1];
            if (args != null && args.Length > 0)
                args.CopyTo(constructorArgs, 1);

            var invokeMethod = middlewareType.GetMethods()
                .FirstOrDefault(m =>
                {
                    if (m.Name != "Invoke") return false;
                    if (m.ReturnType != typeof (Task)) return false;
                    var invokeParams = m.GetParameters();
                    if (invokeParams == null || invokeParams.Length != 1) return false;
                    return invokeParams[0].ParameterType == typeof (IDictionary<string, object>);
                });

            if (invokeMethod == null)
                throw new BuilderException(
                    "LegacyMiddlewareWrapper.Use called with a Type which does not have a public Invoke method "+
                    "taking an OWIN environment dictionary and returning a Task");

            return (context, next) =>
            {
                Func<IDictionary<string, object>, Task> arg0 = d => next();
                constructorArgs[0] = arg0;
                var middleware = constructors[0].Invoke(constructorArgs);

                _wrappedMiddleware = (c, n) =>
                {
                    var invokeArgs = new object[] { c.Environment };
                    return invokeMethod.Invoke(middleware, invokeArgs) as Task;
                };

                return _wrappedMiddleware(context, next);
            };
        }
    }
}
