using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace OwinFramework.Mocks.Owin
{
    public class MockOwinResponse: IOwinResponse
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

        public MockOwinResponse(IOwinContext context, Stream outputStream)
        {
            Context = context;
            Body = outputStream;
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
        }

        public void Redirect(string location)
        {
        }

        public void Write(byte[] data, int offset, int count)
        {
            Body.Write(data, offset, count);
        }

        public void Write(byte[] data)
        {
            Body.Write(data, 0, data.Length);
        }

        public void Write(string text)
        {
            Write(Encoding.UTF8.GetBytes(text));
        }

#if NET45
        public Task WriteAsync(byte[] data, int offset, int count, CancellationToken token)
        {
            return Body.WriteAsync(data, offset, count, token);
        }

        public Task WriteAsync(byte[] data, CancellationToken token)
        {
            return Body.WriteAsync(data, 0, data.Length, token);
        }

        public Task WriteAsync(byte[] data)
        {
            return Body.WriteAsync(data, 0, data.Length, CancellationToken.None);
        }

        public Task WriteAsync(string text, CancellationToken token)
        {
            return WriteAsync(Encoding.UTF8.GetBytes(text), token);
        }

        public Task WriteAsync(string text)
        {
            return WriteAsync(Encoding.UTF8.GetBytes(text));
        }
#endif

#if NET40
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
#endif
    }
}