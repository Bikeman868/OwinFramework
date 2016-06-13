using OwinFramework.Interfaces.Utility;

namespace OwinFramework.Utility
{
    public class SegmenterFactory : ISegmenterFactory
    {
        public ISegmenter Create()
        {
            return new Segmenter();
        }
    }
}
