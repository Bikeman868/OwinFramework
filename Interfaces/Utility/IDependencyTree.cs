using System;
using System.Collections.Generic;
using OwinFramework.Interfaces.Builder;

namespace OwinFramework.Interfaces.Utility
{
    public interface IDependencyTree<T>
    {
        void Add(string key, T data, IEnumerable<ITreeDependency> dependencies, PipelinePosition position);
        T GetData(string key);

        IEnumerable<string> GetDecendents(string key, bool topDown = false);

        IEnumerable<string> GetBuildOrderKeys(bool topDown = false);
        IEnumerable<T> GetBuildOrderData(bool topDown = false);
    }
}
