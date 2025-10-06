using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SpamRemovalAgent.Authentication.Exceptions;
using SpamRemovalAgent.Authentication.Models;

namespace SpamRemovalAgent.Authentication.TokenManagement;

/// <summary>
/// Token store implementation using Windows Credential Manager via DPAPI.
/// Tokens are encrypted with the current user's Windows credentials.
/// </summary>
public class WindowsCredentialStore : ITokenStore
{
    private const string TargetName = "SpamRemovalAgent.OAuth";
    private readonly string _credentialFilePath;

    public WindowsCredentialStore()
    {
        // Store credentials in user's local app data
        var appDataPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "SpamRemovalAgent");
        Directory.CreateDirectory(appFolder);
        _credentialFilePath = Path.Combine(appFolder, "oauth.dat");
    }

    /// <summary>
    /// Retrieves the stored OAuth token.
    /// </summary>
    public async Task<OAuthToken?> RetrieveTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(_credentialFilePath))
                return null;

            // Read encrypted data
            var encryptedData = await File.ReadAllBytesAsync(_credentialFilePath, cancellationToken);

            // Decrypt using DPAPI
            var decryptedData = ProtectedData.Unprotect(
                encryptedData,
                null,
                DataProtectionScope.CurrentUser);

            // Deserialize JSON
            var json = Encoding.UTF8.GetString(decryptedData);
            return JsonSerializer.Deserialize<OAuthToken>(json);
        }
        catch (CryptographicException ex)
        {
            throw new TokenStoreUnavailableException(
                "Failed to decrypt token. The token may have been encrypted by a different user.",
                "WindowsCredentialStore",
                ex);
        }
        catch (Exception ex) when (ex is not TokenStoreUnavailableException)
        {
            // Return null for any retrieval failures (token doesn't exist or is corrupted)
            return null;
        }
    }

    /// <summary>
    /// Stores the OAuth token securely using DPAPI.
    /// </summary>
    public async Task StoreTokenAsync(OAuthToken token, CancellationToken cancellationToken = default)
    {
        if (token == null)
            throw new ArgumentNullException(nameof(token));

        try
        {
            // Serialize to JSON
            var json = JsonSerializer.Serialize(token);
            var plainData = Encoding.UTF8.GetBytes(json);

            // Encrypt using DPAPI
            var encryptedData = ProtectedData.Protect(
                plainData,
                null,
                DataProtectionScope.CurrentUser);

            // Write to file
            await File.WriteAllBytesAsync(_credentialFilePath, encryptedData, cancellationToken);
        }
        catch (CryptographicException ex)
        {
            throw new TokenStoreUnavailableException(
                "Failed to encrypt token using Windows DPAPI.",
                "WindowsCredentialStore",
                ex);
        }
    }

    /// <summary>
    /// Clears the stored OAuth token.
    /// </summary>
    public Task ClearTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(_credentialFilePath))
                File.Delete(_credentialFilePath);
        }
        catch
        {
            // Ignore errors on clear (file might not exist or already deleted)
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Checks if Windows Credential Manager is available.
    /// </summary>
    public Task<bool> IsAvailableAsync()
    {
        // Check if we're on Windows and can write to local app data
        try
        {
            var isWindows = OperatingSystem.IsWindows();
            if (!isWindows)
                return Task.FromResult(false);

            // Test DPAPI availability
            var testData = new byte[] { 1, 2, 3 };
            var encrypted = ProtectedData.Protect(testData, null, DataProtectionScope.CurrentUser);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);

            return Task.FromResult(testData.SequenceEqual(decrypted));
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
}
