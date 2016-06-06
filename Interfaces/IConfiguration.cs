using System;

namespace OwinFramework.Interfaces
{
    public interface IConfiguration
    {
        IDisposable Register<T>(string path, Action<T> onChangeAction, T defaultValue = default(T));
    }
}
