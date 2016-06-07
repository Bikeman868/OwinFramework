using System;
using System.Collections.Generic;
using System.Linq;
using Owin;
using OwinFramework.Interfaces;

namespace OwinFramework.Builder
{
    public class Builder: IBuilder
    {
        private readonly IList<Component> _components;
        private readonly IDependencyTreeFactory _dependencyTreeFactory;

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
            // Get information about the components - their properties might have
            // changed since they were registered with the builder.
            foreach (var component in _components)
            {
                component.Name = component.Middleware.Name;
                component.Dependencies = component.Middleware.Dependencies;
            }

            // When there are multiple middleware compoennts implementing the same
            // interface then dependencies must use the fully qualifies reference.
            // When there is only one instance the names should be ignored to 
            // simplify configuration
            var singeltons = new List<Type>();
            var multiples = new List<Type>();
            foreach (var component in _components)
            {
                if (!multiples.Contains(component.MiddlewareType))
                {
                    if (singeltons.Contains(component.MiddlewareType))
                    {
                        multiples.Add(component.MiddlewareType);
                        singeltons.Remove(component.MiddlewareType);
                    }
                    else
                        singeltons.Add(component.MiddlewareType);
                }
            }

            // These keys are used to pass dependencies to the dependency graph 
            Func<Type, string, string> buildKey = (t, n) =>
            {
                var key = t.FullName;
                if (!string.IsNullOrEmpty(n) && !singeltons.Contains(t))
                    key += ":" + n.ToLower();
                return key;
            };

            // Build a dependency graph
            var dependencyTree = _dependencyTreeFactory.Create<string, Component>();
            foreach (var component in _components)
            {
                var key = buildKey(component.MiddlewareType, component.Name);
                var dependentKeys = component.Dependencies == null
                    ? null
                    : component
                        .Dependencies
                        .Select(c => buildKey(c.DependentType, c.Name));

                dependencyTree.Add(key, component, dependentKeys);
            }

            // Sort components by order of least to most dependent
            var orderedComponents  = dependencyTree.GetAllData();

            // Build the OWIN pipeline
            foreach (var component in orderedComponents)
                app.Use(component.Middleware.Invoke);
        }

        private class Component
        {
            public IMiddleware Middleware;
            public Type MiddlewareType;
            public string Name;
            public IList<IDependency> Dependencies;
        }
    }
}
