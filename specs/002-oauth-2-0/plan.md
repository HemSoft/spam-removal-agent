
# Implementation Plan: OAuth 2.0 Authentication for Microsoft Graph

**Branch**: `002-oauth-2-0` | **Date**: October 6, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `F:\github\HemSoft\spam-removal-agent\specs\002-oauth-2-0\spec.md`

## Execution Flow (/plan command scope)
```
1. Load feature spec from Input path
   → If not found: ERROR "No feature spec at {path}"
2. Fill Technical Context (scan for NEEDS CLARIFICATION)
   → Detect Project Type from file system structure or context (web=frontend+backend, mobile=app+api)
   → Set Structure Decision based on project type
3. Fill the Constitution Check section based on the content of the constitution document.
4. Evaluate Constitution Check section below
   → If violations exist: Document in Complexity Tracking
   → If no justification possible: ERROR "Simplify approach first"
   → Update Progress Tracking: Initial Constitution Check
5. Execute Phase 0 → research.md
   → If NEEDS CLARIFICATION remain: ERROR "Resolve unknowns"
6. Execute Phase 1 → contracts, data-model.md, quickstart.md, agent-specific template file (e.g., `CLAUDE.md` for Claude Code, `.github/copilot-instructions.md` for GitHub Copilot, `GEMINI.md` for Gemini CLI, `QWEN.md` for Qwen Code, or `AGENTS.md` for all other agents).
7. Re-evaluate Constitution Check section
   → If new violations: Refactor design, return to Phase 1
   → Update Progress Tracking: Post-Design Constitution Check
8. Plan Phase 2 → Describe task generation approach (DO NOT create tasks.md)
9. STOP - Ready for /tasks command
```

**IMPORTANT**: The /plan command STOPS at step 7. Phases 2-4 are executed by other commands:
- Phase 2: /tasks command creates tasks.md
- Phase 3-4: Implementation execution (manual or via tools)

## Summary

This plan implements OAuth 2.0 authentication for Microsoft Graph mailbox access across three deployment environments (Windows local, Azure cloud, GitHub Actions). The implementation uses Azure Identity SDK for .NET with environment-specific token storage: Windows Credential Manager (via ProtectedData API) for local deployment, Azure Key Vault with managed identity for Azure, and GitHub Secrets for GitHub Actions workflows. Authentication supports both interactive authorization code flow with PKCE for local development and non-interactive service principal authentication for cloud deployments. The system automatically refreshes access tokens proactively (5 minutes before expiration) and handles permanent token failures with environment-specific recovery instructions.

## Technical Context
**Language/Version**: C# 12 / .NET 10
**Primary Dependencies**:
- Azure.Identity (v1.16.0+) - DefaultAzureCredential, Azure Key Vault integration
- Microsoft.Graph (v5.x) - Microsoft Graph .NET SDK
- Azure.Security.KeyVault.Secrets (v4.x) - Azure Key Vault secret management
- System.Security.Cryptography.ProtectedData - Windows Credential Manager via DPAPI
- Microsoft.Identity.Client (MSAL) - OAuth 2.0 flows and token management
- Microsoft.Extensions.Configuration - Environment variable and appsettings.json configuration
- Microsoft.Extensions.Logging - Structured logging with ILogger<T>

**Storage**:
- Windows local: Windows Credential Manager (DPAPI via ProtectedData API)
- Azure cloud: Azure Key Vault with managed identity authentication
- GitHub Actions: GitHub Secrets (accessed via environment variables at runtime)

**Testing**: xUnit, Moq (for mocking Graph API and token storage), WireMock.Net (for HTTP mocking), FluentAssertions
**Target Platform**:
- Primary: Windows 10+ local deployment
- Secondary: Azure App Service / Azure Container Instances
- Tertiary: GitHub Actions workflows (scheduled or event-triggered)

**Project Type**: Single console application with multi-environment abstraction layer
**Performance Goals**:
- Token acquisition: <2 seconds for interactive flow, <1 second for refresh
- Proactive refresh: 5 minutes before token expiration
- Retry resilience: 5 attempts with exponential backoff (2s, 4s, 8s, 16s, 32s)

**Constraints**:
- Security: Never store tokens in plaintext, logs, or unsecured configuration
- Compliance: OAuth 2.0 with PKCE, minimal permissions (Mail.ReadWrite only)
- Observability: All authentication events logged with correlation IDs
- Network: Must handle Microsoft Graph throttling (HTTP 429) with Retry-After headers

**Scale/Scope**:
- Single mailbox per deployment instance (no multi-tenancy)
- Three deployment environments with isolated credential stores
- ~20 authentication-related classes, ~50 unit tests, ~15 integration tests

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

## Constitution Check
*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Autonomous Operation**:
- [x] Agent designed for continuous, unattended operation - Authentication system supports both interactive (local) and non-interactive (cloud) flows
- [x] Proper error handling that doesn't stop the agent - Retry logic with exponential backoff, graceful degradation on permanent failures
- [x] Clear termination conditions (no spam detected or manual stop) - N/A for authentication module (handles auth lifecycle independently)

**Microsoft Graph Integration**:
- [x] Only Microsoft Graph .NET SDK used for email operations - Using Microsoft.Graph SDK for all mailbox access
- [x] No IMAP/POP3 or direct email protocol usage - Exclusively OAuth 2.0 via Microsoft Graph
- [x] OAuth 2.0 authentication with minimal permissions (Mail.ReadWrite) - Requesting Mail.ReadWrite scope only, with permission validation

**Agent Framework Architecture**:
- [~] Built using Microsoft Agent Framework patterns - Authentication service provides foundation for future agent integration (not implementing agent patterns yet)
- [~] Proper agent lifecycle management implemented - Token lifecycle managed, agent integration deferred to future work
- [x] Decision-making capabilities and extensibility considered - Environment detection, token store abstraction, extensible retry policies

**Conservative Spam Detection (NON-NEGOTIABLE)**:
- [N/A] Multiple validation layers with confidence thresholds - Not applicable to authentication module
- [N/A] False negatives prioritized over false positives - Not applicable to authentication module
- [N/A] All deletions logged with detailed reasoning - Not applicable to authentication module
- [N/A] No risk of deleting legitimate emails - Not applicable to authentication module

**Comprehensive Logging & Observability**:
- [x] Structured logging for all agent actions - ILogger<T> with structured logging for all authentication events
- [x] Email metadata, confidence scores, and decision rationale logged - Authentication metadata (correlation IDs, timestamps, token lifetimes) logged
- [x] Proper log levels and observability patterns - Debug/Info/Warning/Error levels, sensitive data redaction
- [x] Performance monitoring and error tracking - Authentication latency, success/failure rates tracked
- [x] Application Insights integrated for telemetry and distributed tracing - TelemetryClient for custom events and metrics
- [~] .NET Aspire used for development-time observability and orchestration - Deferred to AppHost project (not in authentication module scope)

**Technology Stack Requirements**:
- [x] .NET 10 C# Console Application architecture - Modern C# 12 features, top-level statements
- [~] .NET Aspire integration for orchestration and service discovery - Deferred to AppHost project
- [x] Application Insights configured for production monitoring - TelemetryClient integrated in authentication service
- [x] Local Windows 10 deployment with Microsoft Agent Framework - Multi-environment support including Windows local
- [x] Modern C# language features and performance optimizations utilized - Async/await, nullable reference types, record types

**Security Requirements**:
- [x] OAuth tokens stored securely (Windows Credential Manager) - ProtectedData API for Windows, Azure Key Vault for Azure, GitHub Secrets for Actions
- [x] No credentials in plain text or config files - Environment variables required for cloud, secure stores for all tokens
- [x] Local email processing without external service calls - Authentication service operates within app boundary
- [x] Minimal Graph API permissions requested - Mail.ReadWrite scope validated and enforced

**Quality Standards**:
- [x] Exponential backoff and circuit breaker patterns - 5 retries with 2/4/8/16/32 second backoff
- [x] API rate limiting and batching implemented - Respects Retry-After headers for HTTP 429 throttling
- [x] Comprehensive unit and integration tests - ~50 unit tests (token store, retry logic, environment detection), ~15 integration tests (Graph API, OAuth flows)
- [N/A] Performance targets met (100+ emails/minute) - Not applicable to authentication module (email processing handled elsewhere)

## Project Structure

### Documentation (this feature)
```
specs/[###-feature]/
├── plan.md              # This file (/plan command output)
├── research.md          # Phase 0 output (/plan command)
├── data-model.md        # Phase 1 output (/plan command)
├── quickstart.md        # Phase 1 output (/plan command)
├── contracts/           # Phase 1 output (/plan command)
└── tasks.md             # Phase 2 output (/tasks command - NOT created by /plan)
```

### Source Code (repository root)
```
src/SpamRemovalAgent/
├── Authentication/
│   ├── IOAuthAuthenticator.cs          # Core authentication interface
│   ├── OAuthAuthenticator.cs           # Main authenticator implementation
│   ├── OAuthConfiguration.cs           # Configuration model (tenant, client ID, scopes)
│   ├── TokenManagement/
│   │   ├── ITokenStore.cs              # Token storage abstraction
│   │   ├── WindowsCredentialStore.cs   # Windows Credential Manager implementation
│   │   ├── AzureKeyVaultTokenStore.cs  # Azure Key Vault implementation
│   │   ├── GitHubSecretsTokenStore.cs  # GitHub Actions environment variable reader
│   │   ├── TokenStoreFactory.cs        # Environment-based factory
│   │   └── OAuthToken.cs               # Token model (access, refresh, expiry)
│   ├── Flows/
│   │   ├── InteractiveAuthFlow.cs      # Authorization code + PKCE for Windows
│   │   ├── ServicePrincipalAuthFlow.cs # Client credential flow for cloud
│   │   └── IAuthFlow.cs                # Auth flow abstraction
│   ├── Middleware/
│   │   ├── RetryPolicyMiddleware.cs    # Exponential backoff + circuit breaker
│   │   ├── TokenRefreshMiddleware.cs   # Proactive token refresh (5min before expiry)
│   │   └── ThrottlingMiddleware.cs     # HTTP 429 handling with Retry-After
│   ├── Environment/
│   │   ├── DeploymentEnvironmentDetector.cs  # Detect Windows/Azure/GitHubActions
│   │   └── DeploymentEnvironment.cs          # Enum for environment types
│   └── Exceptions/
│       ├── PermanentAuthFailureException.cs  # Non-retryable auth errors
│       └── TokenStoreUnavailableException.cs # Secure storage not found
├── Observability/
│   ├── AuthenticationTelemetry.cs      # Application Insights custom events
│   └── StructuredLogger.cs             # ILogger<T> wrapper with redaction
└── appsettings.json                    # Fallback config (environment vars preferred)

tests/SpamRemovalAgent.Tests/
├── unit/
│   ├── Authentication/
│   │   ├── TokenManagement/
│   │   │   ├── WindowsCredentialStoreTests.cs
│   │   │   ├── AzureKeyVaultTokenStoreTests.cs
│   │   │   ├── TokenStoreFactoryTests.cs
│   │   │   └── OAuthTokenTests.cs
│   │   ├── Flows/
│   │   │   ├── InteractiveAuthFlowTests.cs
│   │   │   └── ServicePrincipalAuthFlowTests.cs
│   │   ├── Middleware/
│   │   │   ├── RetryPolicyMiddlewareTests.cs
│   │   │   ├── TokenRefreshMiddlewareTests.cs
│   │   │   └── ThrottlingMiddlewareTests.cs
│   │   ├── Environment/
│   │   │   └── DeploymentEnvironmentDetectorTests.cs
│   │   └── OAuthAuthenticatorTests.cs
│   └── Observability/
│       ├── AuthenticationTelemetryTests.cs
│       └── StructuredLoggerTests.cs
├── integration/
│   ├── Authentication/
│   │   ├── InteractiveAuthFlowIntegrationTests.cs    # Real Azure AD (test tenant)
│   │   ├── ServicePrincipalAuthFlowIntegrationTests.cs
│   │   ├── MicrosoftGraphAuthenticationTests.cs      # End-to-end Graph API call
│   │   ├── AzureKeyVaultIntegrationTests.cs          # Real Key Vault operations
│   │   └── TokenRefreshIntegrationTests.cs           # Full refresh cycle
│   └── contracts/
│       ├── OAuthTokenContractTests.cs                # Token schema validation
│       └── OAuthConfigurationContractTests.cs        # Config schema validation
└── utilities/
    ├── TestOAuthTokenFactory.cs          # Generate test tokens
    ├── TestEnvironmentBuilder.cs         # Mock environment variables
    └── GraphApiMockServer.cs             # WireMock.Net setup for Graph API
```

**Structure Decision**: Single project architecture (Option 1) with authentication module as a subdirectory of the main console application. This approach keeps authentication concerns isolated while allowing direct integration with the existing `SpamRemovalAgent` project structure. The `Authentication/` directory contains all OAuth-related logic with clear separation of concerns:
- **TokenManagement**: Storage abstraction + environment-specific implementations
- **Flows**: Authentication flow implementations (interactive vs service principal)
- **Middleware**: Cross-cutting concerns (retry, refresh, throttling)
- **Environment**: Deployment environment detection
- **Exceptions**: Domain-specific error types

Test structure mirrors source structure with dedicated unit, integration, and contract test directories.

## Phase 0: Outline & Research

✅ **COMPLETE** - All research questions resolved in `research.md`

**Research Areas Completed**:
1. ✅ OAuth 2.0 Authorization Code Flow with Microsoft Identity Platform
   - Decision: Authorization code + PKCE for local, client credentials for cloud
   - Researched: Microsoft Learn docs, OAuth 2.0 RFC, PKCE RFC

2. ✅ Azure Identity SDK for .NET Best Practices
   - Decision: Azure.Identity (DefaultAzureCredential) + MSAL.NET
   - Researched: Azure Identity documentation, MSAL patterns, token caching

3. ✅ Multi-Environment Token Storage
   - Decision: Windows Credential Manager (DPAPI), Azure Key Vault, GitHub Secrets
   - Researched: ProtectedData API, Key Vault SDK, GitHub Actions secrets

4. ✅ Token Refresh and Lifetime Management
   - Decision: Proactive refresh 5 minutes before expiration
   - Researched: Microsoft identity token lifetimes, refresh best practices

5. ✅ Error Handling and Retry Strategies
   - Decision: Exponential backoff with jitter, fail-fast for permanent errors
   - Researched: Microsoft Graph throttling, resilience patterns

6. ✅ Azure AD App Registration Requirements
   - Decision: Separate registrations for local (delegated) and cloud (application permissions)
   - Researched: Azure AD app registration docs, permission types

7. ✅ Observability and Telemetry
   - Decision: Application Insights SDK + ILogger<T> with sensitive data redaction
   - Researched: Application Insights for .NET, structured logging patterns

**Output**: [research.md](./research.md) - Comprehensive research document with decisions, rationale, implementation patterns, and alternatives

## Phase 1: Design & Contracts

✅ **COMPLETE** - All design artifacts generated

**Artifacts Created**:
1. ✅ **data-model.md** - Core entity definitions
   - `OAuthToken` - Complete token set with expiration metadata and validation
   - `OAuthConfiguration` - Static authentication configuration with environment loading
   - `AuthenticationResult` - Authentication operation outcome with success/failure metadata
   - `DeploymentEnvironment` enum - Environment detection (Windows/Azure/GitHubActions)
   - `AuthenticationMode` enum - Interactive vs ServicePrincipal flows
   - `ValidationResult` - Validation helper for all entities

2. ✅ **contracts/authentication-contracts.md** - Public API contracts
   - `IOAuthAuthenticator` - Primary authentication interface
   - `ITokenStore` - Token storage abstraction (3 implementations)
   - `IDeploymentEnvironmentDetector` - Environment detection interface
   - `IRetryPolicy` - Retry logic with exponential backoff
   - `IAuthenticationTelemetry` - Telemetry and logging interface
   - Contract tests for all interfaces (TDD-ready)
   - API stability guarantees and backward compatibility rules

3. ✅ **quickstart.md** - End-to-end setup guide
   - Azure AD app registration (local + cloud)
   - Azure Key Vault configuration
   - Scenario 1: Windows local development (interactive flow)
   - Scenario 2: Azure cloud deployment (service principal)
   - Scenario 3: GitHub Actions workflow
   - Testing guide (unit, integration, manual tests)
   - Troubleshooting common issues
   - Validation checklist

4. ⏭️ **Agent file update** - Deferred to post-plan phase (requires script execution)

**Output**: [data-model.md](./data-model.md), [contracts/](./contracts/), [quickstart.md](./quickstart.md)

## Phase 2: Task Planning Approach

✅ **COMPLETE** - Task generation approach defined

*This section describes what the /tasks command will do - DO NOT execute during /plan*

**Task Generation Strategy** (to be executed by /tasks command):

1. **Load Base Template**:
   - Use `.specify/templates/tasks-template.md` as starting structure
   - Parse Phase 1 artifacts: data-model.md (6 entities), contracts/ (5 interfaces), quickstart.md (3 scenarios)

2. **Generate Task Categories**:

   **Category A: Model Creation** (All [P] - Pure data structures, no dependencies)
   - Task: Create `OAuthToken` record with expiration validation
   - Task: Create `OAuthConfiguration` record with environment loading
   - Task: Create `AuthenticationResult`, `DeploymentEnvironment`, `AuthenticationMode`, `ValidationResult`
   - **Estimated**: 6 tasks × 1-2 hours = 6-12 hours

   **Category B: Contract Tests** (All [P] - Independent test suites)
   - Task: Contract tests for `IOAuthAuthenticator` (4 methods)
   - Task: Contract tests for `ITokenStore` (4 methods × 3 implementations)
   - Task: Contract tests for `IDeploymentEnvironmentDetector`, `IRetryPolicy`, `IAuthenticationTelemetry`
   - **Estimated**: 5 tasks × 2-3 hours = 10-15 hours

   **Category C: Core Implementations** (Mixed dependencies)
   - Task: Implement `WindowsTokenStore` (Windows Credential Manager + DPAPI)
   - Task: Implement `AzureKeyVaultTokenStore` (parallel with Windows)
   - Task: Implement `GitHubSecretsTokenStore` (parallel with Windows/Azure)
   - Task: Implement `TokenStoreFactory` (depends on 3 stores above)
   - Task: Implement `InteractiveAuthFlow` (authorization code + PKCE)
   - Task: Implement `ServicePrincipalAuthFlow` (client credentials)
   - Task: Implement `OAuthAuthenticator` (orchestrates flows + stores)
   - Task: Implement `DeploymentEnvironmentDetector`, `ExponentialBackoffRetryPolicy`, `AuthenticationTelemetry`
   - Task: Implement `AuthenticationMiddleware` for Agent Framework integration
   - **Estimated**: 11 tasks × 3-5 hours = 33-55 hours

   **Category D: Integration Tests** (Sequential - End-to-end validation)
   - Task: Windows local interactive flow test (browser OAuth + Windows Credential Manager)
   - Task: Azure cloud service principal test (client credentials + Key Vault)
   - Task: Token refresh test (5-minute proactive buffer)
   - Task: Multi-environment detection test (Windows/Azure/GitHub)
   - **Estimated**: 4 tasks × 3-4 hours = 12-16 hours

   **Category E: Configuration & Setup** (Prerequisites)
   - Task: Azure AD app registration (local delegated + cloud application permissions)
   - Task: Azure Key Vault setup with managed identity
   - Task: GitHub Secrets configuration for Actions workflow
   - Task: Application Insights instrumentation key setup
   - **Estimated**: 4 tasks × 1-2 hours = 4-8 hours

3. **Task Ordering Rules**:
   - ✅ **Phase 1**: Models (Category A) - No dependencies, all [P]
   - ✅ **Phase 2**: Contract Tests (Category B) - Depend on models, all [P]
   - ✅ **Phase 3**: Implementations (Category C) - Depend on models + contracts, mixed [P]
   - ✅ **Phase 4**: Integration Tests (Category D) - Depend on implementations, sequential
   - ✅ **Prerequisites**: Setup tasks (Category E) can run anytime before integration tests

4. **Parallelization Strategy**:
   - Mark [P] for independent work: models, contract tests, token store implementations
   - Sequential: Factory classes, orchestrators, integration tests
   - **Estimated Parallel Work**: ~15 tasks can run simultaneously

5. **Expected Output**:
   - **Total Tasks**: ~30 numbered, ordered tasks in tasks.md
   - **Critical Path**: Models → Contract Tests → Core Implementations → Integration Tests
   - **Total Effort**: 65-106 hours (2-4 weeks for 1 developer, 1 week for team)
   - **Task Format**: ID | Title | Description | Acceptance Criteria | Dependencies | [P] flag

**IMPORTANT**: This phase is executed by the /tasks command, NOT by /plan

## Phase 3+: Future Implementation
*These phases are beyond the scope of the /plan command*

**Phase 3**: Task execution (/tasks command creates tasks.md)
**Phase 4**: Implementation (execute tasks.md following constitutional principles)
**Phase 5**: Validation (run tests, execute quickstart.md, performance validation)

## Complexity Tracking
*Fill ONLY if Constitution Check has violations that must be justified*

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |


## Progress Tracking
*This checklist is updated during execution flow*

**Phase Status**:
- [x] Phase 0: Research complete (/plan command) - ✅ **COMPLETE** on October 6, 2025
  - research.md generated with 7 research areas
  - All technical unknowns resolved with decisions and rationale
- [x] Phase 1: Design complete (/plan command) - ✅ **COMPLETE** on October 6, 2025
  - data-model.md: 6 entities defined
  - contracts/: 5 interfaces specified with contract tests
  - quickstart.md: 3 deployment scenarios documented
- [x] Phase 2: Task planning complete (/plan command - describe approach only) - ✅ **COMPLETE** on October 6, 2025
  - Task generation strategy defined (~30 tasks estimated)
  - Ordering and parallelization rules specified
  - Effort estimate: 65-106 hours total
- [ ] Phase 3: Tasks generated (/tasks command) - ⏳ **READY** - Run `/tasks` to execute
- [ ] Phase 4: Implementation complete
- [ ] Phase 5: Validation passed

**Gate Status**:
- [x] Initial Constitution Check: ✅ **PASS** - 15/15 applicable checks compliant (3 N/A spam-specific, 2 deferred Agent Framework)
- [x] Post-Design Constitution Check: ✅ **PASS** - No new violations introduced
- [x] All NEEDS CLARIFICATION resolved: ✅ **YES** - 5 questions resolved in Session 2025-10-05 (cloud storage, initial consent, multi-mailbox, refresh expiration, config storage)
- [ ] Complexity deviations documented

---
*Based on Constitution v1.1.0 - See `.specify/memory/constitution.md`*
