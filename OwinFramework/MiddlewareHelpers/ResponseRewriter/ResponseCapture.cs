using System.IO;
using Microsoft.Owin;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;

namespace OwinFramework.MiddlewareHelpers.ResponseRewriter
{
    /// <summary>
    /// This is a helper class for middleware that captures the response and modifies it
    /// before returning it to the browser
    /// </summary>
    public class ResponseCapture: IResponseRewriter
    {
        private readonly IResponseRewriter _prior;
        private readonly MemoryStream _memoryStream;
        private readonly Stream _responseStream;

        /// <summary>
        /// Constructs an object that will capture the output from downstream middleware.
        /// If multiple middleware components do this, then the response will only be
        /// captured once and they will all share the same output buffer.
        /// </summary>
        /// <param name="owinContext"></param>
        public ResponseCapture(IOwinContext owinContext)
        {
            _prior = owinContext.GetFeature<IResponseRewriter>();

            if (_prior == null)
            {
                _responseStream = owinContext.Response.Body;
                _memoryStream = new MemoryStream();
                owinContext.Response.Body = _memoryStream;
            }

            owinContext.SetFeature<IResponseRewriter>(this);
        }

        /// <summary>
        /// Sends the buffered output to the browser
        /// </summary>
        public void Send()
        { 
            if (_prior == null)
            {
                var buffer = _memoryStream.ToArray();
                _responseStream.Write(buffer, 0, buffer.Length);
            }
        }

        /// <summary>
        /// Gets or sets the content that will be returned to the browser when request
        /// processing is complete.
        /// </summary>
        public byte[] OutputBuffer
        {
            get
            {
                return _prior == null ? _memoryStream.ToArray() : _prior.OutputBuffer;
            }
            set
            {
                if (_prior == null)
                {
                    _memoryStream.Position = 0;
                    if (value == null)
                    {
                        _memoryStream.SetLength(0);
                    }
                    else
                    {
                        _memoryStream.SetLength(value.Length);
                        _memoryStream.Write(value, 0, value.Length);
                    }
                }
                else
                {
                    _prior.OutputBuffer = value;
                }
            }
        }
    }
}
