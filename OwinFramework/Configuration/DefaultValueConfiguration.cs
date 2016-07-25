using System;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Configuration
{
    /// <summary>
    /// This implementation of IConfiguration always supplies the default configuration value.
    /// Most real world applications need to be configurable and should use a different
    /// implementation of IConfiguration. This implementation is useful for demo projects,
    /// unit tests and 'hello world' type projets.
    /// </summary>
    public class DefaultValueConfiguration: IConfiguration
    {
        IDisposable IConfiguration.Register<T>(string path, Action<T> onChangeAction, T defaultValue)
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
