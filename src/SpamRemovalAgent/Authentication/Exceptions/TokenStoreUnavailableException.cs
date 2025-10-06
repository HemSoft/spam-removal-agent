namespace SpamRemovalAgent.Authentication.Exceptions;

/// <summary>
/// Exception thrown when the token storage mechanism is unavailable or inaccessible.
/// </summary>
public class TokenStoreUnavailableException : Exception
{
    /// <summary>
    /// Type of token store that is unavailable.
    /// </summary>
    public string StoreType { get; }

    public TokenStoreUnavailableException(string message, string storeType)
        : base(message)
    {
        StoreType = storeType;
    }

    public TokenStoreUnavailableException(string message, string storeType, Exception innerException)
        : base(message, innerException)
    {
        StoreType = storeType;
    }
}
