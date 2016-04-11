namespace NServiceBus.Installation
{
    /// <summary>
    /// Interface invoked by the infrastructure when going to install an endpoint.
    /// Implementors should not implement this type directly but rather the generic version of it.
    /// </summary>
    public interface INeedToInstallSomething
    {
        /// <summary>
        /// Performs the installation providing permission for the given user.
        /// </summary>
        /// <param name="identity">The user for whom permissions will be given.</param>
        /// <param name="config"><see cref="Configure"/> instance.</param>
        void Install(string identity, Configure config);
    }
}
