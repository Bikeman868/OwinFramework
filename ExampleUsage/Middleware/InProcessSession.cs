using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Upstream;

namespace ExampleUsage.Middleware
{
    /// <summary>
    /// This class demonstrates a middleware component that supports upstream communication.
    /// 
    /// Injects the ISession feature into the OWIN context.
    /// 
    /// Also injects IUpstreamSession feature allowing downstream middleware to indicate whether 
    /// session is required for the request.
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

            // This middleware depends on knowing the identity of the caller. This feature
            // is provided by the IIdentification middleware
            this.RunAfter<IIdentification>();
        }

        /// <summary>
        /// This method gets called during the routing phase. It gives the middleware component
        /// an opportunity to inject something into the OWIN context that downstream middleware
        /// can use to configure it's behaviour specifically for this request.
        /// </summary>
        public void InvokeUpstream(IOwinContext context)
        {
            Console.WriteLine("In process session middleware upstream invoked");

            context.SetFeature<IUpstreamSession>(new UpstreamSession(context));
        }

        /// <summary>
        /// This method gets called when the request is processed by the OWIN pipeline.
        /// In this case it injects session into the OWIN context for downstream
        /// middleware components to use.
        /// </summary>
        public Task Invoke(IOwinContext context, Func<Task> next)
        {
            Console.WriteLine("In process session middleware invoked");

            var upstreamSession = context.GetFeature<IUpstreamSession>();
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

            return next.Invoke();
        }

        /// <summary>
        /// This class illustrates one technique for downstream components to configure an upstream
        /// component by storing settings in the OWIN context's environment.
        /// </summary>
        private class UpstreamSession : IUpstreamSession
        {
            private readonly IOwinContext _context;

            public UpstreamSession(IOwinContext context)
            {
                _context = context;
            }

            public bool SessionRequired
            {
                get
                {
                    var value = _context.Environment["InProcessSessionRequired"];
                    return value == null ? false : (bool)value;
                }
                set
                {
                    if (value) _context.Environment["InProcessSessionRequired"] = true;
                }
            }
        }

        /// <summary>
        /// This is a very basic implementation of session that is only usefull for
        /// illustration purposes. Do not use this code in your production web site.
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
