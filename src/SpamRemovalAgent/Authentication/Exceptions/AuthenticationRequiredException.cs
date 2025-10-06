namespace SpamRemovalAgent.Authentication.Exceptions;

/// <summary>
/// Exception thrown when re-authentication is required (e.g., refresh token expired).
/// </summary>
public class AuthenticationRequiredException : Exception
{
    public AuthenticationRequiredException(string message)
        : base(message)
    {
    }

    public AuthenticationRequiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
