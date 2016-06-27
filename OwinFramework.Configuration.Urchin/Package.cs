using System.Collections.Generic;
using Ioc.Modules;
using OwinFramework.Interfaces.Builder;
using Urchin.Client.Interfaces;

namespace OwinFramework.Configuration.Urchin
{
    [Package]
    public class Package : IPackage
    {
        public string Name { get { return "Owin framework Urchin configuration"; } }
        public IList<IocRegistration> IocRegistrations { get; private set; }

        public Package()
        {
            IocRegistrations = new List<IocRegistration>
            {
                new IocRegistration().Init<IConfiguration, UrchinConfiguration>(),
                new IocRegistration().Init<IConfigurationStore>(),
            };
        }
    }
}
