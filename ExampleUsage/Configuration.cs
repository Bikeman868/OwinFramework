using System;
using OwinFramework.Interfaces;

namespace ExampleUsage
{
    public class Configuration: IConfiguration
    {
        public IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue = default(T))
        {
            onChangeAction(defaultValue);
            return new ChangeRegistration();
        }

        private class ChangeRegistration: IDisposable
        {
            public void Dispose()
            { }
        }
    }
}
