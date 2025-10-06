using SpamRemovalAgent.Authentication.Models;

namespace SpamRemovalAgent.Authentication.TokenManagement;

/// <summary>
/// Interface for secure OAuth token storage.
/// Implementations must provide encryption at rest and thread-safe operations.
/// </summary>
public interface ITokenStore
{
    /// <summary>
    /// Retrieves the stored OAuth token.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    /// <returns>OAuth token if found, null if no token stored</returns>
    Task<OAuthToken?> RetrieveTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores the OAuth token securely.
    /// Overwrites any existing token.
    /// </summary>
    /// <param name="token">OAuth token to store</param>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the stored OAuth token.
    /// No-op if no token is stored.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations</param>
    Task ClearTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the token store is available and operational.
    /// Called at startup to validate storage prerequisites.
    /// </summary>
    /// <returns>True if storage is available, false otherwise</returns>
    Task<bool> IsAvailableAsync();
}
