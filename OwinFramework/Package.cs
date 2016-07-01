using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Interfaces.Builder;
using OwinFramework.Interfaces.Routing;
using OwinFramework.Interfaces.Utility;
using OwinFramework.Utility;

namespace OwinFramework
{
    [Package]
    public class Package : IPackage
    {
        public string Name { get { return "Owin framework"; } }
        public IList<IocRegistration> IocRegistrations { get; private set; }

        public Package()
        {
            IocRegistrations = new List<IocRegistration>
            {
                new IocRegistration().Init<IDependencyGraphFactory, DependencyGraphFactory>(),
                new IocRegistration().Init<ISegmenterFactory, SegmenterFactory>(),
                new IocRegistration().Init<IBuilder, Builder.Builder>(),
                new IocRegistration().Init<IRouter, Routing.Router>(IocLifetime.MultiInstance),
            };
        }
    }
}
