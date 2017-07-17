using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Interfaces.Utility;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Utility;

namespace OwinFramework.Routing
{
    /// <summary>
    /// A router consists of a set of filter expressions and a segment (list of middleware) 
    /// to execute when that filter evaluates to true. When routing and processing
    /// requests the router will evaluate filters until one matches, then only
    /// execute that one segment.
    /// </summary>
    public class Router : IRouter, ITraceable
    {
        IList<IDependency> IMiddleware.Dependencies { get { return _dependencies; } }
        IList<IRoutingSegment> IRouter.Segments { get { return _segments; } }

        /// <summary>
        ///  Impelemnts IRouter
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Implements ITraceable
        /// </summary>
        public Action<IOwinContext, Func<string>> Trace { get; set; }

        private readonly IList<IDependency> _dependencies;
        private readonly IList<IRoutingSegment> _segments;
        private readonly string _owinContextKey;
        private readonly IDependencyGraphFactory _dependencyGraphFactory;

        /// <summary>
        /// Constructs a new router
        /// </summary>
        /// <param name="dependencyGraphFactory"></param>
        public Router(IDependencyGraphFactory dependencyGraphFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;

            _owinContextKey = "R:" + Guid.NewGuid().ToShortString(false);
            _dependencies = new List<IDependency>();
            _segments = new List<IRoutingSegment>();
        }

        IRouter IRouter.Add(string routeName, Func<IOwinContext, bool> filterExpression)
        {
            _segments.Add(new RoutingSegment(_dependencyGraphFactory).Initialize(this, routeName, filterExpression));
            return this;
        }

        Task IRoutingProcessor.RouteRequest(IOwinContext context, Func<Task> next)
        {
            var name = string.IsNullOrEmpty(Name) ? GetType().Name : Name;
            foreach (var segment in _segments)
            {
                var seg = segment;
                if (segment.Filter(context))
                {
                    Trace(context, () => "The '" + name + "' router '" + seg.Name + "' filter matches the request");
                    context.Set(_owinContextKey, segment);
                    return segment.RouteRequest(context, next) ?? next();
                }
                Trace(context, () => "The '" + name + "' router '" + seg.Name + "' filter does not match the request");
            }
            return next();
        }

        Task IMiddleware.Invoke(IOwinContext context, Func<Task> next)
        {
            var segment = context.Get<IRoutingSegment>(_owinContextKey);

            if (segment == null)
                return next();

            return segment.Invoke(context, next);
        }

        /// <summary>
        /// This is used to encapsulate information about the middleware components
        /// during the process of resolving dependencies
        /// </summary>
        private class Component
        {
            public IMiddleware Middleware;
            public Type MiddlewareType;
            public string Name;
            public IList<IDependency> Dependencies;
        }

        /// <summary>
        /// The routing segment represents an ordered list of middleware components
        /// that are chained together and executed in a pipeline. The Router will 
        /// use logic to select which RoutingSegment to execute for a given request.
        /// </summary>
        private class RoutingSegment : IRoutingSegment
        {
            public string Name { get; private set; }
            public Func<IOwinContext, bool> Filter { get; private set; }
            public IList<IMiddleware> Middleware { get; private set; }

            private readonly IList<Component> _components;
            private readonly IDependencyGraphFactory _dependencyGraphFactory;

            private IList<IRoutingProcessor> _routingProcessors;
            private ITraceable _traceable;

            public RoutingSegment(IDependencyGraphFactory dependencyGraphFactory)
            {
                _dependencyGraphFactory = dependencyGraphFactory;
                _components = new List<Component>();
            }

            public IRoutingSegment Initialize(ITraceable traceable, string name, Func<IOwinContext, bool> filter)
            {
                _traceable = traceable;
                Name = name;
                Filter = filter;
                return this;
            }

            public void Add(IMiddleware middleware, Type middlewareType)
            {
                if (middleware == null)
                    throw new BuilderException("Internal error, middleware can not be null");
                if (middlewareType == null)
                    throw new BuilderException("Internal error, middleware type can not be null");

                _components.Add(new Component
                {
                    Middleware = middleware,
                    MiddlewareType = middlewareType
                });
            }

            public void ResolveDependencies()
            {
                // Get information about the components - their properties might have
                // changed since they were registered
                foreach (var component in _components)
                {
                    component.Name = component.Middleware.Name;
                    component.Dependencies = component.Middleware.Dependencies
                        .Where(d => d.DependentType != typeof(IRoute))
                        .ToList();
                }

                // When there are multiple middleware components implementing the same
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
                    if (multiples.Contains(t))
                        key += ":" + (string.IsNullOrEmpty(n) ? Guid.NewGuid().ToString("N") : n.ToLower());
                    return key;
                };

                // Build a dependency graph
                var dependencyGraph = _dependencyGraphFactory.Create<Component>();
                foreach (var component in _components)
                {
                    var key = buildKey(component.MiddlewareType, component.Name);

                    var position = PipelinePosition.Middle;
                    if (component.Dependencies.Any(dep => dep.Position == PipelinePosition.Back))
                        position = PipelinePosition.Back;
                    if (component.Dependencies.Any(dep => dep.Position == PipelinePosition.Front))
                        position = PipelinePosition.Front;

                    var dependentKeys = component
                        .Dependencies
                        .Where(dep => dep.DependentType != null)
                        .Select(c => 
                            new DependencyGraphEdge 
                            { 
                                Key = buildKey(c.DependentType, c.Name),
                                Required = c.Required
                            });

                    dependencyGraph.Add(key, component, dependentKeys, position);
                }

                // Sort components by order of least to most dependent
                IEnumerable<Component> orderedComponents;
                try
                {
                    orderedComponents = dependencyGraph.GetBuildOrderData();
                }
                catch (Exception ex)
                {
                    throw new RoutingException("There is a problem with the dependencies between your OWIN middleware components", ex);
                }

                Middleware = orderedComponents
                    .Select(c => c.Middleware)
                    .ToList();

                // Make a list of the middleware that wants to participate in routing
                // so that we don't figure this out again for each request
                _routingProcessors = Middleware
                    .Select(middleware => middleware as IRoutingProcessor)
                    .Where(rp => rp != null)
                    .ToList();
            }

            public Task RouteRequest(IOwinContext context, Func<Task> next)
            {
                if (_routingProcessors == null)
                    throw new RoutingException("Requests can not be routed until dependencies have been resolved");

                _traceable.Trace(context, () => "Routing request in '" + Name + "' routing segment");

                var nextIndex = 0;
                Func<Task> getNext = null;

                getNext = () =>
                {
                    if (nextIndex < _routingProcessors.Count)
                    {
                        var routingProcessor = _routingProcessors[nextIndex++];
                        _traceable.Trace(context, () =>
                        {
                            var middleware = routingProcessor as IMiddleware;
                            if (middleware == null || string.IsNullOrEmpty(middleware.Name))
                                return "Routing request to " + routingProcessor.GetType().FullName;
                            return "Routing request to middleware '" + middleware.Name + "'";
                        });
                        return routingProcessor.RouteRequest(context, getNext) ?? next();
                    }
                    return next();
                };

                return getNext();
            }

            public Task Invoke(IOwinContext context, Func<Task> next)
            {
                if (Middleware == null)
                    throw new RoutingException("Requests can not be processed until dependencies have been resolved");

                _traceable.Trace(context, () => "Processing request in '" + Name + "' routing segment");
                
                var nextIndex = 0;
                Func<Task> getNext = null;

                getNext = () =>
                    {
                        if (nextIndex < Middleware.Count)
                        {
                            var middleware = Middleware[nextIndex++];
                            _traceable.Trace(context, () => 
                                "Processing request with '" + 
                                (string.IsNullOrEmpty(middleware.Name) ? middleware.GetType().FullName : middleware.Name) + 
                                "' middleware");
                            return middleware.Invoke(context, getNext);
                        }
                        return next();
                    };

                return getNext();
            }
        }
    }
}
