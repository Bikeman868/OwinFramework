using System;
using OwinFramework.Interfaces;
using OwinFramework.Interfaces.Builder;

namespace ExampleUsage
{
    /// <summary>
    /// This example implementation of IConfiguration always supplies the default configuration value.
    /// Real implementations should read configuration from a database, config file, web service etc.
    /// </summary>
    public class Configuration: IConfiguration
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
