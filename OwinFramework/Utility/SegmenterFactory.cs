using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    /// <summary>
    /// Constructs instances that implement ISegmenter
    /// </summary>
    public class SegmenterFactory : ISegmenterFactory
    {
        private readonly IDependencyGraphFactory _dependencyGraphFactory;

        /// <summary>
        /// Consuructs a SegmenterFactory
        /// </summary>
        public SegmenterFactory(IDependencyGraphFactory dependencyGraphFactory)
        {
            _dependencyGraphFactory = dependencyGraphFactory;
        }


        /// <summary>
        /// Constructs an instance that implements ISegmenter
        /// </summary>
        public ISegmenter Create()
        {
            return new Segmenter(_dependencyGraphFactory);
        }
    }
}
