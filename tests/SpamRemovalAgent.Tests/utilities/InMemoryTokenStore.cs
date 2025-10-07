namespace SpamRemovalAgent.Tests.Utilities;

using System.Threading;
using System.Threading.Tasks;
using SpamRemovalAgent.Authentication.Models;
using SpamRemovalAgent.Authentication.TokenManagement;

/// <summary>
/// In-memory token store for testing purposes.
/// Does not persist tokens and does not require any external dependencies.
/// </summary>
public sealed class InMemoryTokenStore : ITokenStore
{
    private OAuthToken? _storedToken;
    private readonly bool _isAvailable;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryTokenStore"/> class.
    /// </summary>
    /// <param name="isAvailable">Whether this store should report as available. Default is true.</param>
    public InMemoryTokenStore(bool isAvailable = true)
    {
        _isAvailable = isAvailable;
    }

    /// <inheritdoc />
    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(_isAvailable);
    }

    /// <inheritdoc />
    public Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);
        _storedToken = token;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<OAuthToken?> RetrieveTokenAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_storedToken);
    }

    /// <inheritdoc />
    public Task ClearTokenAsync(CancellationToken cancellationToken = default)
    {
        _storedToken = null;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets the currently stored token for test assertions.
    /// </summary>
    public OAuthToken? GetStoredToken() => _storedToken;
}
