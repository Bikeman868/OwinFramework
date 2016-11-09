using System;
using Moq.Modules;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Mocks.Builder
{
    public class MockConfiguration : ConcreteImplementationProvider<IConfiguration>, IConfiguration, IDisposable
    {
        protected override IConfiguration GetImplementation(IMockProducer mockProducer)
        {
            return this;
        }

        public IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue)
        {
            onChangeAction(defaultValue);
            return this;
        }

        public void Dispose()
        {
        }
    }
}
