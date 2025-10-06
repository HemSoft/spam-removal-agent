# Tasks: OAuth 2.0 Authentication for Microsoft Graph

**Feature**: OAuth 2.0 Authentication
**Branch**: `002-oauth-2-0`
**Date**: October 6, 2025
**Input**: Design documents from `F:\github\HemSoft\spam-removal-agent\specs\002-oauth-2-0\`

## Summary

This task list implements OAuth 2.0 authentication for Microsoft Graph across three deployment environments (Windows local, Azure cloud, GitHub Actions). Total of **45 tasks** organized into 6 phases following Test-Driven Development (TDD) principles.

**Estimated Effort**: 65-106 hours (2-4 weeks for 1 developer, 1 week for team with parallel execution)

**Critical Path**: Setup → Models → Contract Tests → Implementations → Integration Tests → Polish

---

## Task Format Legend

- **[P]**: Can run in parallel (different files, no dependencies)
- **Depends on**: Tasks that must complete first
- **File Path**: Exact location for implementation
- **Acceptance Criteria**: Definition of done for each task

---

## Phase 1: Setup and Prerequisites (5 tasks)

### T001: Create Authentication Module Directory Structure
**Description**: Create all subdirectories for the authentication module per the implementation plan.

**File Paths**:
- `src/SpamRemovalAgent/Authentication/`
- `src/SpamRemovalAgent/Authentication/TokenManagement/`
- `src/SpamRemovalAgent/Authentication/Flows/`
- `src/SpamRemovalAgent/Authentication/Middleware/`
- `src/SpamRemovalAgent/Authentication/Environment/`
- `src/SpamRemovalAgent/Authentication/Exceptions/`
- `src/SpamRemovalAgent/Authentication/Models/`
- `tests/SpamRemovalAgent.Tests/unit/Authentication/`
- `tests/SpamRemovalAgent.Tests/unit/Authentication/TokenManagement/`
- `tests/SpamRemovalAgent.Tests/unit/Authentication/Flows/`
- `tests/SpamRemovalAgent.Tests/unit/Authentication/Middleware/`
- `tests/SpamRemovalAgent.Tests/unit/Authentication/Environment/`
- `tests/SpamRemovalAgent.Tests/integration/Authentication/`
- `tests/SpamRemovalAgent.Tests/integration/contracts/`
- `tests/SpamRemovalAgent.Tests/utilities/`

**Acceptance Criteria**:
- [X] All directories created
- [X] Directory structure matches plan.md exactly
- [X] No placeholder files needed (empty directories OK)

**Dependencies**: None

---

### T002: Install NuGet Dependencies
**Description**: Add all required NuGet packages for OAuth authentication to `SpamRemovalAgent.csproj` and `SpamRemovalAgent.Tests.csproj`.

**File Paths**:
- `src/SpamRemovalAgent/SpamRemovalAgent.csproj`
- `tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj`

**Packages to Install**:
```xml
<!-- SpamRemovalAgent.csproj -->
<PackageReference Include="Azure.Identity" Version="1.16.0" />
<PackageReference Include="Microsoft.Graph" Version="5.50.0" />
<PackageReference Include="Azure.Security.KeyVault.Secrets" Version="4.6.0" />
<PackageReference Include="Microsoft.Identity.Client" Version="4.61.0" />
<PackageReference Include="System.Security.Cryptography.ProtectedData" Version="8.0.0" />

<!-- SpamRemovalAgent.Tests.csproj -->
<PackageReference Include="WireMock.Net" Version="1.5.58" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
```

**Acceptance Criteria**:
- [ ] All packages restore without errors
- [ ] `dotnet build` succeeds
- [ ] No version conflicts with existing packages

**Dependencies**: T001

---

### T003 [P]: Configure EditorConfig for Authentication Module
**Description**: Add C# coding style rules specific to authentication code (nullable reference types enabled, async naming conventions).

**File Path**: `src/SpamRemovalAgent/.editorconfig`

**Acceptance Criteria**:
- [ ] Nullable reference types enforced for Authentication namespace
- [ ] Async method naming convention enforced (*Async suffix)
- [ ] CA5350 (cryptographic warnings) enabled
- [ ] IDE0058 (unused expression values) enabled for authentication

**Dependencies**: T001

---

### T004 [P]: Add Authentication Configuration to appsettings.json
**Description**: Add fallback configuration structure for OAuth settings (environment variables preferred, this is fallback only).

**File Path**: `src/SpamRemovalAgent/appsettings.json`

**Configuration Structure**:
```json
{
  "Authentication": {
    "TenantId": "",
    "ClientId": "",
    "RedirectUri": "http://localhost",
    "Scopes": "Mail.ReadWrite offline_access",
    "KeyVaultUri": ""
  }
}
```

**Acceptance Criteria**:
- [ ] Configuration section added to appsettings.json
- [ ] appsettings.Development.json includes placeholder values
- [ ] Comments indicate environment variables are preferred

**Dependencies**: T001

---

### T005: Azure Resources Setup Documentation
**Description**: Create setup documentation for Azure AD app registrations and Azure Key Vault (infrastructure prerequisites).

**File Path**: `specs/002-oauth-2-0/SETUP.md`

**Content Sections**:
1. Azure AD app registration steps (local + cloud)
2. Azure Key Vault creation commands
3. Managed identity configuration
4. GitHub Secrets configuration
5. Validation checklist

**Acceptance Criteria**:
- [ ] Document includes all Azure CLI commands from quickstart.md
- [ ] Step-by-step instructions for both local and cloud setups
- [ ] Troubleshooting section included

**Dependencies**: None (documentation only)

---

## Phase 2: Data Models (6 tasks - ALL [P])

### T006 [P]: Create OAuthToken Record
**Description**: Implement the `OAuthToken` record with all properties, validation logic, and `IsExpiringSoon()` method.

**File Path**: `src/SpamRemovalAgent/Authentication/Models/OAuthToken.cs`

**Acceptance Criteria**:
- [ ] Record defined with all 6 properties (AccessToken, RefreshToken, ExpiresAt, Scope, TokenType, CorrelationId)
- [ ] `IsExpiringSoon(TimeSpan? bufferTime)` method implemented with 5-minute default
- [ ] `Validate()` method checks JWT format, expiration, required scope
- [ ] JSON serialization attributes (`[JsonPropertyName]`) applied
- [ ] XML documentation comments for all public members

**Dependencies**: T001, T002

---

### T007 [P]: Create OAuthConfiguration Record
**Description**: Implement the `OAuthConfiguration` record with environment-based loading and validation.

**File Path**: `src/SpamRemovalAgent/Authentication/Models/OAuthConfiguration.cs`

**Acceptance Criteria**:
- [ ] Record defined with all 7 properties (TenantId, ClientId, RedirectUri, ClientSecret, Scopes, AuthMode, KeyVaultUri)
- [ ] `LoadFromEnvironment(IConfiguration)` static method implemented
- [ ] `Validate(DeploymentEnvironment)` method with environment-specific rules
- [ ] Environment variables checked before appsettings.json fallback
- [ ] Throws `InvalidOperationException` if required values missing

**Dependencies**: T001, T002

---

### T008 [P]: Create AuthenticationResult Record
**Description**: Implement the `AuthenticationResult` record with success/failure factory methods.

**File Path**: `src/SpamRemovalAgent/Authentication/Models/AuthenticationResult.cs`

**Acceptance Criteria**:
- [ ] Record defined with all 8 properties (IsSuccess, AccessToken, Token, ErrorCode, ErrorMessage, CorrelationId, Duration, IsPermanentFailure)
- [ ] `Success(OAuthToken, TimeSpan, string?)` static factory method
- [ ] `Failure(string, string, TimeSpan, bool, string?)` static factory method
- [ ] Immutable design (init-only properties)

**Dependencies**: T001, T002, T006 (depends on OAuthToken)

---

### T009 [P]: Create DeploymentEnvironment Enum
**Description**: Define the `DeploymentEnvironment` enum with XML documentation.

**File Path**: `src/SpamRemovalAgent/Authentication/Models/DeploymentEnvironment.cs`

**Acceptance Criteria**:
- [ ] Enum defined with 4 values: Unknown=0, WindowsLocal=1, Azure=2, GitHubActions=3
- [ ] XML documentation for each value explaining detection criteria
- [ ] Namespace: `SpamRemovalAgent.Authentication.Models`

**Dependencies**: T001, T002

---

### T010 [P]: Create AuthenticationMode Enum
**Description**: Define the `AuthenticationMode` enum with XML documentation.

**File Path**: `src/SpamRemovalAgent/Authentication/Models/AuthenticationMode.cs`

**Acceptance Criteria**:
- [ ] Enum defined with 2 values: Interactive=1, ServicePrincipal=2
- [ ] XML documentation for each value explaining use case
- [ ] Namespace: `SpamRemovalAgent.Authentication.Models`

**Dependencies**: T001, T002

---

### T011 [P]: Create ValidationResult Record
**Description**: Implement the `ValidationResult` record with success/failure factory methods.

**File Path**: `src/SpamRemovalAgent/Authentication/Models/ValidationResult.cs`

**Acceptance Criteria**:
- [ ] Record defined with IsSuccess and Errors properties
- [ ] `Success()` static factory method
- [ ] `Failure(IEnumerable<string>)` static factory method
- [ ] Errors as `IReadOnlyList<string>`

**Dependencies**: T001, T002

---

## Phase 3: Contract Tests (10 tasks - ALL [P])

⚠️ **CRITICAL: These tests MUST be written and MUST FAIL before ANY implementation in Phase 4**

### T012 [P]: Contract Test for IOAuthAuthenticator.AuthenticateAsync
**Description**: Write contract test verifying `AuthenticateAsync()` returns valid JWT token.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/contracts/OAuthAuthenticatorContractTests.cs`

**Test Cases**:
- [ ] `AuthenticateAsync_WhenCalled_ReturnsNonEmptyToken()`
- [ ] `AuthenticateAsync_WhenCalled_ReturnsValidJwtFormat()`
- [ ] `AuthenticateAsync_WhenCancelled_ThrowsOperationCanceledException()`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL (implementation not yet created)
- [ ] Uses mock `ITokenStore` and `IConfiguration`
- [ ] JWT format validated with regex: `^[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+\.[A-Za-z0-9-_]+$`

**Dependencies**: T001, T002, T006-T011 (models must exist)

---

### T013 [P]: Contract Test for IOAuthAuthenticator.GetValidAccessTokenAsync
**Description**: Write contract test verifying token retrieval and proactive refresh.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/contracts/OAuthAuthenticatorContractTests.cs`

**Test Cases**:
- [ ] `GetValidAccessTokenAsync_WhenTokenExpired_RefreshesAutomatically()`
- [ ] `GetValidAccessTokenAsync_WithForceRefresh_IgnoresCachedToken()`
- [ ] `GetValidAccessTokenAsync_WhenNoToken_AuthenticatesFirst()`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL
- [ ] Simulates time advancement to trigger refresh
- [ ] Verifies new token obtained after expiration

**Dependencies**: T001, T002, T006-T011

---

### T014 [P]: Contract Test for IOAuthAuthenticator.ClearCredentialsAsync
**Description**: Write contract test verifying credential removal.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/contracts/OAuthAuthenticatorContractTests.cs`

**Test Cases**:
- [ ] `ClearCredentialsAsync_WhenCalled_RemovesStoredTokens()`
- [ ] `HasValidCredentialsAsync_AfterClear_ReturnsFalse()`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL
- [ ] Verifies token store is empty after clear

**Dependencies**: T001, T002, T006-T011

---

### T015 [P]: Contract Test for ITokenStore (All Implementations)
**Description**: Write parameterized contract tests for all 3 `ITokenStore` implementations.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/contracts/TokenStoreContractTests.cs`

**Test Cases** (run for WindowsCredentialStore, AzureKeyVaultTokenStore, GitHubSecretsTokenStore):
- [ ] `StoreAndRetrieve_RoundTrip_PreservesAllFields(Type storeType)`
- [ ] `ClearToken_WhenCalled_RemovesStoredToken(Type storeType)`
- [ ] `RetrieveToken_WhenNoToken_ReturnsNull(Type storeType)`
- [ ] `IsAvailableAsync_WhenStorageAccessible_ReturnsTrue(Type storeType)`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL
- [ ] Uses `[Theory]` with `[InlineData]` for 3 implementations
- [ ] Factory method `CreateTokenStore(Type)` for test setup

**Dependencies**: T001, T002, T006-T011

---

### T016 [P]: Contract Test for IDeploymentEnvironmentDetector
**Description**: Write contract tests for environment detection logic.

**File Path**: `tests/SpamRemovalAgent.Tests/unit/Authentication/Environment/DeploymentEnvironmentDetectorTests.cs`

**Test Cases**:
- [ ] `DetectEnvironment_WhenRunningOnWindows_ReturnsWindowsLocal()`
- [ ] `DetectEnvironment_WhenAzureEnvironmentVariablesSet_ReturnsAzure()`
- [ ] `DetectEnvironment_WhenGitHubActionsVariableSet_ReturnsGitHubActions()`
- [ ] `GetRecommendedAuthMode_ForWindowsLocal_ReturnsInteractive()`
- [ ] `GetRecommendedAuthMode_ForCloudEnvironments_ReturnsServicePrincipal()`
- [ ] `ValidateEnvironmentPrerequisites_WhenMissingVariables_ReturnsFailure()`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL
- [ ] Environment variables mocked using `TestEnvironmentBuilder` utility
- [ ] All 3 environment types tested

**Dependencies**: T001, T002, T006-T011

---

### T017 [P]: Contract Test for IRetryPolicy
**Description**: Write contract tests for retry logic with exponential backoff.

**File Path**: `tests/SpamRemovalAgent.Tests/unit/Authentication/Middleware/RetryPolicyTests.cs`

**Test Cases**:
- [ ] `ExecuteWithRetryAsync_TransientError_RetriesWithBackoff()`
- [ ] `ExecuteWithRetryAsync_PermanentError_FailsImmediately()`
- [ ] `ExecuteWithRetryAsync_MaxRetriesExceeded_ThrowsException()`
- [ ] `IsTransientError_HttpRequestException429_ReturnsTrue()`
- [ ] `IsTransientError_PermanentAuthFailure_ReturnsFalse()`
- [ ] `GetRetryDelay_FirstAttempt_Returns2Seconds()`
- [ ] `GetRetryDelay_FifthAttempt_Returns32Seconds()`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL
- [ ] Verifies backoff delays: 2s, 4s, 8s, 16s, 32s
- [ ] Jitter tested (non-deterministic, verify range)

**Dependencies**: T001, T002, T006-T011

---

### T018 [P]: Contract Test for IAuthenticationTelemetry
**Description**: Write contract tests for telemetry tracking.

**File Path**: `tests/SpamRemovalAgent.Tests/unit/Observability/AuthenticationTelemetryTests.cs`

**Test Cases**:
- [ ] `TrackAuthenticationStart_WhenCalled_LogsEvent()`
- [ ] `TrackAuthenticationSuccess_WhenCalled_LogsWithDuration()`
- [ ] `TrackAuthenticationFailure_WhenCalled_LogsErrorCode()`
- [ ] `TrackTokenRefresh_WhenCalled_LogsSuccess()`
- [ ] `TrackTokenStoreOperation_WhenCalled_LogsOperation()`

**Acceptance Criteria**:
- [ ] Tests compile but FAIL
- [ ] Uses mock `TelemetryClient` and `ILogger<T>`
- [ ] Verifies log messages and custom events

**Dependencies**: T001, T002, T006-T011

---

### T019 [P]: Contract Test for OAuthToken Schema Validation
**Description**: Write JSON serialization contract tests for `OAuthToken`.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/contracts/OAuthTokenContractTests.cs`

**Test Cases**:
- [ ] `Serialize_ThenDeserialize_PreservesAllProperties()`
- [ ] `Deserialize_ValidJson_CreatesToken()`
- [ ] `Validate_ExpiredToken_ReturnsFailure()`
- [ ] `Validate_MissingScope_ReturnsFailure()`

**Acceptance Criteria**:
- [ ] Tests compile and PASS (models already implemented in Phase 2)
- [ ] JSON round-trip preserves all fields
- [ ] Validation rules enforced

**Dependencies**: T001, T002, T006-T011

---

### T020 [P]: Contract Test for OAuthConfiguration Schema Validation
**Description**: Write JSON serialization and validation contract tests for `OAuthConfiguration`.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/contracts/OAuthConfigurationContractTests.cs`

**Test Cases**:
- [ ] `LoadFromEnvironment_WhenVariablesSet_LoadsCorrectly()`
- [ ] `LoadFromEnvironment_WhenMissing_ThrowsException()`
- [ ] `Validate_ForWindowsLocal_RequiresRedirectUri()`
- [ ] `Validate_ForAzure_RequiresClientSecretAndKeyVault()`

**Acceptance Criteria**:
- [ ] Tests compile and PASS (models already implemented)
- [ ] Environment variable loading tested
- [ ] Validation rules per environment type

**Dependencies**: T001, T002, T006-T011

---

### T021 [P]: Create Test Utilities (TestOAuthTokenFactory, TestEnvironmentBuilder)
**Description**: Create helper classes for generating test tokens and mocking environment variables.

**File Paths**:
- `tests/SpamRemovalAgent.Tests/utilities/TestOAuthTokenFactory.cs`
- `tests/SpamRemovalAgent.Tests/utilities/TestEnvironmentBuilder.cs`
- `tests/SpamRemovalAgent.Tests/utilities/GraphApiMockServer.cs`

**Acceptance Criteria**:
- [ ] `TestOAuthTokenFactory.CreateValidToken()` generates realistic test tokens
- [ ] `TestEnvironmentBuilder.WithAzureEnvironment()` mocks Azure variables
- [ ] `GraphApiMockServer` uses WireMock.Net for Graph API mocking
- [ ] All utilities documented with XML comments

**Dependencies**: T001, T002

---

## Phase 4: Core Implementations (13 tasks - Mixed Parallelism)

⚠️ **PREREQUISITE: All Phase 3 contract tests MUST be failing before starting Phase 4**

### T022 [P]: Implement WindowsCredentialStore
**Description**: Implement Windows Credential Manager token storage using ProtectedData API.

**File Path**: `src/SpamRemovalAgent/Authentication/TokenManagement/WindowsCredentialStore.cs`

**Acceptance Criteria**:
- [ ] Implements `ITokenStore` interface
- [ ] Uses `System.Security.Cryptography.ProtectedData` with `DataProtectionScope.CurrentUser`
- [ ] Target name: "SpamRemovalAgent.OAuth"
- [ ] JSON serialization for token storage
- [ ] P/Invoke to Advapi32.dll (CredRead, CredWrite, CredDelete) or use CredentialManagement NuGet
- [ ] `IsAvailableAsync()` checks Windows platform and Credential Manager service status
- [ ] Contract tests from T015 now PASS for this implementation

**Dependencies**: T001-T021 (contract tests must be failing first)

---

### T023 [P]: Implement AzureKeyVaultTokenStore
**Description**: Implement Azure Key Vault token storage using DefaultAzureCredential.

**File Path**: `src/SpamRemovalAgent/Authentication/TokenManagement/AzureKeyVaultTokenStore.cs`

**Acceptance Criteria**:
- [ ] Implements `ITokenStore` interface
- [ ] Uses `Azure.Security.KeyVault.Secrets.SecretClient`
- [ ] Authenticates with `DefaultAzureCredential` (managed identity)
- [ ] Secret name: "oauth-token"
- [ ] JSON serialization for token storage
- [ ] `IsAvailableAsync()` tests Key Vault connectivity
- [ ] Handles `RequestFailedException` gracefully
- [ ] Contract tests from T015 now PASS for this implementation

**Dependencies**: T001-T021

---

### T024 [P]: Implement GitHubSecretsTokenStore
**Description**: Implement GitHub Secrets token storage (read-only from environment variables).

**File Path**: `src/SpamRemovalAgent/Authentication/TokenManagement/GitHubSecretsTokenStore.cs`

**Acceptance Criteria**:
- [ ] Implements `ITokenStore` interface
- [ ] Reads from environment variables: `OAUTH_ACCESS_TOKEN`, `OAUTH_REFRESH_TOKEN`, `OAUTH_EXPIRY`
- [ ] `StoreTokenAsync()` throws `NotSupportedException` with message about GitHub CLI
- [ ] `IsAvailableAsync()` checks if `GITHUB_ACTIONS=true` and required variables exist
- [ ] Contract tests from T015 now PASS (except StoreTokenAsync which throws)

**Dependencies**: T001-T021

---

### T025: Implement TokenStoreFactory
**Description**: Implement factory for creating environment-specific token stores.

**File Path**: `src/SpamRemovalAgent/Authentication/TokenManagement/TokenStoreFactory.cs`

**Acceptance Criteria**:
- [ ] Static method `Create(DeploymentEnvironment)` returns appropriate `ITokenStore`
- [ ] WindowsLocal → `WindowsCredentialStore`
- [ ] Azure → `AzureKeyVaultTokenStore` (requires `AZURE_KEY_VAULT_URI` environment variable)
- [ ] GitHubActions → `GitHubSecretsTokenStore`
- [ ] Unknown → throws `NotSupportedException`
- [ ] Unit tests verify correct store returned for each environment

**Dependencies**: T022-T024 (all stores must be implemented first)

---

### T026 [P]: Implement InteractiveAuthFlow
**Description**: Implement authorization code + PKCE flow using MSAL.NET.

**File Path**: `src/SpamRemovalAgent/Authentication/Flows/InteractiveAuthFlow.cs`

**Acceptance Criteria**:
- [ ] Implements `IAuthFlow` interface (create interface first if needed)
- [ ] Uses `Microsoft.Identity.Client.PublicClientApplicationBuilder`
- [ ] Calls `AcquireTokenInteractive()` with PKCE enabled (default in MSAL 4.x+)
- [ ] Opens system browser for user consent
- [ ] Returns `OAuthToken` with access token, refresh token, and expiration
- [ ] Handles `MsalException` and maps to `PermanentAuthFailureException` or `TransientAuthFailureException`
- [ ] Logs authentication start/success/failure with correlation IDs

**Dependencies**: T001-T021

---

### T027 [P]: Implement ServicePrincipalAuthFlow
**Description**: Implement client credentials flow using Azure.Identity.

**File Path**: `src/SpamRemovalAgent/Authentication/Flows/ServicePrincipalAuthFlow.cs`

**Acceptance Criteria**:
- [ ] Implements `IAuthFlow` interface
- [ ] Uses `Azure.Identity.ClientSecretCredential`
- [ ] Scope: `https://graph.microsoft.com/.default`
- [ ] Returns `OAuthToken` with access token (no refresh token)
- [ ] Handles `AuthenticationFailedException` and maps to exceptions
- [ ] Logs authentication events

**Dependencies**: T001-T021

---

### T028: Create IAuthFlow Interface
**Description**: Define authentication flow abstraction interface.

**File Path**: `src/SpamRemovalAgent/Authentication/Flows/IAuthFlow.cs`

**Acceptance Criteria**:
- [ ] Method: `Task<OAuthToken> AuthenticateAsync(OAuthConfiguration config, CancellationToken cancellationToken)`
- [ ] Method: `Task<OAuthToken> RefreshTokenAsync(OAuthToken token, OAuthConfiguration config, CancellationToken cancellationToken)`
- [ ] XML documentation for all methods

**Dependencies**: T001-T011 (models only)

---

### T029: Implement OAuthAuthenticator
**Description**: Implement the main orchestrator for OAuth authentication.

**File Path**: `src/SpamRemovalAgent/Authentication/OAuthAuthenticator.cs`

**Acceptance Criteria**:
- [ ] Implements `IOAuthAuthenticator` interface
- [ ] Constructor injection: `IConfiguration`, `ILogger<OAuthAuthenticator>`, `ITokenStore`, `IDeploymentEnvironmentDetector`, `IAuthenticationTelemetry`
- [ ] `AuthenticateAsync()` selects flow based on environment (Interactive vs ServicePrincipal)
- [ ] `GetValidAccessTokenAsync()` checks expiration and refreshes proactively (5-minute buffer)
- [ ] `ClearCredentialsAsync()` calls token store clear
- [ ] `HasValidCredentialsAsync()` checks token existence and validates not expired
- [ ] All contract tests from T012-T014 now PASS

**Dependencies**: T022-T028 (token stores, auth flows, factory)

---

### T030: Implement DeploymentEnvironmentDetector
**Description**: Implement environment detection logic.

**File Path**: `src/SpamRemovalAgent/Authentication/Environment/DeploymentEnvironmentDetector.cs`

**Acceptance Criteria**:
- [ ] Implements `IDeploymentEnvironmentDetector` interface
- [ ] `DetectEnvironment()` checks:
  - `GITHUB_ACTIONS=true` → GitHubActions
  - `AZURE_FUNCTIONS_ENVIRONMENT` or `WEBSITE_INSTANCE_ID` → Azure
  - `RuntimeInformation.IsOSPlatform(OSPlatform.Windows)` → WindowsLocal
  - Else → Unknown
- [ ] `GetRecommendedAuthMode()` returns Interactive for WindowsLocal, ServicePrincipal for cloud
- [ ] `ValidateEnvironmentPrerequisites()` checks required environment variables per environment
- [ ] All contract tests from T016 now PASS

**Dependencies**: T001-T021

---

### T031: Implement RetryPolicyMiddleware
**Description**: Implement exponential backoff retry logic.

**File Path**: `src/SpamRemovalAgent/Authentication/Middleware/RetryPolicyMiddleware.cs`

**Acceptance Criteria**:
- [ ] Implements `IRetryPolicy` interface
- [ ] Backoff delays: [2000ms, 4000ms, 8000ms, 16000ms, 32000ms]
- [ ] Max 5 retry attempts
- [ ] Jitter: 0-1000ms random added to each delay
- [ ] `IsTransientError()` returns true for HTTP 429, 503, 504, SocketException, TimeoutException
- [ ] Permanent failures (4xx except 429) fail immediately
- [ ] All contract tests from T017 now PASS

**Dependencies**: T001-T021

---

### T032: Implement TokenRefreshMiddleware
**Description**: Implement proactive token refresh logic.

**File Path**: `src/SpamRemovalAgent/Authentication/Middleware/TokenRefreshMiddleware.cs`

**Acceptance Criteria**:
- [ ] Constructor injection: `ITokenStore`, `IOAuthAuthenticator`, `ILogger<TokenRefreshMiddleware>`
- [ ] `GetValidAccessTokenAsync()` checks if token expiring within 5 minutes
- [ ] If expiring soon: calls `RefreshTokenAsync()` with refresh token
- [ ] On refresh failure: logs warning, clears credentials, throws `AuthenticationRequiredException`
- [ ] Unit tests verify 5-minute buffer logic
- [ ] Integration test verifies full refresh cycle

**Dependencies**: T029 (OAuthAuthenticator must exist)

---

### T033: Implement ThrottlingMiddleware
**Description**: Implement HTTP 429 handling with Retry-After header.

**File Path**: `src/SpamRemovalAgent/Authentication/Middleware/ThrottlingMiddleware.cs`

**Acceptance Criteria**:
- [ ] Method: `Task<HttpResponseMessage> SendWithThrottlingAsync(HttpClient, HttpRequestMessage, CancellationToken)`
- [ ] Detects HTTP 429 responses
- [ ] Reads `Retry-After` header (seconds or HTTP-date format)
- [ ] Waits for specified duration before retrying
- [ ] Logs throttling events with retry delay
- [ ] Unit tests verify Retry-After parsing and wait behavior

**Dependencies**: T001-T021

---

### T034: Implement AuthenticationTelemetry
**Description**: Implement Application Insights telemetry tracking.

**File Path**: `src/SpamRemovalAgent/Observability/AuthenticationTelemetry.cs`

**Acceptance Criteria**:
- [ ] Implements `IAuthenticationTelemetry` interface
- [ ] Constructor injection: `TelemetryClient`, `ILogger<AuthenticationTelemetry>`
- [ ] All 5 tracking methods implemented with custom events
- [ ] Properties and metrics correctly populated
- [ ] Sensitive data redacted (tokens never logged)
- [ ] All contract tests from T018 now PASS

**Dependencies**: T001-T021

---

## Phase 5: Integration Tests (4 tasks - Sequential)

### T035: Integration Test - Windows Local Interactive Flow
**Description**: End-to-end test for interactive authentication on Windows.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/Authentication/InteractiveAuthFlowIntegrationTests.cs`

**Test Cases**:
- [ ] `AuthenticateInteractively_WithRealAzureAD_ObtainsValidToken()`
- [ ] `TokenStoredInWindowsCredentialManager_CanBeRetrieved()`
- [ ] `RefreshToken_AfterInitialAuth_WorksCorrectly()`

**Acceptance Criteria**:
- [ ] Requires test Azure AD tenant (environment variables: `TEST_AZURE_TENANT_ID`, `TEST_AZURE_CLIENT_ID`)
- [ ] Opens real browser for consent (manual step in CI)
- [ ] Verifies token stored in Windows Credential Manager
- [ ] Cleans up credentials after test

**Dependencies**: T022-T034 (all implementations complete)

---

### T036: Integration Test - Azure Cloud Service Principal Flow
**Description**: End-to-end test for service principal authentication.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/Authentication/ServicePrincipalAuthFlowIntegrationTests.cs`

**Test Cases**:
- [ ] `AuthenticateWithServicePrincipal_WithRealAzureAD_ObtainsValidToken()`
- [ ] `TokenStoredInAzureKeyVault_CanBeRetrieved()` (requires test Key Vault)
- [ ] `AccessTokenExpiration_TriggersReAuthentication()`

**Acceptance Criteria**:
- [ ] Requires test service principal (environment variables: `TEST_AZURE_CLIENT_SECRET`)
- [ ] Requires test Azure Key Vault (managed identity configured)
- [ ] Verifies token stored in Key Vault
- [ ] Cleans up secrets after test

**Dependencies**: T023, T027, T029-T034

---

### T037: Integration Test - Token Refresh Cycle
**Description**: Test proactive token refresh behavior.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/Authentication/TokenRefreshIntegrationTests.cs`

**Test Cases**:
- [ ] `ProactiveRefresh_FiveMinutesBeforeExpiry_RefreshesAutomatically()`
- [ ] `RefreshTokenExpired_ClearsCredentialsAndRequiresReAuth()`
- [ ] `MultipleRefreshCycles_MaintainValidTokens()`

**Acceptance Criteria**:
- [ ] Simulates time advancement to trigger refresh
- [ ] Verifies new access token obtained
- [ ] Verifies old token no longer used
- [ ] Tests 3 consecutive refresh cycles

**Dependencies**: T032 (TokenRefreshMiddleware), T029 (OAuthAuthenticator)

---

### T038: Integration Test - Multi-Environment Detection
**Description**: Test environment detection across Windows, Azure, and GitHub Actions.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/Authentication/MultiEnvironmentIntegrationTests.cs`

**Test Cases**:
- [ ] `DetectWindowsLocal_WhenNoCloudVariables_ReturnsWindowsLocal()`
- [ ] `DetectAzure_WhenAzureVariablesSet_ReturnsAzure()`
- [ ] `DetectGitHubActions_WhenGitHubActionsVariableSet_ReturnsGitHubActions()`
- [ ] `CorrectTokenStoreSelected_BasedOnEnvironment()`

**Acceptance Criteria**:
- [ ] Uses `TestEnvironmentBuilder` to mock variables
- [ ] Verifies correct `DeploymentEnvironment` detected
- [ ] Verifies correct token store created by factory
- [ ] Tests all 3 environment types

**Dependencies**: T025 (TokenStoreFactory), T030 (DeploymentEnvironmentDetector)

---

## Phase 6: Configuration and Polish (7 tasks)

### T039: Azure AD App Registration Setup
**Description**: Document and execute Azure AD app registration for local and cloud.

**File Path**: `specs/002-oauth-2-0/AZURE_AD_SETUP.md`

**Steps** (manual execution with documentation):
1. Create local development app registration (delegated permissions)
2. Create cloud deployment app registration (application permissions)
3. Grant admin consent for both apps
4. Generate client secret for cloud app
5. Document client IDs and tenant ID in secure location

**Acceptance Criteria**:
- [ ] Both app registrations exist in Azure AD
- [ ] Permissions configured per quickstart.md
- [ ] Admin consent granted
- [ ] Documentation includes validation commands

**Dependencies**: T005 (SETUP.md documentation)

---

### T040: Azure Key Vault Setup
**Description**: Create Azure Key Vault and configure managed identity.

**File Path**: `specs/002-oauth-2-0/AZURE_KEY_VAULT_SETUP.md`

**Steps** (Azure CLI commands):
```bash
az group create --name rg-spamremoval --location eastus
az keyvault create --name kv-spamremoval-<unique> --resource-group rg-spamremoval --location eastus
az keyvault set-policy --name kv-spamremoval-<unique> --object-id <managed-identity-object-id> --secret-permissions get set delete list
az keyvault secret set --vault-name kv-spamremoval-<unique> --name azure-client-secret --value <client-secret>
```

**Acceptance Criteria**:
- [ ] Key Vault exists and accessible
- [ ] Managed identity has correct permissions
- [ ] Client secret stored securely
- [ ] Connection string documented

**Dependencies**: T039 (requires client secret from app registration)

---

### T041: GitHub Secrets Configuration
**Description**: Configure GitHub repository secrets for GitHub Actions workflow.

**File Path**: `.github/workflows/README.md`

**Secrets to Configure**:
- `AZURE_TENANT_ID`
- `AZURE_CLIENT_ID`
- `AZURE_CLIENT_SECRET`
- `OAUTH_ACCESS_TOKEN` (optional initial token)
- `OAUTH_REFRESH_TOKEN` (optional initial token)
- `OAUTH_EXPIRY` (optional initial expiry)

**Commands**:
```bash
gh secret set AZURE_TENANT_ID --body "<tenant-id>"
gh secret set AZURE_CLIENT_ID --body "<client-id>"
gh secret set AZURE_CLIENT_SECRET --body "<client-secret>"
```

**Acceptance Criteria**:
- [ ] All secrets configured in GitHub repository
- [ ] Secrets accessible in workflow runs
- [ ] Documentation includes rotation instructions

**Dependencies**: T039 (requires app registration credentials)

---

### T042 [P]: Unit Tests for Exception Classes
**Description**: Write unit tests for custom exception classes.

**File Path**: `tests/SpamRemovalAgent.Tests/unit/Authentication/Exceptions/AuthenticationExceptionTests.cs`

**Test Cases**:
- [ ] `PermanentAuthFailureException_ContainsRecoveryInstructions()`
- [ ] `PermanentAuthFailureException_WindowsLocal_HasCorrectGuidance()`
- [ ] `PermanentAuthFailureException_Azure_HasCorrectGuidance()`
- [ ] `TokenStoreUnavailableException_IncludesEnvironmentContext()`

**Acceptance Criteria**:
- [ ] Tests verify exception messages
- [ ] Recovery instructions validated per environment
- [ ] All exception constructors tested

**Dependencies**: T029-T034 (implementations must be complete)

---

### T043 [P]: Performance Tests
**Description**: Validate authentication performance meets requirements.

**File Path**: `tests/SpamRemovalAgent.Tests/integration/Authentication/PerformanceTests.cs`

**Test Cases**:
- [ ] `InteractiveAuthentication_CompletesWithin2Seconds()`
- [ ] `TokenRefresh_CompletesWithin1Second()`
- [ ] `GetValidAccessToken_FromCache_CompletesWithin100Milliseconds()`

**Acceptance Criteria**:
- [ ] Tests run with realistic network conditions
- [ ] P95 latency meets targets
- [ ] Performance baseline documented

**Dependencies**: T035-T038 (integration tests complete)

---

### T044 [P]: Update Project Documentation
**Description**: Update README.md and architecture docs with authentication details.

**File Paths**:
- `README.md` (add authentication section)
- `specs/002-oauth-2-0/ARCHITECTURE.md` (create detailed architecture doc)

**Sections to Add**:
1. Authentication overview
2. Environment-specific setup instructions
3. Troubleshooting guide
4. Security best practices
5. Token lifecycle diagrams

**Acceptance Criteria**:
- [ ] README.md has authentication quick start
- [ ] ARCHITECTURE.md has component diagrams
- [ ] All quickstart scenarios documented
- [ ] External dependencies listed

**Dependencies**: T039-T041 (setup complete)

---

### T045: Execute Manual Test Scenarios from Quickstart
**Description**: Manually execute all 3 scenarios from quickstart.md to validate end-to-end functionality.

**File Path**: `specs/002-oauth-2-0/MANUAL_TEST_RESULTS.md`

**Scenarios to Test**:
1. Windows local interactive authentication
2. Azure cloud service principal authentication
3. GitHub Actions workflow execution

**Acceptance Criteria**:
- [ ] All 3 scenarios execute successfully
- [ ] Tokens obtained and validated
- [ ] Application Insights shows telemetry
- [ ] No errors in logs
- [ ] Results documented with screenshots/logs

**Dependencies**: T039-T044 (all setup and implementation complete)

---

## Dependency Graph

```
Phase 1 (Setup)
T001 → T002 → T003, T004, T005

Phase 2 (Models) - All depend on T001, T002
T006 [P] OAuthToken
T007 [P] OAuthConfiguration
T008 [P] AuthenticationResult (depends on T006)
T009 [P] DeploymentEnvironment
T010 [P] AuthenticationMode
T011 [P] ValidationResult

Phase 3 (Contract Tests) - All depend on T001-T011
T012-T021 [P] All contract tests

Phase 4 (Implementations) - All depend on T001-T021
T022 [P] WindowsCredentialStore
T023 [P] AzureKeyVaultTokenStore
T024 [P] GitHubSecretsTokenStore
T025 TokenStoreFactory (depends on T022-T024)
T026 [P] InteractiveAuthFlow
T027 [P] ServicePrincipalAuthFlow
T028 IAuthFlow interface
T029 OAuthAuthenticator (depends on T025-T028)
T030 DeploymentEnvironmentDetector
T031 RetryPolicyMiddleware
T032 TokenRefreshMiddleware (depends on T029)
T033 ThrottlingMiddleware
T034 AuthenticationTelemetry

Phase 5 (Integration Tests) - Sequential, depend on T022-T034
T035 → T036 → T037 → T038

Phase 6 (Polish) - Depend on Phase 5
T039 Azure AD setup
T040 Key Vault setup (depends on T039)
T041 GitHub Secrets (depends on T039)
T042 [P] Exception tests
T043 [P] Performance tests
T044 [P] Documentation
T045 Manual testing (depends on T039-T044)
```

---

## Parallel Execution Examples

### Example 1: Models (Phase 2)
All 6 model tasks can run simultaneously:
```bash
# Task Agent: Launch parallel tasks
Task: "Create OAuthToken record in src/SpamRemovalAgent/Authentication/Models/OAuthToken.cs"
Task: "Create OAuthConfiguration record in src/SpamRemovalAgent/Authentication/Models/OAuthConfiguration.cs"
Task: "Create AuthenticationResult record in src/SpamRemovalAgent/Authentication/Models/AuthenticationResult.cs"
Task: "Create DeploymentEnvironment enum in src/SpamRemovalAgent/Authentication/Models/DeploymentEnvironment.cs"
Task: "Create AuthenticationMode enum in src/SpamRemovalAgent/Authentication/Models/AuthenticationMode.cs"
Task: "Create ValidationResult record in src/SpamRemovalAgent/Authentication/Models/ValidationResult.cs"
```

### Example 2: Token Stores (Phase 4)
3 token store implementations can run in parallel:
```bash
Task: "Implement WindowsCredentialStore in src/SpamRemovalAgent/Authentication/TokenManagement/WindowsCredentialStore.cs"
Task: "Implement AzureKeyVaultTokenStore in src/SpamRemovalAgent/Authentication/TokenManagement/AzureKeyVaultTokenStore.cs"
Task: "Implement GitHubSecretsTokenStore in src/SpamRemovalAgent/Authentication/TokenManagement/GitHubSecretsTokenStore.cs"
```

### Example 3: Auth Flows (Phase 4)
2 auth flow implementations can run in parallel:
```bash
Task: "Implement InteractiveAuthFlow in src/SpamRemovalAgent/Authentication/Flows/InteractiveAuthFlow.cs"
Task: "Implement ServicePrincipalAuthFlow in src/SpamRemovalAgent/Authentication/Flows/ServicePrincipalAuthFlow.cs"
```

---

## Validation Checklist

Before marking feature complete, verify:

- [ ] All 45 tasks completed
- [ ] All contract tests passing (T012-T021)
- [ ] All integration tests passing (T035-T038)
- [ ] All unit tests passing (T042)
- [ ] Performance tests meet targets (T043): <2s auth, <1s refresh
- [ ] Manual test scenarios executed successfully (T045)
- [ ] Azure AD app registrations configured (T039)
- [ ] Azure Key Vault accessible (T040)
- [ ] GitHub Secrets configured (T041)
- [ ] Documentation complete (T044): README, ARCHITECTURE, quickstart
- [ ] No sensitive data in logs (tokens redacted)
- [ ] Application Insights receiving telemetry
- [ ] All 3 deployment environments tested
- [ ] Code coverage ≥80% for authentication module
- [ ] No compiler warnings in authentication code
- [ ] EditorConfig rules followed (nullable reference types, async naming)

---

## Notes

- **TDD Mandate**: Contract tests (Phase 3) MUST be written and failing before implementations (Phase 4)
- **Parallel Execution**: Tasks marked [P] can run simultaneously (different files, no dependencies)
- **Security**: Never commit tokens, secrets, or client credentials to Git
- **Testing**: Use test Azure AD tenant for integration tests, not production
- **Cleanup**: Always clean up test credentials after integration tests
- **Environment Variables**: Prefer environment variables over appsettings.json for secrets
- **Commit Strategy**: Commit after each task with descriptive message referencing task ID

---

**Document Status**: ✅ Complete
**Generated**: October 6, 2025
**Ready for Execution**: Yes (run `/implement` or execute manually)
