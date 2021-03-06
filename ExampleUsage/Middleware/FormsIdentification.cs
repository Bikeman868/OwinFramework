﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.InterfacesV1.Upstream;
using OwinFramework.MiddlewareHelpers.Identification;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This middleware example demonstrates the following techniques:
    /// * It injects the IIdentification feature into the OWIN context
    /// * It is configurable
    /// </summary>
    public class FormsIdentification: 
        IMiddleware<IIdentification>, 
        IConfigurable, 
        IRoutingProcessor,
        ITraceable
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public Action<IOwinContext, Func<string>> Trace { get; set; }

        public FormsIdentification()
        {
            Dependencies = new List<IDependency>();

            // This middleware depends on having session available. This feature
            // is provided by IMiddleware<ISession>
            this.RunAfter<ISession>();
        }

        /// <summary>
        /// Note that implementing IConfigurable is optional in your middleware
        /// </summary>
        void IConfigurable.Configure(IConfiguration configuration, string path)
        {
            var registration = configuration.Register(
                path,
                cfg => System.Diagnostics.Trace.WriteLine("Forms identification configuration changed for '" + Name + "' from " + path),
                string.Empty);
            registration.Dispose();
        }

        public Task RouteRequest(IOwinContext context, Func<Task> next)
        {
            // Get a reference to the session middleware
            var upstreamSession = context.GetFeature<IUpstreamSession>();
            if (upstreamSession == null)
                throw new Exception("The forms identification middleware needs a session to be available");

            // Tell the session middleware that a session must be established for this request
            // because forms identification can not work without it.
            upstreamSession.EstablishSession();

            // Execute the next step in routing the request
            return next();
        }

        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            // A real implementation would check the username and password and get a list
            // of known claims associated with this user
            context.SetFeature<IIdentification>(new Identification(
                Guid.NewGuid().ToString("N"),
                new[]{
                    new IdentityClaim ( ClaimNames.Username, "user1", ClaimStatus.Verified ),
                    new IdentityClaim ( ClaimNames.Email, "user1@gmail.com", ClaimStatus.Unverified )
                    }));

            return next();
        }
    }
}
