using System;
using NUnit.Framework;
using OwinFramework.Mocks.Builder;

namespace OwinFramework.Mocks.UnitTests
{
    /// <summary>
    /// Unit tests for mocks in a bit further than most people like to take it
    /// but there is no harm, right.
    /// </summary>
    [TestFixture]
    public class MockConfiguraationTests
    {
        private MockConfiguration _mockConfiguration;

        [SetUp]
        public void Setup()
        {
            _mockConfiguration = new MockConfiguration();
        }

        [Test]
        public void Should_default_configuration()
        {
            string configuredValue = null;

            Action<string> changeHandler = s =>
            {
                configuredValue = s;
            };

            _mockConfiguration.Register("application/class", changeHandler, "default_value");

            Assert.AreEqual(configuredValue, "default_value");
        }

        [Test]
        public void Should_update_configuration()
        {
            string configuredValue = null;

            Action<string> changeHandler = s =>
            {
                configuredValue = s;
            };

            _mockConfiguration.Register("application/class", changeHandler, "default_value");
            _mockConfiguration.SetConfiguration("application/class", "new_value");

            Assert.AreEqual(configuredValue, "new_value");
        }

        [Test]
        public void Should_remember_configuration()
        {
            string configuredValue = null;

            Action<string> changeHandler = s =>
            {
                configuredValue = s;
            };

            _mockConfiguration.SetConfiguration("application/class", "new_value");
            _mockConfiguration.Register("application/class", changeHandler, "default_value");

            Assert.AreEqual(configuredValue, "new_value");
        }

        [Test]
        public void Should_not_polute()
        {
            string configuredValue = null;

            Action<string> changeHandler = s =>
            {
                configuredValue = s;
            };

            _mockConfiguration.Register("application/class", changeHandler, "default_value");
            _mockConfiguration.SetConfiguration("application/module", "new_value");

            Assert.AreEqual(configuredValue, "default_value");
        }

    }
}
