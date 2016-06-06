using System.Collections.Generic;
using OwinFramework.Interfaces;

namespace ExampleUsage
{
    public class SessionMiddleware: IMiddleware<ISession>
    {
        public string Name { get; set; }
        public IList<IDependency> Dependencies { get; private set; }

        public SessionMiddleware(IBuilder builder)
        {
            Dependencies = new List<IDependency>();
            builder.Register(this);
        }
    }
}
