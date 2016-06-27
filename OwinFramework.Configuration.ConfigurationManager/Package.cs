using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Configuration.ConfiurationManager
{
    [Package]
    public class Package : IPackage
    {
        public string Name { get { return "Owin framework ConfigurationManager configuration"; } }
        public IList<IocRegistration> IocRegistrations { get; private set; }

        public Package()
        {
            IocRegistrations = new List<IocRegistration>
            {
                new IocRegistration().Init<IConfiguration, ConfigurationManagerConfiguration>(),
            };
        }
    }
}
