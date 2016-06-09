using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Routing;
using System.Threading.Tasks;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Builder
{
    public class Builder: IBuilder
    {
        private readonly IList<Component> _components;
        private readonly IDependencyTreeFactory _dependencyTreeFactory;

        private Router _router;

        public Builder(IDependencyTreeFactory dependencyTreeFactory)
        {
            _dependencyTreeFactory = dependencyTreeFactory;
            _components = new List<Component>();
        }

        public IMiddleware<T> Register<T>(IMiddleware<T> middleware)
        {
            _components.Add(new Component
            {
                Middleware = middleware,
                MiddlewareType = typeof(T)
            });
            return middleware;
        }

        public void Build(IAppBuilder app)
        {
            var routers = _components
                .Select(c => c.Middleware as IRouter)
                .Where(r => r != null)
                .ToList();

            // This root level router is a container for evertthinng that's not on a route. When
            // the application does not use routing everything ends up in here
            _router = new Router(_dependencyTreeFactory);
            _router.Add(null, owinContext => true);
            routers.Add(_router);

            foreach (var component in _components)
            {
                // TODO: put components in the right segments
                _router.Segments[0].Add(component.Middleware, component.MiddlewareType);
            }

            foreach (var router in routers)
                foreach (var segment in router.Segments)
                    segment.ResolveDependencies();

            Dump(_router, "");

            app.Use(Invoke);
        }

        private Task Invoke(IOwinContext context, Func<Task> next)
        {
            _router.RouteRequest(context, () => { });
            return _router.Invoke(context, next);
        }

#region Diagnostic dump

        private void Dump(IRouter router, string indent)
        {
            System.Diagnostics.Debug.WriteLine(indent + "Router \"" + (router.Name ?? "<anonymous>") + "\"");
            indent += "  ";

            foreach (var dependency in router.Dependencies) Dump(dependency, indent);
            foreach (var segment in router.Segments) Dump(segment, indent);
        }

        private void Dump(IDependency dependency, string indent)
        {
            var line = "depends on " + dependency.DependentType.Name;
            if (dependency.Name != null) line += " \"" + dependency.Name + "\"";
            if (!dependency.Required) line += (" (optional)");
            System.Diagnostics.Debug.WriteLine(indent + line);
        }

        private void Dump(IRoutingSegment segment, string indent)
        {
            System.Diagnostics.Debug.WriteLine(indent + "has route \"" + (segment.Name ?? "<anonymous>") + "\"");

            indent += "  ";
            foreach (var middleware in segment.Middleware)
                Dump(middleware, indent);
        }

        private void Dump(IMiddleware middleware, string indent)
        {
            var router = middleware as IRouter;
            if (router != null)
            {
                Dump(router, indent);
                return;
            }

            System.Diagnostics.Debug.WriteLine(indent + "Middleware " + middleware.GetType().Name + " \"" + (middleware.Name ?? "<anonymous>") + "\"");
            indent += "  ";

            foreach (var dependency in middleware.Dependencies) Dump(dependency, indent);
        }

#endregion

        private class Component
        {
            public IMiddleware Middleware;
            public Type MiddlewareType;
        }
    }
}
