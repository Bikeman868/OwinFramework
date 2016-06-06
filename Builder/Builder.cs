using System;
using System.Collections.Generic;
using OwinFramework.Interfaces;

namespace OwinFramework.Builder
{
    public class Builder: IBuilder
    {
        private readonly IList<Component> _components;

        public Builder()
        {
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

        public void Build()
        {
            foreach (var component in _components)
            {
                component.Name = component.Middleware.Name;
                component.Dependencies = component.Middleware.Dependencies;
            }

            // Resolve dependencies

            // Build OWIN chain
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
