using System.Collections.Generic;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyGraph<T>
    {
        void Add(string key, T data, IEnumerable<IDependencyGraphEdge> edges, PipelinePosition position);
        T GetData(string key);

        IEnumerable<string> GetDecendents(string key, bool topDown = false);

        IEnumerable<string> GetBuildOrderKeys(bool topDown = false);
        IEnumerable<T> GetBuildOrderData(bool topDown = false);
    }
}
