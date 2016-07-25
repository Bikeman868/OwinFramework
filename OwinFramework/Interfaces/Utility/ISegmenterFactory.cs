namespace OwinFramework.Interfaces.Utility
{
    /// <summary>
    /// Creates instances of ISegmenter
    /// </summary>
    public interface ISegmenterFactory
    {
        /// <summary>
        /// Creates and initializes an instance of ISegmenter
        /// </summary>
        ISegmenter Create();
    }
}
