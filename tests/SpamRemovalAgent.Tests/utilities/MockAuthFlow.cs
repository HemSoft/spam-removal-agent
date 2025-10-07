namespace SpamRemovalAgent.Tests.Utilities;

using System;
using System.Threading;
using System.Threading.Tasks;
using SpamRemovalAgent.Authentication.Exceptions;
using SpamRemovalAgent.Authentication.Flows;
using SpamRemovalAgent.Authentication.Models;

/// <summary>
/// Mock authentication flow for testing purposes.
/// Returns pre-configured tokens without making real authentication calls.
/// </summary>
public sealed class MockAuthFlow : IAuthFlow
{
    private readonly OAuthToken? _tokenToReturn;
    private readonly Exception? _exceptionToThrow;
    private readonly OAuthToken? _refreshTokenToReturn;
    private readonly Exception? _refreshExceptionToThrow;

    /// <summary>
    /// Number of times AuthenticateAsync was called.
    /// </summary>
    public int AuthenticateCallCount { get; private set; }

    /// <summary>
    /// Number of times RefreshTokenAsync was called.
    /// </summary>
    public int RefreshCallCount { get; private set; }

    /// <summary>
    /// Last configuration passed to AuthenticateAsync.
    /// </summary>
    public OAuthConfiguration? LastAuthConfig { get; private set; }

    /// <summary>
    /// Last token passed to RefreshTokenAsync.
    /// </summary>
    public OAuthToken? LastTokenToRefresh { get; private set; }

    /// <summary>
    /// Creates a mock auth flow that returns a successful token.
    /// </summary>
    public static MockAuthFlow WithSuccess(OAuthToken? token = null)
    {
        return new MockAuthFlow(token ?? TestOAuthTokenFactory.CreateValidToken());
    }

    /// <summary>
    /// Creates a mock auth flow that throws an exception during authentication.
    /// </summary>
    public static MockAuthFlow WithFailure(Exception exception)
    {
        return new MockAuthFlow(exceptionToThrow: exception);
    }

    /// <summary>
    /// Creates a mock auth flow with separate behaviors for auth and refresh.
    /// </summary>
    public static MockAuthFlow WithBehaviors(
        OAuthToken? authToken = null,
        Exception? authException = null,
        OAuthToken? refreshToken = null,
        Exception? refreshException = null)
    {
        return new MockAuthFlow(authToken, authException, refreshToken, refreshException);
    }

    private MockAuthFlow(
        OAuthToken? tokenToReturn = null,
        Exception? exceptionToThrow = null,
        OAuthToken? refreshTokenToReturn = null,
        Exception? refreshExceptionToThrow = null)
    {
        _tokenToReturn = tokenToReturn;
        _exceptionToThrow = exceptionToThrow;
        _refreshTokenToReturn = refreshTokenToReturn;
        _refreshExceptionToThrow = refreshExceptionToThrow;
    }

    /// <inheritdoc />
    public Task<OAuthToken> AuthenticateAsync(OAuthConfiguration config, CancellationToken cancellationToken)
    {
        AuthenticateCallCount++;
        LastAuthConfig = config;

        if (_exceptionToThrow != null)
        {
            throw _exceptionToThrow;
        }

        if (_tokenToReturn == null)
        {
            throw new InvalidOperationException("MockAuthFlow not configured with a token to return");
        }

        return Task.FromResult(_tokenToReturn);
    }

    /// <inheritdoc />
    public Task<OAuthToken> RefreshTokenAsync(OAuthToken token, OAuthConfiguration config, CancellationToken cancellationToken)
    {
        RefreshCallCount++;
        LastTokenToRefresh = token;

        if (_refreshExceptionToThrow != null)
        {
            throw _refreshExceptionToThrow;
        }

        if (_refreshTokenToReturn != null)
        {
            return Task.FromResult(_refreshTokenToReturn);
        }

        // Default behavior: return a refreshed version of the input token
        return Task.FromResult(new OAuthToken
        {
            AccessToken = _tokenToReturn?.AccessToken ?? TestOAuthTokenFactory.CreateValidToken().AccessToken,
            RefreshToken = token.RefreshToken,
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1),
            Scope = token.Scope,
            TokenType = token.TokenType,
            CorrelationId = token.CorrelationId ?? Guid.NewGuid().ToString()
        });
    }
}
