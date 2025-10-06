# Quickstart: OAuth 2.0 Authentication for Microsoft Graph

**Date**: October 6, 2025
**Branch**: `002-oauth-2-0`
**Related Spec**: [spec.md](./spec.md)
**Related Plan**: [plan.md](./plan.md)

---

## Overview

This quickstart demonstrates how to set up and use the OAuth 2.0 authentication system for Microsoft Graph across all three supported deployment environments: Windows local, Azure cloud, and GitHub Actions.

**Time to Complete**: ~30 minutes
**Prerequisites**: Azure AD tenant, Azure subscription (for Azure deployment), GitHub repository (for Actions deployment)

---

## Prerequisites

### 1. Azure AD App Registration

First, register two applications in Azure AD (one for local, one for cloud).

#### Local Development App (Interactive Flow)

```bash
# Create app registration for local development
az ad app create --display-name "SpamRemovalAgent-Local" \
  --sign-in-audience "AzureADMyOrg" \
  --enable-id-token-issuance true \
  --public-client-redirect-uris "http://localhost"

# Get the app ID
az ad app list --display-name "SpamRemovalAgent-Local" --query "[0].appId" -o tsv

# Add Microsoft Graph permissions
az ad app permission add --id <app-id> --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope  # Mail.ReadWrite

# Grant admin consent (requires admin privileges)
az ad app permission admin-consent --id <app-id>
```

#### Cloud Deployment App (Service Principal)

```bash
# Create app registration for cloud deployment
az ad app create --display-name "SpamRemovalAgent-Cloud" \
  --sign-in-audience "AzureADMyOrg"

# Get the app ID
az ad app list --display-name "SpamRemovalAgent-Cloud" --query "[0].appId" -o tsv

# Add Microsoft Graph APPLICATION permissions (not delegated)
az ad app permission add --id <app-id> --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions e2a3a72e-5f79-4c64-b1b1-878b674786c9=Role  # Mail.ReadWrite (Application)

# Grant admin consent (REQUIRED for application permissions)
az ad app permission admin-consent --id <app-id>

# Create client secret (save this value securely!)
az ad app credential reset --id <app-id> --append --display-name "cloud-secret"
```

**Important**: Save these values:
- Tenant ID: `az account show --query "tenantId" -o tsv`
- Local App Client ID
- Cloud App Client ID
- Cloud App Client Secret

### 2. Azure Key Vault (for Azure Deployment)

```bash
# Create resource group
az group create --name rg-spamremoval --location eastus

# Create Key Vault
az keyvault create --name kv-spamremoval-<unique> --resource-group rg-spamremoval --location eastus

# Enable managed identity for App Service (if using App Service)
az webapp identity assign --name app-spamremoval --resource-group rg-spamremoval

# Grant Key Vault access to managed identity
az keyvault set-policy --name kv-spamremoval-<unique> \
  --object-id <managed-identity-object-id> \
  --secret-permissions get set delete list

# Store client secret in Key Vault
az keyvault secret set --vault-name kv-spamremoval-<unique> \
  --name azure-client-secret --value <cloud-app-client-secret>
```

---

## Scenario 1: Windows Local Development (Interactive)

### Step 1: Configure Environment

Create `.env` file (or set environment variables):

```bash
# Azure AD Configuration
AZURE_TENANT_ID=<your-tenant-id>
AZURE_CLIENT_ID=<local-app-client-id>
OAUTH_REDIRECT_URI=http://localhost
OAUTH_SCOPES=Mail.ReadWrite offline_access

# Optional: Logging
LOG_LEVEL=Information
```

**Or** use `appsettings.Development.json`:

```json
{
  "Authentication": {
    "TenantId": "<your-tenant-id>",
    "ClientId": "<local-app-client-id>",
    "RedirectUri": "http://localhost",
    "Scopes": "Mail.ReadWrite offline_access"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "SpamRemovalAgent.Authentication": "Debug"
    }
  }
}
```

### Step 2: Run the Application

```bash
cd src/SpamRemovalAgent
dotnet run
```

### Step 3: Authenticate

On first run, a browser window will open:
1. Sign in with your Microsoft account
2. Consent to the requested permissions
3. Browser redirects to `http://localhost` with authorization code
4. Application exchanges code for tokens automatically
5. Tokens stored in Windows Credential Manager

### Step 4: Verify Authentication

```bash
# Check if credentials are stored
dotnet run -- --check-auth

# Expected output:
# [INFO] Valid credentials found
# [INFO] Token expires at: 2025-10-06 15:30:00 UTC
# [INFO] Scopes: Mail.ReadWrite offline_access
```

### Step 5: Test Token Refresh

Wait for token to approach expiration (or simulate):

```bash
# The application automatically refreshes tokens 5 minutes before expiry
dotnet run

# Expected log output:
# [INFO] Token expiring in 00:04:32, refreshing proactively
# [INFO] Token refresh successful
```

---

## Scenario 2: Azure Cloud Deployment

### Step 1: Configure Environment Variables

Set environment variables in Azure App Service:

```bash
az webapp config appsettings set --name app-spamremoval --resource-group rg-spamremoval --settings \
  AZURE_TENANT_ID="<your-tenant-id>" \
  AZURE_CLIENT_ID="<cloud-app-client-id>" \
  AZURE_CLIENT_SECRET="<cloud-app-client-secret>" \
  OAUTH_SCOPES="https://graph.microsoft.com/.default" \
  AZURE_KEY_VAULT_URI="https://kv-spamremoval-<unique>.vault.azure.net/"
```

**Or** use Azure Key Vault references:

```bash
az webapp config appsettings set --name app-spamremoval --resource-group rg-spamremoval --settings \
  AZURE_CLIENT_SECRET="@Microsoft.KeyVault(VaultName=kv-spamremoval-<unique>;SecretName=azure-client-secret)"
```

### Step 2: Deploy Application

```bash
# Publish application
dotnet publish -c Release -o ./publish

# Deploy to Azure App Service
az webapp deploy --name app-spamremoval --resource-group rg-spamremoval --src-path ./publish
```

### Step 3: Verify Deployment

```bash
# Check application logs
az webapp log tail --name app-spamremoval --resource-group rg-spamremoval

# Expected log output:
# [INFO] Detected deployment environment: Azure
# [INFO] Using service principal authentication
# [INFO] Authentication started: Flow=ServicePrincipal, Environment=Azure
# [INFO] Authentication succeeded: Duration=1234ms
```

### Step 4: Monitor with Application Insights

```bash
# View authentication metrics
az monitor app-insights metrics show --app <app-insights-name> --resource-group rg-spamremoval \
  --metric "AuthenticationSucceeded" --aggregation Count --interval PT1H

# Query authentication failures
az monitor app-insights query --app <app-insights-name> --resource-group rg-spamremoval \
  --analytics-query "customEvents | where name == 'AuthenticationFailed' | project timestamp, properties"
```

---

## Scenario 3: GitHub Actions Workflow

### Step 1: Configure GitHub Secrets

Add secrets to your GitHub repository:

```bash
gh secret set AZURE_TENANT_ID --body "<your-tenant-id>"
gh secret set AZURE_CLIENT_ID --body "<cloud-app-client-id>"
gh secret set AZURE_CLIENT_SECRET --body "<cloud-app-client-secret>"

# Optional: Store initial OAuth tokens (if pre-authenticated)
gh secret set OAUTH_ACCESS_TOKEN --body "<initial-access-token>"
gh secret set OAUTH_REFRESH_TOKEN --body "<initial-refresh-token>"
gh secret set OAUTH_EXPIRY --body "2025-10-06T15:30:00Z"
```

### Step 2: Create Workflow File

Create `.github/workflows/spam-removal.yml`:

```yaml
name: Run Spam Removal Agent

on:
  schedule:
    - cron: '0 */4 * * *'  # Every 4 hours
  workflow_dispatch:  # Manual trigger

jobs:
  remove-spam:
    runs-on: ubuntu-latest

    steps:
      - name: Checkout code
        uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: Run spam removal agent
        env:
          AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
          AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
          OAUTH_SCOPES: "https://graph.microsoft.com/.default"
          OAUTH_ACCESS_TOKEN: ${{ secrets.OAUTH_ACCESS_TOKEN }}
          OAUTH_REFRESH_TOKEN: ${{ secrets.OAUTH_REFRESH_TOKEN }}
          OAUTH_EXPIRY: ${{ secrets.OAUTH_EXPIRY }}
        run: |
          cd src/SpamRemovalAgent
          dotnet run

      - name: Upload logs on failure
        if: failure()
        uses: actions/upload-artifact@v4
        with:
          name: agent-logs
          path: logs/*.log
```

### Step 3: Trigger Workflow

```bash
# Manual trigger
gh workflow run "Run Spam Removal Agent"

# View run status
gh run list --workflow="Run Spam Removal Agent"

# View logs
gh run view <run-id> --log
```

### Step 4: Monitor Workflow

```yaml
# Add monitoring step to workflow
- name: Check authentication status
  run: |
    if dotnet run -- --check-auth; then
      echo "✅ Authentication successful"
    else
      echo "❌ Authentication failed"
      exit 1
    fi
```

---

## Testing Authentication

### Unit Tests

```bash
cd tests/SpamRemovalAgent.Tests
dotnet test --filter "Category=Unit&Category=Authentication"
```

### Integration Tests (requires real Azure AD)

```bash
# Set test environment variables
export TEST_AZURE_TENANT_ID="<your-tenant-id>"
export TEST_AZURE_CLIENT_ID="<test-app-client-id>"
export TEST_AZURE_CLIENT_SECRET="<test-app-client-secret>"

# Run integration tests
dotnet test --filter "Category=Integration&Category=Authentication"
```

### Manual Test Script

```csharp
// TestAuthentication.cs
using SpamRemovalAgent.Authentication;

var authenticator = new OAuthAuthenticator(configuration, logger, tokenStore);

// Test 1: Initial authentication
Console.WriteLine("Test 1: Initial authentication...");
var token = await authenticator.AuthenticateAsync();
Console.WriteLine($"✅ Token obtained: {token[..20]}...");

// Test 2: Token retrieval (should use cached token)
Console.WriteLine("\nTest 2: Get cached token...");
var cachedToken = await authenticator.GetValidAccessTokenAsync();
Console.WriteLine($"✅ Cached token: {cachedToken[..20]}...");

// Test 3: Force token refresh
Console.WriteLine("\nTest 3: Force token refresh...");
var refreshedToken = await authenticator.GetValidAccessTokenAsync(forceRefresh: true);
Console.WriteLine($"✅ Refreshed token: {refreshedToken[..20]}...");

// Test 4: Clear credentials
Console.WriteLine("\nTest 4: Clear credentials...");
await authenticator.ClearCredentialsAsync();
var hasCredentials = await authenticator.HasValidCredentialsAsync();
Console.WriteLine($"✅ Credentials cleared: {!hasCredentials}");
```

---

## Troubleshooting

### Issue: "Consent required" error

**Cause**: User or admin has not consented to requested permissions.

**Solution**:
```bash
# Grant admin consent
az ad app permission admin-consent --id <app-id>
```

### Issue: "Token store unavailable"

**Windows**: Windows Credential Manager service not running.
```bash
# Check service status
sc query CredentialManager

# Start service if stopped
sc start CredentialManager
```

**Azure**: Managed identity not configured or Key Vault access policy missing.
```bash
# Verify managed identity
az webapp identity show --name app-spamremoval --resource-group rg-spamremoval

# Verify Key Vault access
az keyvault show-policy --name kv-spamremoval-<unique>
```

### Issue: "Invalid client secret"

**Cause**: Client secret expired or incorrect.

**Solution**:
```bash
# Generate new secret
az ad app credential reset --id <app-id> --append

# Update secret in Key Vault or GitHub Secrets
az keyvault secret set --vault-name kv-spamremoval-<unique> \
  --name azure-client-secret --value <new-secret>
```

### Issue: "Refresh token expired"

**Cause**: Refresh token valid for 90 days with no use, or revoked by user/admin.

**Solution**:
- **Windows local**: Re-run interactive authentication
- **Azure/GitHub Actions**: Cannot refresh interactively - requires new service principal or user re-consent

---

## Validation Checklist

Before deploying to production:

- [ ] All environment variables configured
- [ ] Azure AD app permissions granted and admin consent obtained
- [ ] Token storage mechanism tested (Windows Credential Manager / Key Vault / GitHub Secrets)
- [ ] Authentication succeeds in target environment
- [ ] Token refresh works automatically
- [ ] Application Insights receiving telemetry
- [ ] Error handling tested (network failures, invalid credentials)
- [ ] Logs contain no sensitive information (tokens redacted)
- [ ] Performance meets requirements (<2s auth, <1s refresh)

---

## Next Steps

1. **Integrate with Email Processing**: Use `GetValidAccessTokenAsync()` to obtain tokens for Microsoft Graph API calls
2. **Set Up Monitoring**: Configure Application Insights alerts for authentication failures
3. **Implement Token Rotation**: Schedule client secret rotation every 6 months
4. **Add Health Checks**: Expose authentication status via health check endpoint

---

**Document Status**: ✅ Complete | **Last Updated**: October 6, 2025
