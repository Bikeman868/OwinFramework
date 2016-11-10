using System;
using System.IO;
using System.Text;
using Microsoft.Owin;
using NUnit.Framework;
using OwinFramework.Mocks.Owin;

namespace OwinFramework.Mocks.UnitTests
{
    [TestFixture]
    public class MockOwinContextTests
    {
        private IOwinContext _owinContext;
        private MockOwinContext _mockOwinContext;

        [SetUp]
        public void Setup()
        {
            _mockOwinContext = new MockOwinContext();
            _owinContext = _mockOwinContext.GetImplementation<IOwinContext>(null);

            const string body = "Body of the request";
            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
            _mockOwinContext.SetMockequest(new Uri("http://test.com/mypath?param=value"), bodyStream);
        }

        [Test]
        public void Should_present_request_details()
        {
            var request = _owinContext.Request;
            Assert.AreEqual("http", request.Scheme);
            Assert.AreEqual(80, request.LocalPort);
            Assert.AreEqual(false, request.IsSecure);
            Assert.AreEqual("test.com", request.Host.Value);
            Assert.AreEqual("/mypath", request.Path.Value);
            Assert.AreEqual("/mypath", request.PathBase.Value);
            Assert.AreEqual("param=value", request.QueryString.Value);
            Assert.AreEqual("value", request.Query["param"]);

            var buffer = new byte[10000];
            var contentLength = request.Body.Read(buffer, 0, buffer.Length);
            var requestBody = Encoding.UTF8.GetString(buffer, 0, contentLength);

            Assert.AreEqual("Body of the request", requestBody);
        }

        [Test]
        public void Should_capture_response()
        {
            const string responseText = "{\"hello\":\"world\"}";
            var response = _owinContext.Response;
            response.ContentType = "application/json";
            response.Write(responseText);

            var buffer = new byte[10000];
            var responseStream = _mockOwinContext.ResponseStream;
            responseStream.Position = 0;
            var contentLength = responseStream.Read(buffer, 0, buffer.Length);
            var capturedResponse = Encoding.UTF8.GetString(buffer, 0, contentLength);

            Assert.AreEqual(responseText, capturedResponse);
        }
    }
}
