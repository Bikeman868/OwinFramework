using System.Collections.Generic;
using Ioc.Modules;
using Urchin.Client.Data;
using Urchin.Client.Interfaces;

namespace TestServer
{
    [Package]
    public class Package : IPackage
    {
        public string Name { get { return "Test server"; } }

        public IList<IocRegistration> IocRegistrations
        {
            get
            {
                return new List<IocRegistration>
                {
                    new IocRegistration().Init<IConfigurationStore, ConfigurationStore>(IocLifetime.SingleInstance),
                };
            }
        }
    }
}
