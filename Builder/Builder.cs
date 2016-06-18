using System;
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
        private readonly ISegmenterFactory _segmenterFactory;

        private Router _router;

        public Builder(
            IDependencyGraphFactory dependencyGraphFactory,
            ISegmenterFactory segmenterFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;
            _segmenterFactory = segmenterFactory;
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

            // Split components into routers and other types of middleware
            var routerComponents = _components
                .Select(c => c as RouterComponent)
                .Where(rc => rc != null)
                .ToList();
            var middlewareComponents = _components
                .Select(c => c as MiddlewareComponent)
                .Where(mc => mc != null)
                .ToList();

            var routeBuilder = new RouteBuilder(_dependencyGraphFactory);
            _router = routeBuilder.BuildRoutes(routerComponents);

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

                AddToFront(routerComponents, frontComponents);
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

        private void AddToFront(IEnumerable<RouterComponent> routerComponents, IEnumerable<MiddlewareComponent> components)
        {
            var rootRouterComponent = routerComponents.FirstOrDefault(rc => rc.SegmentAssignments.Count == 0);
            if (rootRouterComponent == null)
                throw new BuilderException("Internal error, there is no root router.");

            var segments = new List<Segment> { rootRouterComponent.RouterSegments[0] };
            foreach (var component in components)
            {
                component.SegmentAssignments = segments;
                foreach (var segment in segments)
                    segment.Components.Add(component);
            }
        }

        private void AddToMiddle(IList<RouterComponent> routerComponents, IList<MiddlewareComponent> components)
        {
            var segmenter = _segmenterFactory.Create();

            foreach (var routerComponent in routerComponents)
                AddToSegmenter(segmenter, routerComponent);

            foreach (var component in components)
                AddToSegmenter(segmenter, component);

            //foreach (var component in components)
            //{
            //    component.SegmentAssignments = segmenter
            //        .GetNodeSegments(component.UniqueId)
            //        .Select(nk => )
            //        .ToList());
            //}
        }

        private void AddToSegmenter(ISegmenter segmenter, RouterComponent routerComponent)
        {
            //segmenter.AddSegment(routerComponent.Middleware.Name ?? "", routerComponent.RouterSegments.Select(s => s.Name));
        }

        private void AddToSegmenter(ISegmenter segmenter, MiddlewareComponent middlewareComponent)
        {
        }

        private void AddToBack(IEnumerable<RouterComponent> routers, IEnumerable<MiddlewareComponent> components)
        {
            var allSegments = routers
                .SelectMany(r => r.RouterSegments)
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
                foreach (var segment in component.SegmentAssignments)
                    segment.Components.Add(component);
            }
        }

        private Task Invoke(IOwinContext context, Func<Task> next)
        {
            context.Set<IRouter>("OwinFramework.Router", _router);
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
            public string UniqueId = Guid.NewGuid().ToString();
            public IMiddleware Middleware;
            public Type MiddlewareType;
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

        private class RouteBuilder
        {
            private readonly IDependencyGraphFactory _dependencyGraphFactory;

            public RouteBuilder(IDependencyGraphFactory dependencyGraphFactory)
            {
                _dependencyGraphFactory = dependencyGraphFactory;
            }

            public Router BuildRoutes(IList<RouterComponent> routerComponents)
            {
                // Create a root level router as a container for everythinng that's not on a route. 
                // When the application does not use routing everything ends up in here
                var rootRouter = new Router(_dependencyGraphFactory);

                // Add one segment called "root" with a pass everything filter
                rootRouter.Add("root", owinContext => true);
                var rootRoutingSegment = rootRouter.Segments[0];

                // Wrap the root router in a component, this is equivalent to registering the router
                // with the builder. 
                var rootRouterComponent = new RouterComponent
                {
                    Middleware = rootRouter,
                    MiddlewareType = typeof(IRoute),
                };
                routerComponents.Add(rootRouterComponent);

                // Create Segment objects for each IRoutingSegment configured by the application
                // We will use the segmenter later to fill in the list of components on each segment
                foreach (var routerComponent in routerComponents)
                {
                    var router = (IRouter)routerComponent.Middleware;
                    routerComponent.RouterSegments = router
                        .Segments
                        .Select(s => new Segment
                        {
                            Name = s.Name,
                            RoutingSegment = s
                        })
                        .ToList();
                }
                var rootSegment = rootRouterComponent.RouterSegments[0];

                // Connect the routers together by assiging each one to its parents
                var allSegments = routerComponents.SelectMany(r => r.RouterSegments).ToList();
                foreach (var routerComponent in routerComponents)
                {
                    if (routerComponent == rootRouterComponent)
                        continue;

                    var router = (IRouter)routerComponent.Middleware;
                    var dependentRoutes = router
                        .Dependencies
                        .Where(dep => dep.DependentType == typeof (IRoute))
                        .ToList();
                    if (dependentRoutes.Count == 0)
                    {
                        rootSegment.Components.Add(routerComponent);
                        routerComponent.SegmentAssignments.Add(rootSegment);
                    }
                    else
                    {
                        foreach (var routeDependency in dependentRoutes)
                        {
                            var dependentSegment = allSegments.FirstOrDefault(
                                s => string.Equals(s.Name, routeDependency.Name, StringComparison.OrdinalIgnoreCase));
                            if (dependentSegment == null)
                            {
                                if (routeDependency.Required)
                                    throw new MissingDependencyException(
                                        "Route '" 
                                        + routerComponent.Middleware.Name 
                                        + "' depends on route '"
                                        + routeDependency.Name
                                        + "' which is not configured");
                            }
                            else
                            {
                                dependentSegment.Components.Add(routerComponent);
                                routerComponent.SegmentAssignments.Add(dependentSegment);
                            }
                        }
                    }
                }

                return rootRouter;
            }
        }
    }
}
