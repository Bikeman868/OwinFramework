using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Interfaces.Utility;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.Routing;
using OwinFramework.Utility;

namespace OwinFramework.Builder
{
    /// <summary>
    /// This is the class that builds an OWIN pipeline with routing and
    /// dependencies between middleware
    /// </summary>
    public class Builder: IBuilder, ITraceable
    {
        private readonly IList<Component> _components;
        private readonly IDependencyGraphFactory _dependencyGraphFactory;
        private readonly ISegmenterFactory _segmenterFactory;

        private IRouter _router;

        /// <summary>
        /// Implements ITraceable
        /// </summary>
        public Action<IOwinContext, Func<string>> Trace { get; set; }

        /// <summary>
        ///  Defines how captured traces will be output
        /// </summary>
        public Action<IOwinContext, string> TraceOutput { get; set; }

        private RequestsToTrace _requestsToTrace;

        /// <summary>
        /// Constructs a new OWIN pipeline builder
        /// </summary>
        public Builder(
            IDependencyGraphFactory dependencyGraphFactory,
            ISegmenterFactory segmenterFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;
            _segmenterFactory = segmenterFactory;
            _components = new List<Component>();
            Trace = (c, f) => { };
            TraceOutput = (c, t) => System.Diagnostics.Trace.WriteLine(t);
        }

        /// <summary>
        /// Implements IBuilder
        /// </summary>
        public IBuilder EnableTracing(RequestsToTrace requestsToTrace = RequestsToTrace.All)
        {
            _requestsToTrace = requestsToTrace;
            if (requestsToTrace == RequestsToTrace.None)
            {
                Trace = (c, f) => { };
            }
            else
            {
                Trace = (c, f) =>
                {
                    if (f == null) return;

                    if (_requestsToTrace == RequestsToTrace.QueryString)
                    {
                        if (c.Request.Query["trace"] == null) return;
                    }

                    string message;
                    try
                    {
                        message = f();
                    }
                    catch (Exception ex)
                    {
                        message = "Exception thrown in trace function: " + Environment.NewLine + ex.StackTrace;
                    }
                    if (string.IsNullOrEmpty(message)) return;

                    var t = c.Get<TraceContext>("fw.builder.trace");
                    if (t == null)
                    {
                        t = new TraceContext();
                        c.Set("fw.builder.trace", t);
                    }

                    t.Append(message);
                };
            }

            foreach (var component in _components)
            {
                var traceable = component.Middleware as InterfacesV1.Capability.ITraceable;
                if (traceable != null) traceable.Trace = Trace;
            }
            return this;
        }

        IMiddleware<T> IBuilder.Register<T>(IMiddleware<T> middleware)
        {
            var traceable = middleware as InterfacesV1.Capability.ITraceable;
            if (traceable != null) traceable.Trace = Trace;

            var component = typeof(T) == typeof(IRoute) 
                ? (Component)(new RouterComponent())
                : (Component)(new MiddlewareComponent());

            component.Middleware = middleware;
            component.MiddlewareType = typeof (T);
            _components.Add(component);

            return middleware;
        }

        void IBuilder.Build(IAppBuilder app)
        {
            // Ensure components have unique names
            var componentNames = new SortedList(new CaseInsensitiveComparer());
            foreach (var component in _components)
            {
                component.Name = string.IsNullOrEmpty(component.Middleware.Name) 
                    ? Guid.NewGuid().ToShortString(false) 
                    : component.Middleware.Name;
                if (componentNames.ContainsKey(component.Name))
                    throw new BuilderException("Middleware component names must be unique."
                        + " There is more than one component called '" + component.Name
                        + "'");
                componentNames.Add(component.Name, null);
            }

            // Resolve name only dependencies and fill in the type that they depend on
            foreach (var component in _components)
            {
                foreach (var dependency in component.Middleware.Dependencies)
                {
                    var dependencyName = dependency.Name;
                    if (dependency.DependentType == null && !string.IsNullOrEmpty(dependencyName))
                    {
                        var dependent = _components.FirstOrDefault(
                            c => string.Equals(c.Name, dependencyName, StringComparison.OrdinalIgnoreCase));
                        if (dependent == null)
                        {
                            if (dependency.Required)
                                throw new MissingDependencyException("There are no middleware components called \"" + dependencyName + "\"");
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
            _router = routeBuilder.BuildRoutes(this, routerComponents);

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

#if DEBUG
                Action<MiddlewareComponent> p = c =>
                {
                    var l = "   " + c.Name;
                    l += " IMiddleware<" + c.MiddlewareType.Name + ">";
                    l += " {";
                    var sep = "";
                    foreach (var d in c.Middleware.Dependencies)
                    {
                        l += sep + (d.Required ? "" : "(optional)") + (d.DependentType == null ? "" : d.DependentType.Name) + "[" + d.Name + "]";
                        sep = ", ";
                    }
                    l += "}";
                    l += " {";
                    sep = "";
                    foreach (var s in c.SegmentAssignments)
                    {
                        l += sep + s.Name;
                        sep = ", ";
                    }
                    l += "}";
                    System.Diagnostics.Trace.WriteLine(l);
                };
                System.Diagnostics.Trace.WriteLine("== Before segmentation ==");
                System.Diagnostics.Trace.WriteLine("Components at the front of the pipeline");
                foreach (var c in frontComponents) p(c);
                System.Diagnostics.Trace.WriteLine("Components in the middle of the pipeline");
                foreach (var c in middleComponents) p(c);
                System.Diagnostics.Trace.WriteLine("Components at the back of the pipeline");
                foreach (var c in backComponents) p(c);
#endif

                AddToFront(routerComponents, frontComponents);
                AddToMiddle(routerComponents, middleComponents);
                AddToBack(routerComponents, backComponents);

#if DEBUG
                System.Diagnostics.Trace.WriteLine("== After segmentation ==");
                System.Diagnostics.Trace.WriteLine("Components at the front of the pipeline");
                foreach (var c in frontComponents) p(c);
                System.Diagnostics.Trace.WriteLine("Components in the middle of the pipeline");
                foreach (var c in middleComponents) p(c);
                System.Diagnostics.Trace.WriteLine("Components at the back of the pipeline");
                foreach (var c in backComponents) p(c);
#endif
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

#if DEBUG
            Dump(_router, "");
#endif
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
            // Roll up all the segments from all routers
            var allSegments = routerComponents.SelectMany(rc => rc.RouterSegments).ToList();

            // Make sure all the segments have names since this is how they are linked in the segmenter
            foreach (var segment in allSegments)
                if (string.IsNullOrEmpty(segment.Name))
                    segment.Name = new Guid().ToShortString();

            // The segmenter will assign nodes to segments in a routing graph such that
            // all nodes appear after their dependents when the graph is traversed, and
            // each node is as close to the front of the graph as possible.
            var segmenter = _segmenterFactory.Create();

            foreach (var routerComponent in routerComponents)
                foreach(var parent in routerComponent.ParentRouteNames)
                    segmenter.AddSegment(parent, routerComponent.ChildRouteNames);  

            foreach (var component in components)
                AddToSegmenter(segmenter, components, component);

            foreach (var component in components)
            {
                component.SegmentAssignments = segmenter
                    .GetNodeSegments(component.Name)
                    .Select(sn => allSegments.First(s => s.Name == sn))
                    .ToList();
                if (component.SegmentAssignments.Count == 0)
                    throw new BuilderException(
                        "Middleware '" + component.Name + "' of type " +
                        component.Middleware.GetType().Name + "<" + component.MiddlewareType.Name + ">" +
                        " will not be added to the Owin pipeline because it is not assigned to any route," +
                        " and there are no other middleware that depend on it that are assigned to a route." +
                        " You must either assign this middleware to a route, or assign middleware that depends on" +
                        " it to a route. You can also configure this middleware to run at the start or the " +
                        " end of the pipeline to fix this problem.");
            }
        }

        private void AddToSegmenter(ISegmenter segmenter, IList<MiddlewareComponent> components, MiddlewareComponent middlewareComponent)
        {
            IList<List<string>> nodeDependencies = new List<List<string>>();
            IList<string> routeDependencies = new List<string>();
            foreach (var dependency in middlewareComponent.Middleware.Dependencies)
            {
                if (string.IsNullOrEmpty(dependency.Name))
                {
                    if (dependency.DependentType == typeof (IRoute))
                        throw new BuilderException(
                            "Route dependencies must specify the name of the route."
                            + " Middleware type " + middlewareComponent.Middleware.GetType().Name
                            + "<" + middlewareComponent.MiddlewareType.Name 
                            + "> has a dependency on an unnamed route.");
                    var dependantNames = components
                        .Where(c => c.MiddlewareType == dependency.DependentType)
                        .Select(c => c.Name)
                        .ToList();
                    if (dependantNames.Count > 0)
                    {
                        if (!dependency.Required)
                            dependantNames.Add(null);
                        nodeDependencies.Add(dependantNames);
                    }
                    else
                    {
                        if (dependency.Required)
                            throw new BuilderException(
                                "Missing middleware dependency. Middleware type " 
                                + middlewareComponent.Middleware.GetType().Name
                                + "<" + middlewareComponent.MiddlewareType.Name
                                + "> has a dependency on " 
                                + dependency.DependentType.Name
                                + " but there is no IMiddleware<" + dependency.DependentType.Name 
                                + "> configured.");
                    }
                }
                else
                { 
                    if (dependency.DependentType == typeof(IRoute))
                    {
                        routeDependencies.Add(dependency.Name);
                    }
                    else
                    {
                        var dependantNames = new List<string> { dependency.Name };
                        if (!dependency.Required)
                            dependantNames.Add(null);
                        nodeDependencies.Add(dependantNames);
                    }
                }
            }
            segmenter.AddNode(middlewareComponent.Name, nodeDependencies, routeDependencies);
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
            Trace(context, () => "Request " + context.Request.Uri);

            var task = ExecutePipeline(context, next);

            if (_requestsToTrace != RequestsToTrace.None)
            {
                return task.ContinueWith(t =>
                {
                    var traceContext = context.Get<TraceContext>("fw.builder.trace");
                    if (traceContext != null)
                        TraceOutput(context, traceContext.TraceOutput.ToString());
                });
            }
            return task;
        }

        private Task ExecutePipeline(IOwinContext context, Func<Task> next)
        {
            context.Set<IRouter>("OwinFramework.Router", _router);
            return _router.RouteRequest(context, () => _router.Invoke(context, next));
        }

#if DEBUG

        private void Dump(IRouter router, string indent)
        {
            Debug.WriteLine(indent + "Router \"" + (router.Name ?? "<anonymous>") + "\"");
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
                    Debug.WriteLine(indent + line);
                }
            }
            else 
            {
                var line = "depends on " + dependency.DependentType.Name;
                if (dependency.Name != null) line += " \"" + dependency.Name + "\"";
                if (!dependency.Required) line += (" (optional)");
                Debug.WriteLine(indent + line);
            }

            if (dependency.Position == PipelinePosition.Front)
                Debug.WriteLine(indent + "runs before other middleware");

            if (dependency.Position == PipelinePosition.Back)
                Debug.WriteLine(indent + "runs after other middleware");
        }

        private void Dump(IRoutingSegment segment, string indent)
        {
            Debug.WriteLine(indent + "has route \"" + (segment.Name ?? "<anonymous>") + "\"");

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

            Debug.WriteLine(indent + "Middleware " + middleware.GetType().Name + " \"" + (middleware.Name ?? "<anonymous>") + "\"");
            indent += "  ";

            foreach (var dependency in middleware.Dependencies) Dump(dependency, indent);
        }

#endif

        private class TraceContext
        {
            private static long _nextRequestId;

            public readonly StringBuilder TraceOutput;
            private readonly long _requestId;

            public TraceContext()
            {
                TraceOutput = new StringBuilder();
                _requestId = Interlocked.Increment(ref _nextRequestId);
            }

            public void Append(string message)
            {
                TraceOutput.AppendFormat("#{0:d6} {1:T} {2}{3}", _requestId, DateTime.Now, message, Environment.NewLine);
            }
        }

        private class Component
        {
            public string Name;
            public IMiddleware Middleware;
            public Type MiddlewareType;
            public List<Segment> SegmentAssignments = new List<Segment>();
        }

        private class MiddlewareComponent : Component
        {
        }

        private class RouterComponent : Component
        {
            public List<string> ParentRouteNames = new List<string>();
            public List<string> ChildRouteNames = new List<string>();
            public List<Segment> RouterSegments = new List<Segment>();
        }

        private class Segment
        {
            public string Name;
            public IRoutingSegment RoutingSegment;
            public readonly List<Component> Components = new List<Component>();
        }

        private class RouteBuilder
        {
            private readonly IDependencyGraphFactory _dependencyGraphFactory;

            public RouteBuilder(IDependencyGraphFactory dependencyGraphFactory)
            {
                _dependencyGraphFactory = dependencyGraphFactory;
            }

            public IRouter BuildRoutes(ITraceable traceable, IList<RouterComponent> routerComponents)
            {
                // Create a root level router as a container for everythinng that's not on a route. 
                // When the application does not use routing everything ends up in here
                IRouter rootRouter = new Router(_dependencyGraphFactory);
                ((ITraceable) rootRouter).Trace = traceable.Trace;

                // Add a root segment called with a pass everything filter
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

                // For each router component figure out it's parents and children in the routing graph
                var defaultParents = new List<string> { rootRoutingSegment.Name };
                foreach (var routerComponent in routerComponents)
                {
                    var router = (IRouter)routerComponent.Middleware;

                    routerComponent.ChildRouteNames = router.Segments
                        .Select(s => s.Name)
                        .ToList();

                    routerComponent.ParentRouteNames = router.Dependencies
                        .Where(d => d.DependentType == typeof(IRoute) && !string.IsNullOrEmpty(d.Name))
                        .Select(s => s.Name)
                        .ToList();

                    if (routerComponent.ParentRouteNames.Count == 0 && !ReferenceEquals(routerComponent, rootRouterComponent))
                        routerComponent.ParentRouteNames = defaultParents;
                }

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
