using System;
using System.Collections.Generic;
using Microsoft.Owin;
using Moq.Modules;
using NUnit.Framework;
using OwinFramework.Builder;
using OwinFramework.InterfacesV1.Capability;
using OwinFramework.MiddlewareHelpers.Traceable;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Mocks.Builder;

namespace UnitTests
{
    [TestFixture]
    public class TraceFilterTests: TestBase, ITraceable
    {
        public Action<IOwinContext, Func<string>> Trace { get; set; }

        private MockConfiguration _mockConfiguration;
        private TraceFilter _traceFilter;
        private List<string> _traceMessages;

        [SetUp]
        public void SetUp()
        {
            _mockConfiguration = GetMock<MockConfiguration, IConfiguration>();
            _mockConfiguration.Clear();

            _traceMessages = new List<string>();
            Trace = (c, f) => _traceMessages.Add(f());

            _traceFilter = new TraceFilter(SetupMock<IConfiguration>(), this);
        }

        [Test]
        [TestCase(TraceLevel.Debug, "Debug message", 0)]
        [TestCase(TraceLevel.Information, "Information message", 1)]
        [TestCase(TraceLevel.Error, "Error message", 1)]
        public void Should_supress_debug_trace_by_default(TraceLevel level, string message, int expected)
        {
            _traceFilter.Trace(null, level, () => message);
            Assert.AreEqual(expected, _traceMessages.Count);
        }

        [Test]
        public void Should_only_trace_errors()
        {
            _mockConfiguration.SetConfiguration(
                "/owinFramework/middleware/traceFilter",
                new TraceFilter.Configuration
                { 
                    Level = TraceFilterLevel.Error.ToString(),
                    MiddlewareClasses = new List<string>()
                });

            _traceFilter.Trace(null, TraceLevel.Debug, () => "Debug message");
            _traceFilter.Trace(null, TraceLevel.Information, () => "Information message");
            _traceFilter.Trace(null, TraceLevel.Error, () => "Error message");

            Assert.AreEqual(1, _traceMessages.Count);
        }

        [Test]
        public void Should_only_trace_information_and_errros()
        {
            _mockConfiguration.SetConfiguration(
                "/owinFramework/middleware/traceFilter",
                new TraceFilter.Configuration
                {
                    Level = TraceFilterLevel.Information.ToString(),
                    MiddlewareClasses = new List<string>()
                });

            _traceFilter.Trace(null, TraceLevel.Debug, () => "Debug message");
            _traceFilter.Trace(null, TraceLevel.Information, () => "Information message");
            _traceFilter.Trace(null, TraceLevel.Error, () => "Error message");

            Assert.AreEqual(2, _traceMessages.Count);
        }

        [Test]
        public void Should_trace_all()
        {
            _mockConfiguration.SetConfiguration(
                "/owinFramework/middleware/traceFilter",
                new TraceFilter.Configuration
                {
                    Level = TraceFilterLevel.All.ToString(),
                    MiddlewareClasses = new List<string>()
                });

            _traceFilter.Trace(null, TraceLevel.Debug, () => "Debug message");
            _traceFilter.Trace(null, TraceLevel.Information, () => "Information message");
            _traceFilter.Trace(null, TraceLevel.Error, () => "Error message");

            Assert.AreEqual(3, _traceMessages.Count);
        }

        [Test]
        public void Should_exclude_class_name()
        {
            _mockConfiguration.SetConfiguration(
                "/owinFramework/middleware/traceFilter",
                new TraceFilter.Configuration
                {
                    Level = TraceFilterLevel.All.ToString(),
                    MiddlewareClasses = new List<string> 
                    { 
                        "RandomClassName"
                    }
                });

            _traceFilter.Trace(null, TraceLevel.Debug, () => "Debug message");
            _traceFilter.Trace(null, TraceLevel.Information, () => "Information message");
            _traceFilter.Trace(null, TraceLevel.Error, () => "Error message");

            Assert.AreEqual(0, _traceMessages.Count);
        }

        [Test]
        public void Should_include_class_name()
        {
            _mockConfiguration.SetConfiguration(
                "/owinFramework/middleware/traceFilter",
                new TraceFilter.Configuration
                {
                    Level = TraceFilterLevel.Information.ToString(),
                    MiddlewareClasses = new List<string> 
                    { 
                        GetType().Name
                    }
                });

            _traceFilter.Trace(null, TraceLevel.Debug, () => "Debug message");
            _traceFilter.Trace(null, TraceLevel.Information, () => "Information message");
            _traceFilter.Trace(null, TraceLevel.Error, () => "Error message");

            Assert.AreEqual(2, _traceMessages.Count);
        }

        [Test]
        public void Should_include_full_class_name()
        {
            _mockConfiguration.SetConfiguration(
                "/owinFramework/middleware/traceFilter",
                new TraceFilter.Configuration
                {
                    Level = TraceFilterLevel.Information.ToString(),
                    MiddlewareClasses = new List<string> 
                    { 
                        GetType().FullName
                    }
                });

            _traceFilter.Trace(null, TraceLevel.Debug, () => "Debug message");
            _traceFilter.Trace(null, TraceLevel.Information, () => "Information message");
            _traceFilter.Trace(null, TraceLevel.Error, () => "Error message");

            Assert.AreEqual(2, _traceMessages.Count);
        }
    }
}
