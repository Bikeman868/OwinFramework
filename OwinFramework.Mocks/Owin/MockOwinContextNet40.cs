using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Moq.Modules;

namespace OwinFramework.Mocks.Owin
{
    public class MockOwinContextNet: ConcreteImplementationProvider<IOwinContext>
    {
        protected override IOwinContext GetImplementation(IMockProducer mockProducer)
        {
            return new OwinContext();
        }
    }

    public class OwinContext: IOwinContext
    {
        private readonly IAuthenticationManager _authenticationManager = new AuthenticationManager();
        private readonly IDictionary<string, object> _properties = new Dictionary<string, object>();
        private readonly IDictionary<string, object> _environment = new Dictionary<string, object>();
        private readonly IOwinRequest _request;
        private readonly IOwinResponse _response;

        public IAuthenticationManager Authentication { get { return _authenticationManager; } }
        public IDictionary<string, object> Environment { get { return _environment; } }

        public OwinContext()
        {
            _request = new Request(this);
            _response = new Response(this);

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

            var traceOutput = new StringWriter(new StringBuilder());

            Set<TextWriter>("host.TraceOutput", traceOutput);
            Set<IList<IDictionary<string, object>>>("host.Addresses", addresses);
            Set<IDictionary<string, object>>("server.Capabilities", capabilities);
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

    public class Request : IOwinRequest
    {
        public string Accept { get; set; }
        public Stream Body { get; set; }
        public string CacheControl { get; set; }
        public CancellationToken CallCancelled { get; set; }
        public string ContentType { get; set; }
        public IOwinContext Context { get; set; }
        public RequestCookieCollection Cookies { get; set; }
        public IDictionary<string, object> Environment { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public HostString Host { get; set; }
        public bool IsSecure { get; set; }
        public string LocalIpAddress { get; set; }
        public int? LocalPort { get; set; }
        public string MediaType { get; set; }
        public string Method { get; set; }
        public PathString Path { get; set; }
        public PathString PathBase { get; set; }
        public string Protocol { get; set; }
        public IReadableStringCollection Query { get; set; }
        public QueryString QueryString { get; set; }
        public string RemoteIpAddress { get; set; }
        public int? RemotePort { get; set; }
        public string Scheme { get; set; }
        public Uri Uri { get; set; }
        public IPrincipal User { get; set; }

        public Request(OwinContext context)
        {
            Context = context;
        }

        public T Get<T>(string key)
        {
            return Context.Get<T>(key);
        }

        public IOwinRequest Set<T>(string key, T value)
        {
            Context.Set(key, value);
            return this;
        }

        public Task<IFormCollection> ReadFormAsync()
        {
            throw new NotImplementedException();
        }
    }

    public class Response: IOwinResponse
    {
        public IOwinContext Context { get; set; }
        public Stream Body { get; set; }
        public long? ContentLength { get; set; }
        public string ContentType { get; set; }
        public ResponseCookieCollection Cookies { get; set; }
        public string ETag { get; set; }
        public DateTimeOffset? Expires { get; set; }
        public string Protocol { get; set; }
        public string ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public int StatusCode { get; set; }

        public Response(OwinContext context)
        {
            Context = context;
        }

        public IDictionary<string, object> Environment
        {
            get { return Context.Environment; }
        }

        public T Get<T>(string key)
        {
            return Context.Get<T>(key);
        }

        public IOwinResponse Set<T>(string key, T value)
        {
            Context.Set(key, value);
            return this;
        }

        public void OnSendingHeaders(Action<object> callback, object state)
        {
            throw new NotImplementedException();
        }

        public void Redirect(string location)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Write(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Write(string text)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(byte[] data, int offset, int count, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(byte[] data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string text, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task WriteAsync(string text)
        {
            throw new NotImplementedException();
        }
    }

    public class AuthenticationManager : IAuthenticationManager
    {
        public Task<IEnumerable<AuthenticateResult>> AuthenticateAsync(string[] authenticationTypes)
        {
            throw new NotImplementedException();
        }

        public Task<AuthenticateResult> AuthenticateAsync(string authenticationType)
        {
            throw new NotImplementedException();
        }

        public AuthenticationResponseChallenge AuthenticationResponseChallenge { get; set; }

        public AuthenticationResponseGrant AuthenticationResponseGrant { get; set; }

        public AuthenticationResponseRevoke AuthenticationResponseRevoke { get; set; }

        public void Challenge(params string[] authenticationTypes)
        {
            throw new NotImplementedException();
        }

        public void Challenge(AuthenticationProperties properties, params string[] authenticationTypes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuthenticationDescription> GetAuthenticationTypes(Func<AuthenticationDescription, bool> predicate)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AuthenticationDescription> GetAuthenticationTypes()
        {
            throw new NotImplementedException();
        }

        public void SignIn(params System.Security.Claims.ClaimsIdentity[] identities)
        {
            throw new NotImplementedException();
        }

        public void SignIn(AuthenticationProperties properties, params System.Security.Claims.ClaimsIdentity[] identities)
        {
            throw new NotImplementedException();
        }

        public void SignOut(params string[] authenticationTypes)
        {
            throw new NotImplementedException();
        }

        public void SignOut(AuthenticationProperties properties, params string[] authenticationTypes)
        {
            throw new NotImplementedException();
        }

        public System.Security.Claims.ClaimsPrincipal User { get; set; }
    }
}
