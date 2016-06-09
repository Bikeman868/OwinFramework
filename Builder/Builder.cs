using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;
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
            _router = new Router(_dependencyTreeFactory);
            _router.Add(null, owinContext => true);

            foreach (var component in _components)
                _router.Segments[0].Add(component.Middleware, component.MiddlewareType);
            _router.Segments[0].ResolveDependencies();

            app.Use(Invoke);
        }

        private Task Invoke(IOwinContext context, Func<Task> next)
        {
            _router.RouteRequest(context, () => { });
            return _router.Invoke(context, next);
        }

        private class Component
        {
            public IMiddleware Middleware;
            public Type MiddlewareType;
        }
    }
}
