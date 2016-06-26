using OwinFramework.Interfaces.Routing;

namespace OwinFramework.Interfaces.Upstream
{
    /// <summary>
    /// Middleware that implement this interface are specifying that they support
    /// upstream communication via interface T
    /// </summary>
    /// <typeparam name="T">The type of upstream communication, for example IUpstreamSession</typeparam>
    public interface IUpstreamCommunicator<T>: IRoutingProcessor where T:class
    {
    }
}
