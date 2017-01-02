using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Interfaces.Utility;

namespace OwinFramework
{
    /// <summary>
    /// Configures IoC modules with IoC container needs
    /// </summary>
    [Package]
    public class Package : IPackage
    {
        string IPackage.Name { get { return "Owin framework"; } }
        IList<IocRegistration> IPackage.IocRegistrations { get { return _iocRegistrations; } }

        private readonly IList<IocRegistration> _iocRegistrations;

        /// <summary>
        /// Consutucts this IoC.Modules package definition
        /// </summary>
        public Package()
        {
            _iocRegistrations = new List<IocRegistration>
            {
                new IocRegistration().Init<IDependencyGraphFactory, Utility.DependencyGraphFactory>(),
                new IocRegistration().Init<ISegmenterFactory, Utility.SegmenterFactory>(),
                new IocRegistration().Init<IBuilder, Builder.Builder>(),
                new IocRegistration().Init<IRouter, Routing.Router>(IocLifetime.MultiInstance),
                new IocRegistration().Init<IHostingEnvironment, Utility.HostingEnvironment>(),
            };
        }
    }
}
