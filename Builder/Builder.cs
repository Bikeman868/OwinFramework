﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Routing;
using System.Threading.Tasks;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Utility;

namespace OwinFramework.Builder
{
    public class Builder: IBuilder
    {
        private readonly IList<Component> _components;
        private readonly IDependencyGraphFactory _dependencyGraphFactory;

        private Router _router;

        public Builder(IDependencyGraphFactory dependencyGraphFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;
            _components = new List<Component>();
        }

        public IMiddleware<T> Register<T>(IMiddleware<T> middleware)
        {
            var component = typeof(T) == typeof(IRoute) 
                ? (Component)(new RouterComponent())
                : (Component)(new MiddlewareComponent());

            component.Middleware = middleware;
            component.MiddlewareType = typeof (T);
            _components.Add(component);

            return middleware;
        }

        public void Build(IAppBuilder app)
        {
            // Resolve name only dependencies and fill in the type that they depend on
            foreach (var component in _components)
            {
                foreach (var dependency in component.Middleware.Dependencies)
                {
                    if (dependency.DependentType == null && !string.IsNullOrEmpty(dependency.Name))
                    {
                        var dependent = _components.FirstOrDefault(
                            c => string.Equals(c.Middleware.Name, dependency.Name, StringComparison.OrdinalIgnoreCase));
                        if (dependent == null)
                        {
                            if (dependency.Required)
                                throw new MissingDependencyException("There are no middleware components called \"" + dependency.Name + "\"");
                        }
                        else
                        {
                            dependency.DependentType = dependent.MiddlewareType;
                        }
                    }
                }
            }

            var routerComponents = _components
                .Select(c => c as RouterComponent)
                .Where(rc => rc != null)
                .ToList();

            var middlewareComponents = _components
                .Select(c => c as MiddlewareComponent)
                .Where(mc => mc != null)
                .ToList();

            // This root level router is a container for everythinng that's not on a route. When
            // the application does not use routing everything ends up in here
            _router = new Router(_dependencyGraphFactory);
            _router.Add(null, owinContext => true);
            var rootRouterComponent = new RouterComponent
            {
                Middleware = _router,
                MiddlewareType = typeof(IRoute)
            };
            routerComponents.Add(rootRouterComponent);

            // Create Segment objects for each IRoutingSegment configured for the application
            foreach (var routerComponent in routerComponents)
            {
                var router = (IRouter)routerComponent.Middleware;
                routerComponent.RouterSegments = router.Segments
                    .Select(s => new Segment
                    {
                        Name = s.Name,
                        RoutingSegment = s
                    })
                    .ToList();
            }

            if (routerComponents.Count == 1)
            {
                var segment = routerComponents[0].RouterSegments[0];
                foreach (var component in middlewareComponents)
                    component.SegmentAssignments.Add(segment);
            }
            else
            {
                // Split the middleware components into three groups: front, middle and back. Note that routers
                // are always in the middle. Front means run before routing and back means run after routing
                var frontComponents = middlewareComponents
                    .Where(c => c.Middleware.Dependencies.Any(dep => dep.Position == PipelinePosition.Front))
                    .ToList();
                var backComponents = middlewareComponents
                    .Where(c => !frontComponents.Contains(c))
                    .Where(c => c.Middleware.Dependencies.Any(dep => dep.Position == PipelinePosition.Back))
                    .ToList();
                var middleComponents = middlewareComponents
                    .Where(c => !frontComponents.Contains(c))
                    .Where(c => !backComponents.Contains(c))
                    .ToList();

                AddToFront(rootRouterComponent, frontComponents);
                AddToMiddle(routerComponents, middleComponents);
                AddToBack(routerComponents, backComponents);
            }

            // Add components to the segments they were assigned to
            foreach (var component in _components)
            {
                foreach (var segment in component.SegmentAssignments)
                {
                    var routingSegment = segment.RoutingSegment;
                    routingSegment.Add(component.Middleware, component.MiddlewareType);
                }
            }

            // Order components within each segment according to their dependencies
            foreach (var routerComponent in routerComponents)
            {
                var router = (IRouter)routerComponent.Middleware;
                foreach (var segment in router.Segments)
                    segment.ResolveDependencies();
            }

            Dump(_router, "");

            app.Use(Invoke);
        }

        private void AddToFront(RouterComponent rootRouterComponent, IEnumerable<MiddlewareComponent> components)
        {
            var segments = new List<Segment> { rootRouterComponent.RouterSegments[0] };
            foreach (var component in components)
                component.SegmentAssignments = segments;
        }

        private void AddToMiddle(IEnumerable<RouterComponent> routers, IEnumerable<MiddlewareComponent> components)
        {
            // TODO: assign components to segments
        }

        private void AddToBack(IEnumerable<RouterComponent> routers, IEnumerable<MiddlewareComponent> components)
        {
            var allSegments = routers
                .Aggregate(
                    new List<Segment>(),
                    (s, r) =>
                    {
                        s.AddRange(r.RouterSegments);
                        return s;
                    })
                .ToList();

            var leafSegments = allSegments
                .Where(s => s.Components.All(c => c.GetType() != typeof(RouterComponent)))
                .ToList();

            foreach (var component in components)
            {
                var dependantRoutes = component.Middleware.Dependencies
                    .Where(dep => dep.DependentType == typeof(IRoute))
                    .ToList();
                if (dependantRoutes.Count == 0)
                {
                    // For components at the back with no explicit route dependencies
                    // add them to all segments that are leaves on the routing tree.
                    component.SegmentAssignments = leafSegments;
                }
                else
                {
                    // For components at the back with explicit route dependencies
                    // add them only to the routes they depend on
                    component.SegmentAssignments = dependantRoutes
                        .Select(dr => allSegments.FirstOrDefault(s => string.Equals(s.Name, dr.Name, StringComparison.OrdinalIgnoreCase)))
                        .Where(s => s != null)
                        .ToList();
                }
            }
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
            if (dependency.DependentType == null)
            {
                if (!string.IsNullOrEmpty(dependency.Name))
                {
                    var line = "depends on \"" + dependency.Name + "\"";
                    if (!dependency.Required) line += (" (optional)");
                    System.Diagnostics.Debug.WriteLine(indent + line);
                }
            }
            else 
            {
                var line = "depends on " + dependency.DependentType.Name;
                if (dependency.Name != null) line += " \"" + dependency.Name + "\"";
                if (!dependency.Required) line += (" (optional)");
                System.Diagnostics.Debug.WriteLine(indent + line);
            }

            if (dependency.Position == PipelinePosition.Front)
                System.Diagnostics.Debug.WriteLine(indent + "runs before other middleware");

            if (dependency.Position == PipelinePosition.Back)
                System.Diagnostics.Debug.WriteLine(indent + "runs after other middleware");
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
            public List<Component> PriorComponents = new List<Component>();
            public List<Segment> SegmentAssignments = new List<Segment>();
        }

        private class MiddlewareComponent : Component
        {
        }

        private class RouterComponent : Component
        {
            public List<Segment> RouterSegments = new List<Segment>();
        }

        private class Segment
        {
            public string Name;
            public IRoutingSegment RoutingSegment;
            public List<Component> Components = new List<Component>();
        }
    }
}
