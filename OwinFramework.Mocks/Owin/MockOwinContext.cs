using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using Moq.Modules;

namespace OwinFramework.Mocks.Owin
{
    public class MockOwinContext : ConcreteImplementationProvider<IOwinContext>, IOwinContext
    {
        protected override IOwinContext GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

#if NET45
        private readonly Microsoft.Owin.Security.IAuthenticationManager _authenticationManager = new MockAuthenticationManager();
        public Microsoft.Owin.Security.IAuthenticationManager Authentication { get { return _authenticationManager; } }
#endif

        private IDictionary<string, object> _properties;
        private IDictionary<string, object> _environment;
        private MockOwinRequest _request;
        private MockOwinResponse _response;

        public IDictionary<string, object> Environment { get { return _environment; } }

        public StringBuilder TraceOutputStringBuilder { get; private set; }
        public MemoryStream ResponseStream { get; private set; }

        public MockOwinContext()
        {
            Clear();
        }

        public void Clear()
        {
            ResponseStream = new MemoryStream();

            _request = new MockOwinRequest(this);
            _response = new MockOwinResponse(this, ResponseStream);

            _properties = new Dictionary<string, object>();
            _environment = new Dictionary<string, object>();

            Set("server.RemoteIpAddress", "192.168.0.1");
            Set("server.RemotePort", "80");
            Set("server.LocalIpAddress", "127.0.0.1");
            Set("server.LocalPort", "80");
            Set("server.IsLocal", true);

            var addresses = new List<IDictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"scheme", "http"},
                    {"host", "unittest.com"},
                    {"port", "80"},
                    {"path", "/"}
                }
            };


            var capabilities = new Dictionary<string, object>
            {
            };

            TraceOutputStringBuilder = new StringBuilder();
            var traceOutput = new StringWriter(TraceOutputStringBuilder);

            Set<TextWriter>("host.TraceOutput", traceOutput);
            Set<IList<IDictionary<string, object>>>("host.Addresses", addresses);
            Set<IDictionary<string, object>>("server.Capabilities", capabilities);
        }

        public void SetMockequest(Uri url, Stream body)
        {
            var addresses = Get<IList<IDictionary<string, object>>>("host.Addresses");

            var queryIndex = url.PathAndQuery.IndexOf('?');
            var path = queryIndex < 0 ? url.PathAndQuery : url.PathAndQuery.Substring(0, queryIndex);
            var query = queryIndex < 0 ? string.Empty : url.PathAndQuery.Substring(queryIndex + 1);

            addresses.Clear();
            addresses.Add(
                new Dictionary<string, object>
                {
                    {"scheme", url.Scheme},
                    {"host", url.Host},
                    {"port", url.Port.ToString()},
                    {"path", path}
                });

            var queryParams = new Dictionary<string, string[]>();

            foreach (var p in query
                .Split('&')
                .Select(p =>
                    {
                        var i = p.IndexOf('=');
                        return new
                        {
                            n = p.Substring(0, i),
                            v = p.Substring(i + 1)
                        };
                    }))
            {
                string[] values;
                if (queryParams.TryGetValue(p.n, out values))
                    queryParams[p.n] = values.Concat(Enumerable.Repeat(p.v, 1)).ToArray();
                else
                    queryParams[p.n] = new[] {p.v};
            }

            _request.Uri = url;
            _request.Query = new ReadableStringCollection(queryParams);
            _request.QueryString = new QueryString(query);
            _request.Host = new HostString(url.Host);
            _request.IsSecure = string.Equals(url.Scheme, "https", StringComparison.OrdinalIgnoreCase);
            _request.Path = new PathString(path);
            _request.PathBase = new PathString(path);
            _request.Scheme = url.Scheme;
            _request.Body = body;
        }

        public T Get<T>(string key)
        {
            object value;
            if (_properties.TryGetValue(key, out value))
                return (T) value;
            return default(T);
        }

        public IOwinContext Set<T>(string key, T value)
        {
            _properties[key] = value;
            return this;
        }

        public IOwinRequest Request
        {
            get { return _request; }
        }

        public IOwinResponse Response
        {
            get { return _response; }
        }

        public TextWriter TraceOutput {
            get { return Get<TextWriter>("host.TraceOutput"); }
            set { Set("host.TraceOutput", value); }
        }
    }

}
