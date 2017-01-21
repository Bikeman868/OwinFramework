namespace OwinFramework.InterfacesV1.Middleware
{
    /// <summary>
    /// Middleware that implements IMiddleware&lt;IResponseProducer&gt; is indicating that it
    /// will render content to the response stream. This is mostly usefull in
    /// setting dependencies, for example a NotFound (404) middleware might declare
    /// a dependency on IPresentation middleware so that it only returns a 404
    /// response after the rendering middleware has passed on the request.
    /// </summary>
    public interface IResponseProducer
    {
    }
}
