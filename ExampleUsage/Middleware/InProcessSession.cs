using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.InterfacesV1.Upstream;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This class demonstrates a middleware component that supports upstream communication.
    /// 
    /// This class injects the ISession feature into the OWIN context during request processing.
    /// 
    /// This class injects IUpstreamSession feature into the OWIN context during the routing
    /// phose. This object allows downstream middleware to indicate whether session is required 
    /// for the request.
    /// 
    /// In this example the session creation is so cheap that there is no point in having an on/off
    /// switch for it. This example exists to illustrate how to do it even though in this case it
    /// is not needed. If you are writing session middleware that persists session to a database
    /// it would be a really good idea to avoid loading up session for requests that don't need it.
    /// </summary>
    public class InProcessSession : IMiddleware<ISession>, IUpstreamCommunicator<IUpstreamSession>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        private readonly IDictionary<string, DownstreamSession> _sessions;

        public InProcessSession()
        {
            _sessions = new Dictionary<string, DownstreamSession>();
            Dependencies = new List<IDependency>();
        }

        /// <summary>
        /// This method gets called during the routing phase. It gives the middleware component
        /// an opportunity to inject something into the OWIN context that downstream middleware
        /// can use to configure it's behaviour for this request.
        /// </summary>
        public void RouteRequest(IOwinContext context, Action next)
        {
            Console.WriteLine("ROUTE: In process session");

            context.SetFeature<IUpstreamSession>(new UpstreamSession());

            // Invoke the next middleware in the chain
            next();
        }

        /// <summary>
        /// This method gets called when the request is processed by the OWIN pipeline.
        /// In this case it injects session into the OWIN context for downstream
        /// middleware components to use.
        /// </summary>
        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("PROCESS: In process session");

            var upstreamSession = context.GetFeature<IUpstreamSession>() as UpstreamSession;
            var sessionRequired = upstreamSession != null && upstreamSession.SessionRequired;

            DownstreamSession session = null;

            if (sessionRequired)
            {
                var identification = context.GetFeature<IIdentification>();
                if (identification != null && !identification.IsAnonymous)
                {
                    lock(_sessions)
                    {
                        if (!_sessions.TryGetValue(identification.Identity, out session))
                        {
                            session = new DownstreamSession(true);
                            _sessions.Add(identification.Identity, session);
                        }
                    }
                }
            }
            
            if (session == null)
                session = new DownstreamSession(false);
        
            context.SetFeature<ISession>(session);

            // Invoke the next middleware in the chain
            return next.Invoke();
        }

        /// <summary>
        /// An instance of this class is inserted into the OWIN context during the
        /// request routing phase. This allows downstream middleware to indicate
        /// whether session is required or not for this request.
        /// </summary>
        private class UpstreamSession : IUpstreamSession
        {
            public bool SessionRequired;

            public bool EstablishSession()
            {
                SessionRequired = true;
                return true;
            }
        }

        /// <summary>
        /// This is a very basic implementation of session that is only usefull for
        /// illustration purposes. Do not use this code in your production web site.
        /// 
        /// An object of this type is created and added to the OWIN context during
        /// request processing. It can be used further down the OWIN pipeline to
        /// retrieve and update user specific information that is persisted between
        /// requests.
        /// </summary>
        private class DownstreamSession: ISession
        {
            private readonly IDictionary<string, object> _sessionVariables;

            public bool HasSession { get { return _sessionVariables != null; } }

            public DownstreamSession(bool hasSession)
            {
                if (hasSession)
                    _sessionVariables = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            }

            public T Get<T>(string name)
            {
                if (HasSession)
                {
                    object value;
                    if (_sessionVariables.TryGetValue(name, out value))
                        return (T)value;
                }
                return default(T);
            }

            public void Set<T>(string name, T value)
            {
                if (HasSession)
                {
                    _sessionVariables[name] = value;
                }
            }

            public object this[string name]
            {
                get { return Get<object>(name); }
                set { Set(name, value); }
            }
        }

    }
}
