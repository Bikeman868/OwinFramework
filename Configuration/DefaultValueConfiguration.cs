using System;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Configuration
{
    /// <summary>
    /// This implementation of IConfiguration always supplies the default configuration value.
    /// Most real world applications ned to be configurable and should use a different
    /// implementation of IConfiguration. This implementation is useful for demo projects,
    /// and 'hello world' type projets.
    /// </summary>
    public class DefaultValueConfiguration: IConfiguration
    {
        public IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue = default(T))
        {
            onChangeAction(defaultValue);
            return new ChangeRegistration();
        }

        private class ChangeRegistration: IDisposable
        {
            public void Dispose() { }
        }
    }
}
