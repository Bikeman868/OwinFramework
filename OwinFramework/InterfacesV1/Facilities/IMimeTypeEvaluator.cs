namespace OwinFramework.InterfacesV1.Facilities
{
    /// <summary>
    /// Determines the appropriate mime type to use for a response.
    /// Can use configuration or logic to determine the correct type
    /// </summary>
    public interface IMimeTypeEvaluator
    {
        /// <summary>
        /// Maps a file extension onto a mime type
        /// </summary>
        string MimeTypeFromExtension(string fileExtension);

        /// <summary>
        /// Evaluates the contents of a response and determines
        /// the mime type by looking for known patterns in the date
        /// </summary>
        string MimeTypeFromContent(string fileContent);

        /// <summary>
        /// Evaluates the contents of a response and determines
        /// the mime type by looking for known patterns in the date
        /// </summary>
        string MimeTypeFromContent(byte[] content);
    }
}
