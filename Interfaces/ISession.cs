namespace OwinFramework.Interfaces
{
    /// <summary>
    /// Defines the functionallity exposed by the session feature that
    /// can be used by other middleware compoennts
    /// </summary>
    public interface ISession
    {
        /// <summary>
        /// Returns true if a sesion was established for the current request. If this property
        /// is false then reading session values is not reliable and updating session values
        /// will have no effect
        /// </summary>
        bool HasSession { get; }

        /// <summary>
        /// Gets a strongly typed value from the user's session
        /// </summary>
        T Get<T>(string name);

        /// <summary>
        /// Updates the user's session with a strongly typed value
        /// </summary>
        void Set<T>(string name, T value);

        /// <summary>
        /// For the situations where strong typing is not possible
        /// </summary>
        object this[string name] { get; set; }
    }
}
