# Azure Setup: OAuth 2.0 Authentication

**Date**: October 6, 2025
**Branch**: `002-oauth-2-0`

---

## Overview

This document provides step-by-step instructions for setting up Azure infrastructure required for OAuth 2.0 authentication across all deployment environments.

**Prerequisites**:
- Azure CLI installed and authenticated (`az login`)
- Azure AD tenant with admin privileges
- Azure subscription for Key Vault (cloud deployment only)
- GitHub repository with Actions enabled (GitHub Actions deployment only)

---

## Step 1: Azure AD App Registration (Local Development)

### Create App Registration

```bash
# Create app registration for local development
az ad app create \
  --display-name "SpamRemovalAgent-Local" \
  --sign-in-audience "AzureADMyOrg" \
  --enable-id-token-issuance true \
  --public-client-redirect-uris "http://localhost"

# Get the application (client) ID
LOCAL_CLIENT_ID=$(az ad app list --display-name "SpamRemovalAgent-Local" --query "[0].appId" -o tsv)
echo "Local Client ID: $LOCAL_CLIENT_ID"

# Get your tenant ID
TENANT_ID=$(az account show --query "tenantId" -o tsv)
echo "Tenant ID: $TENANT_ID"
```

### Configure API Permissions

```bash
# Add Microsoft Graph delegated permissions
# Mail.ReadWrite permission ID: e1fe6dd8-ba31-4d61-89e7-88639da4683d
az ad app permission add \
  --id $LOCAL_CLIENT_ID \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions e1fe6dd8-ba31-4d61-89e7-88639da4683d=Scope

# Add offline_access permission (included by default, verify)
# offline_access permission ID: 7427e0e9-2fba-42fe-b0c0-848c9e6a8182
az ad app permission add \
  --id $LOCAL_CLIENT_ID \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions 7427e0e9-2fba-42fe-b0c0-848c9e6a8182=Scope

# Grant admin consent (requires admin privileges)
az ad app permission admin-consent --id $LOCAL_CLIENT_ID
```

### Verify Configuration

```bash
# View app registration details
az ad app show --id $LOCAL_CLIENT_ID --query "{DisplayName:displayName, ClientId:appId, RedirectUris:publicClient.redirectUris}"

# View permissions
az ad app permission list --id $LOCAL_CLIENT_ID
```

---

## Step 2: Azure AD App Registration (Cloud Deployment)

### Create Service Principal App

```bash
# Create app registration for cloud deployment
az ad app create \
  --display-name "SpamRemovalAgent-Cloud" \
  --sign-in-audience "AzureADMyOrg"

# Get the application (client) ID
CLOUD_CLIENT_ID=$(az ad app list --display-name "SpamRemovalAgent-Cloud" --query "[0].appId" -o tsv)
echo "Cloud Client ID: $CLOUD_CLIENT_ID"

# Create service principal
az ad sp create --id $CLOUD_CLIENT_ID
```

### Configure APPLICATION Permissions (not Delegated)

```bash
# Add Microsoft Graph APPLICATION permissions
# Mail.ReadWrite (Application) permission ID: e2a3a72e-5f79-4c64-b1b1-878b674786c9
az ad app permission add \
  --id $CLOUD_CLIENT_ID \
  --api 00000003-0000-0000-c000-000000000000 \
  --api-permissions e2a3a72e-5f79-4c64-b1b1-878b674786c9=Role

# Grant admin consent (REQUIRED for application permissions)
az ad app permission admin-consent --id $CLOUD_CLIENT_ID
```

### Create Client Secret

```bash
# Create client secret (valid for 1 year, adjust as needed)
CLOUD_CLIENT_SECRET=$(az ad app credential reset \
  --id $CLOUD_CLIENT_ID \
  --append \
  --display-name "cloud-secret-2025" \
  --years 1 \
  --query "password" -o tsv)

echo "⚠️ IMPORTANT: Save this client secret securely!"
echo "Client Secret: $CLOUD_CLIENT_SECRET"
```

**CRITICAL**: Save these values in a secure password manager:
- **Tenant ID**: `$TENANT_ID`
- **Cloud Client ID**: `$CLOUD_CLIENT_ID`
- **Cloud Client Secret**: `$CLOUD_CLIENT_SECRET`

---

## Step 3: Azure Key Vault Setup (Azure Deployment Only)

### Create Resource Group and Key Vault

```bash
# Create resource group
RESOURCE_GROUP="rg-spamremoval"
LOCATION="eastus"
az group create --name $RESOURCE_GROUP --location $LOCATION

# Generate unique Key Vault name (must be globally unique)
UNIQUE_SUFFIX=$(openssl rand -hex 4)
KEY_VAULT_NAME="kv-spamremoval-${UNIQUE_SUFFIX}"

# Create Key Vault
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization false

echo "Key Vault URI: https://${KEY_VAULT_NAME}.vault.azure.net/"
```

### Store Client Secret in Key Vault

```bash
# Store the cloud client secret
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "azure-client-secret" \
  --value "$CLOUD_CLIENT_SECRET"

# Verify secret stored
az keyvault secret show \
  --vault-name $KEY_VAULT_NAME \
  --name "azure-client-secret" \
  --query "value" -o tsv
```

### Configure Managed Identity Access (For Azure App Service)

If deploying to Azure App Service:

```bash
# Create App Service (example)
APP_NAME="app-spamremoval-${UNIQUE_SUFFIX}"
az appservice plan create \
  --name "plan-spamremoval" \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux

az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan "plan-spamremoval" \
  --runtime "DOTNETCORE:10.0"

# Enable managed identity
az webapp identity assign \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP

# Get managed identity object ID
MANAGED_IDENTITY_ID=$(az webapp identity show \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "principalId" -o tsv)

# Grant Key Vault access to managed identity
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id $MANAGED_IDENTITY_ID \
  --secret-permissions get list
```

---

## Step 4: GitHub Secrets Configuration (GitHub Actions Deployment)

### Add Secrets to Repository

```bash
# Ensure GitHub CLI is authenticated
gh auth status

# Add Azure AD secrets
gh secret set AZURE_TENANT_ID --body "$TENANT_ID"
gh secret set AZURE_CLIENT_ID --body "$CLOUD_CLIENT_ID"
gh secret set AZURE_CLIENT_SECRET --body "$CLOUD_CLIENT_SECRET"

# Verify secrets added
gh secret list
```

### Optional: Pre-Authenticate and Store Initial Tokens

If you want to pre-authenticate interactively and store initial tokens:

```bash
# Run local authentication first to get tokens
cd src/SpamRemovalAgent
dotnet run -- --auth-only

# Extract tokens from Windows Credential Manager (PowerShell)
# Then add to GitHub Secrets
gh secret set OAUTH_ACCESS_TOKEN --body "<access-token-from-credential-manager>"
gh secret set OAUTH_REFRESH_TOKEN --body "<refresh-token-from-credential-manager>"
gh secret set OAUTH_EXPIRY --body "2025-10-06T15:30:00Z"
```

---

## Step 5: Local Environment Configuration

### Set Environment Variables (Windows PowerShell)

```powershell
# Set environment variables for current session
$env:AZURE_TENANT_ID = "$TENANT_ID"
$env:AZURE_CLIENT_ID = "$LOCAL_CLIENT_ID"
$env:OAUTH_REDIRECT_URI = "http://localhost"
$env:OAUTH_SCOPES = "Mail.ReadWrite offline_access"

# Optional: Set permanently (user-level)
[System.Environment]::SetEnvironmentVariable("AZURE_TENANT_ID", "$TENANT_ID", "User")
[System.Environment]::SetEnvironmentVariable("AZURE_CLIENT_ID", "$LOCAL_CLIENT_ID", "User")
```

### Set Environment Variables (Windows CMD)

```cmd
REM Set environment variables for current session
set AZURE_TENANT_ID=%TENANT_ID%
set AZURE_CLIENT_ID=%LOCAL_CLIENT_ID%
set OAUTH_REDIRECT_URI=http://localhost
set OAUTH_SCOPES=Mail.ReadWrite offline_access

REM Set permanently (user-level)
setx AZURE_TENANT_ID "%TENANT_ID%"
setx AZURE_CLIENT_ID "%LOCAL_CLIENT_ID%"
```

### Set Environment Variables (bash/WSL)

```bash
# Add to ~/.bashrc or ~/.profile
export AZURE_TENANT_ID="$TENANT_ID"
export AZURE_CLIENT_ID="$LOCAL_CLIENT_ID"
export OAUTH_REDIRECT_URI="http://localhost"
export OAUTH_SCOPES="Mail.ReadWrite offline_access"
```

---

## Step 6: Azure App Service Configuration

### Configure App Settings

```bash
# Set environment variables for Azure App Service
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
    AZURE_TENANT_ID="$TENANT_ID" \
    AZURE_CLIENT_ID="$CLOUD_CLIENT_ID" \
    AZURE_CLIENT_SECRET="@Microsoft.KeyVault(VaultName=${KEY_VAULT_NAME};SecretName=azure-client-secret)" \
    OAUTH_SCOPES="https://graph.microsoft.com/.default" \
    AZURE_KEY_VAULT_URI="https://${KEY_VAULT_NAME}.vault.azure.net/"
```

**Note**: Using Key Vault reference (`@Microsoft.KeyVault(...)`) ensures the secret is never exposed in logs.

---

## Validation Checklist

### Local Development Setup

- [ ] Local app registration created
- [ ] Delegated permissions configured (Mail.ReadWrite, offline_access)
- [ ] Admin consent granted
- [ ] Environment variables set
- [ ] `dotnet run` opens browser for authentication
- [ ] Token stored in Windows Credential Manager

### Azure Cloud Setup

- [ ] Cloud app registration created
- [ ] Application permissions configured (Mail.ReadWrite)
- [ ] Admin consent granted
- [ ] Service principal created
- [ ] Client secret generated and stored in Key Vault
- [ ] Key Vault created and accessible
- [ ] Managed identity configured for App Service
- [ ] App Service environment variables configured

### GitHub Actions Setup

- [ ] GitHub Secrets configured (AZURE_TENANT_ID, AZURE_CLIENT_ID, AZURE_CLIENT_SECRET)
- [ ] Workflow file created (`.github/workflows/spam-removal.yml`)
- [ ] Workflow runs without errors
- [ ] Logs show successful authentication

---

## Troubleshooting

### Issue: "AADSTS65001: Consent required"

**Solution**: Grant admin consent for the app registration:
```bash
az ad app permission admin-consent --id <app-id>
```

### Issue: "Key Vault access denied"

**Solution**: Verify managed identity has correct permissions:
```bash
az keyvault set-policy \
  --name $KEY_VAULT_NAME \
  --object-id <managed-identity-object-id> \
  --secret-permissions get list
```

### Issue: "Client secret expired"

**Solution**: Generate new secret and update Key Vault:
```bash
NEW_SECRET=$(az ad app credential reset --id $CLOUD_CLIENT_ID --append --query "password" -o tsv)
az keyvault secret set --vault-name $KEY_VAULT_NAME --name "azure-client-secret" --value "$NEW_SECRET"
```

---

## Security Best Practices

1. **Never commit secrets to source control** - Always use environment variables or Key Vault
2. **Rotate client secrets regularly** - Set expiration to 6-12 months
3. **Use managed identities** - Preferred over client secrets when possible
4. **Limit permissions** - Request only Mail.ReadWrite, avoid broader permissions
5. **Monitor access** - Enable Azure AD sign-in logs and audit logs
6. **Use separate app registrations** - Keep local and cloud registrations isolated

---

**Document Status**: ✅ Complete | **Last Updated**: October 6, 2025
