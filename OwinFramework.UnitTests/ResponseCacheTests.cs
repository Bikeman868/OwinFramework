using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Owin;
using NUnit.Framework;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Middleware;
using OwinFramework.MiddlewareHelpers;
using OwinFramework.MiddlewareHelpers.ResponseRewriter;
using OwinFramework.Mocks.Owin;

namespace UnitTests
{
    [TestFixture]
    public class ResponseCacheTests: Moq.Modules.TestBase
    {
        private IOwinContext _owinContext;
        private MockOwinContext _mockOwinContext;

        [SetUp]
        public void SetUp()
        {
            _owinContext = SetupMock<IOwinContext>();
            _mockOwinContext = GetMock<MockOwinContext, IOwinContext>();
        }

        [TearDown]
        public void TearDown()
        {
            _mockOwinContext.Clear();
        }

        [Test]
        public void Should_buffer_response()
        {
            var responseCapture = new ResponseCapture(_owinContext);

            const string testOutput = "This is a test";
            _owinContext.Response.Write(testOutput);

            Assert.AreEqual(0, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(0, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            var buffer = Encoding.UTF8.GetString(responseCapture.OutputBuffer);

            Assert.AreEqual(testOutput, buffer, "Buffered output");

            responseCapture.Send();

            Assert.AreEqual(testOutput.Length, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(testOutput.Length, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            var sentBytes = _mockOwinContext.ResponseStream.ToArray();
            var sentMessage = Encoding.UTF8.GetString(sentBytes);

            Assert.AreEqual(testOutput, sentMessage);
        }

        [Test]
        public void Should_make_output_buffer_available()
        {
            var responseCapture = new ResponseCapture(_owinContext);

            const string testOutput = "This is a test";
            _owinContext.Response.Write(testOutput);

            var responseRewriter = _owinContext.GetFeature<IResponseRewriter>();
            var buffer = Encoding.UTF8.GetString(responseRewriter.OutputBuffer);

            Assert.AreEqual(testOutput, buffer, "Buffered output");

            responseCapture.Send();
        }

        [Test]
        public void Should_replace_output_buffer()
        {
            var responseCapture = new ResponseCapture(_owinContext);

            const string originalMessage = "This is a test";
            const string replacementMessage = "A different output";
            _owinContext.Response.Write(originalMessage);

            var responseRewriter = _owinContext.GetFeature<IResponseRewriter>();
            responseRewriter.OutputBuffer = Encoding.UTF8.GetBytes(replacementMessage);

            responseCapture.Send();

            Assert.AreEqual(replacementMessage.Length, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(replacementMessage.Length, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            var sentBytes = _mockOwinContext.ResponseStream.ToArray();
            var sentMessage = Encoding.UTF8.GetString(sentBytes);

            Assert.AreEqual(replacementMessage, sentMessage);
        }

        [Test]
        public void Should_share_buffered_output()
        {
            var responseCapture1 = new ResponseCapture(_owinContext);
            var responseCapture2 = new ResponseCapture(_owinContext);

            const string testOutput = "This is a test";
            _owinContext.Response.Write(testOutput);

            Assert.AreEqual(0, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(0, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            var buffer1 = Encoding.UTF8.GetString(responseCapture1.OutputBuffer);
            var buffer2 = Encoding.UTF8.GetString(responseCapture2.OutputBuffer);

            Assert.AreEqual(testOutput, buffer1, "Buffered output from inner capture");
            Assert.AreEqual(testOutput, buffer2, "Buffered output from outer capture");
            
            responseCapture2.Send();

            // The inner capture should not send to the response stream
            Assert.AreEqual(0, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(0, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            responseCapture1.Send();

            // The outermost capture should send to the response stream
            Assert.AreEqual(testOutput.Length, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(testOutput.Length, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            var sentBytes = _mockOwinContext.ResponseStream.ToArray();
            var sentMessage = Encoding.UTF8.GetString(sentBytes);

            Assert.AreEqual(testOutput, sentMessage);
        }

        [Test]
        public void Should_replace_shared_output_buffer()
        {
            var responseCapture1 = new ResponseCapture(_owinContext);
            var responseCapture2 = new ResponseCapture(_owinContext);
            var responseCapture3 = new ResponseCapture(_owinContext);

            const string originalMessage = "This is a test";
            const string replacementMessage1 = "First replacement message";
            const string replacementMessage2 = "Second replacement message";
            const string replacementMessage3 = "Third replacement message";

            _owinContext.Response.Write(originalMessage);

            var buffer1 = Encoding.UTF8.GetString(responseCapture1.OutputBuffer);
            var buffer2 = Encoding.UTF8.GetString(responseCapture2.OutputBuffer);
            var buffer3 = Encoding.UTF8.GetString(responseCapture3.OutputBuffer);

            Assert.AreEqual(originalMessage, buffer1, "Buffered output from inner capture");
            Assert.AreEqual(originalMessage, buffer2, "Buffered output from middle capture");
            Assert.AreEqual(originalMessage, buffer3, "Buffered output from outer capture");

            responseCapture3.OutputBuffer = Encoding.UTF8.GetBytes(replacementMessage1);
            responseCapture3.Send();

            var buffer4 = Encoding.UTF8.GetString(responseCapture2.OutputBuffer);
            var buffer5 = Encoding.UTF8.GetString(responseCapture3.OutputBuffer);

            Assert.AreEqual(replacementMessage1, buffer4, "Buffered output from middle capture after 1 replacement");
            Assert.AreEqual(replacementMessage1, buffer5, "Buffered output from outer capture after 1 replacement");

            responseCapture2.OutputBuffer = Encoding.UTF8.GetBytes(replacementMessage2);
            responseCapture2.Send();

            var buffer6 = Encoding.UTF8.GetString(responseCapture3.OutputBuffer);

            Assert.AreEqual(replacementMessage2, buffer6, "Buffered output from middle capture after 2 replacements");

            responseCapture1.OutputBuffer = Encoding.UTF8.GetBytes(replacementMessage3);
            responseCapture1.Send();

            Assert.AreEqual(replacementMessage3.Length, _mockOwinContext.ResponseStream.Position, "Position of output stream");
            Assert.AreEqual(replacementMessage3.Length, _mockOwinContext.ResponseStream.Length, "Length of output stream");

            var sentMessage = Encoding.UTF8.GetString(_mockOwinContext.ResponseStream.ToArray());

            Assert.AreEqual(replacementMessage3, sentMessage);
        }

        [Test]
        public void Should_append_to_replacement_buffer()
        {
            var responseCapture1 = new ResponseCapture(_owinContext);
            var responseCapture2 = new ResponseCapture(_owinContext);

            const string testOutput = "This is a test";
            const string replacementMessage = "First replacement message";
            const string appendedText = " with appendage";

            _owinContext.Response.Write(testOutput);
            responseCapture2.OutputBuffer = Encoding.UTF8.GetBytes(replacementMessage);
            _owinContext.Response.Write(appendedText);

            responseCapture2.Send();
            responseCapture1.Send();

            var sentBytes = _mockOwinContext.ResponseStream.ToArray();
            var sentMessage = Encoding.UTF8.GetString(sentBytes);

            Assert.AreEqual(replacementMessage + appendedText, sentMessage, "Sent message");
        }


    }
}
