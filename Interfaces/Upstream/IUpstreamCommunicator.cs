using Microsoft.Owin;

namespace OwinFramework.Interfaces.Upstream
{
    /// <summary>
    /// Allows multiple IUpstreamCommunicator<T> instances to be added to 
    /// a strongly typed collection
    /// </summary>
    public interface IUpstreamCommunicator
    {
        void InvokeUpstream(IOwinContext context);
    }

    /// <summary>
    /// Middleware that implement this interface are specifying that they support
    /// upstream communication via interface T
    /// </summary>
    /// <typeparam name="T">The type of upstream communication, for example IUpstreamSession</typeparam>
    public interface IUpstreamCommunicator<T>: IUpstreamCommunicator where T:class
    {
    }
}
