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

            // Resolve dependencies
            var dependencyTree = _dependencyTreeFactory.Create<string, Component>();
            foreach (var component in _components)
            {
                var key = component.MiddlewareType.FullName;
                if (!string.IsNullOrEmpty(component.Name))
                    key += ":" + component.Name.ToLower();

                var dependentKeys = component.Dependencies == null 
                    ? null
                    : component
                        .Dependencies
                        .Select(c => 
                            {
                                var dependentKey = c.DependentType.FullName;
                                if (!string.IsNullOrEmpty(c.Name))
                                    dependentKey += ":" + c.Name;
                                return dependentKey;
                            });

                dependencyTree.Add(key, component, dependentKeys);
            }

            // Sort components by order of registration
            var orderedComponents  = dependencyTree.GetAllData();

            // Build OWIN chain
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
