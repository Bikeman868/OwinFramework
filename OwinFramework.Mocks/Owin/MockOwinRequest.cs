using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Mocks.Owin
{
    public class MockOwinRequest : IOwinRequest
    {
        public string Accept { get; set; }
        public Stream Body { get; set; }
        public string CacheControl { get; set; }
        public CancellationToken CallCancelled { get; set; }
        public string ContentType { get; set; }
        public IOwinContext Context { get; private set; }
        public RequestCookieCollection Cookies { get; set; }
        public IHeaderDictionary Headers { get; set; }
        public HostString Host { get; set; }
        public bool IsSecure { get; set; }
        public string MediaType { get; set; }
        public string Method { get; set; }
        public PathString Path { get; set; }
        public PathString PathBase { get; set; }
        public string Protocol { get; set; }
        public IReadableStringCollection Query { get; set; }
        public QueryString QueryString { get; set; }
        public string Scheme { get; set; }
        public Uri Uri { get; set; }
        public IPrincipal User { get; set; }

        public string RemoteIpAddress 
        { 
            get { return Get<string>("server.RemoteIpAddress"); }
            set { Set("server.RemoteIpAddress", value); }
        }

        public string LocalIpAddress
        {
            get { return Get<string>("server.LocalIpAddress"); }
            set { Set("server.LocalIpAddress", value); }
        }

        public int? RemotePort
        {
            get
            {
                int port;
                return int.TryParse(Get<string>("server.RemotePort"), out port) ? (int?) port : null;
            }
            set 
            { 
                Set("server.RemotePort", value.HasValue ? value.ToString() : string.Empty); 
            }
        }

        public int? LocalPort
        {
            get
            {
                int port;
                return int.TryParse(Get<string>("server.LocalPort"), out port) ? (int?) port : null;
            }
            set
            {
                Set("server.LocalPort", value.HasValue ? value.ToString() : string.Empty);
            }
        }

        public IDictionary<string, object> Environment 
        {
            get { return Context.Environment; }
        }


        public MockOwinRequest(IOwinContext context)
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
}