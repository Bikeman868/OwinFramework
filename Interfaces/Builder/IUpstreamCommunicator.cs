using OwinFramework.Interfaces.Routing;

namespace OwinFramework.Interfaces.Builder
{
    /// <summary>
    /// Middleware that implement this interface are specifying that they support
    /// upstream communication via interface T. The middleware must inject an
    /// implementation of T into the Owin context during the routing phase of
    /// request processing.
    /// </summary>
    /// <typeparam name="T">The type of upstream communication, for example IUpstreamSession</typeparam>
    public interface IUpstreamCommunicator<T>: IRoutingProcessor where T:class
    {
    }
}
