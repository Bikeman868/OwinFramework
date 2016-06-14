using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class SegmenterFactory : ISegmenterFactory
    {
        private readonly IDependencyGraphFactory _dependencyGraphFactory;

        public SegmenterFactory(IDependencyGraphFactory dependencyGraphFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;
        }

        public ISegmenter Create()
        {
            return new Segmenter(_dependencyGraphFactory);
        }
    }
}
