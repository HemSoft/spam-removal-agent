# Spam Removal Agent

An autonomous .NET 10 C# console agent that connects to Outlook mailboxes via Microsoft Graph and intelligently removes spam emails using the Microsoft Agent Framework.

## Overview

The Spam Removal Agent is designed for unattended, continuous operation with conservative spam detection to ensure legitimate emails are never deleted. It prioritizes false negatives over false positpositives with a strict ≥90% confidence threshold.

## Key Features

- **Conservative Spam Detection**: ≥90% confidence threshold required for deletion
- **Autonomous Operation**: Runs continuously as a background service
- **Microsoft Graph Integration**: Secure OAuth 2.0 authentication with minimal permissions (Mail.ReadWrite)
- **Multi-layer Validation**: Sender reputation, content analysis, and pattern matching
- **Comprehensive Logging**: Structured logging with Application Insights telemetry
- **.NET Aspire Orchestration**: Development-time observability dashboard
- **Graceful Error Handling**: Resilient operation with exponential backoff and circuit breaker patterns

## Technology Stack

- **.NET 10** - Latest C# features
- **Microsoft Agent Framework** - `Microsoft.Agents.AI` for autonomous operations
- **Microsoft Graph .NET SDK** - Secure Outlook mailbox access
- **.NET Aspire** - Orchestration and observability
- **Application Insights** - Production telemetry and monitoring
- **xUnit** - Test-driven development

## Implementation Status

### ✅ Feature Complete: OAuth 2.0 Authentication

The authentication system is **production ready** with comprehensive testing:

- ✅ Multi-environment support (Windows/Azure/GitHub Actions)
- ✅ Interactive PKCE and Service Principal flows
- ✅ Secure token storage (Windows Credential Manager, Azure Key Vault)
- ✅ Proactive token refresh with 5-minute buffer
- ✅ Retry logic and throttling middleware
- ✅ Application Insights telemetry integration
- ✅ 38 tests passing (100% pass rate)
- ✅ Zero build warnings

**See**: `specs/002-oauth-2-0/FEATURE_COMPLETE.md` for details

### 🚧 In Progress: Microsoft Graph Email Processing

Next feature specification in development.

## Project Structure

```
src/
├── SpamRemovalAgent/              # Main console application
│   ├── Agents/                    # AIAgent implementations
│   ├── Services/                  # Graph, spam detection, rule engine
│   ├── Authentication/            # OAuth 2.0 (✅ COMPLETE)
│   ├── Hosting/                   # IHostedService implementation
│   ├── Observability/             # Application Insights telemetry
│   └── Models/                    # Domain models
├── SpamRemovalAgent.AppHost/      # .NET Aspire orchestration
└── tests/
    └── SpamRemovalAgent.Tests/    # Unit, integration, agent tests (38 passing)
```

## Development Methodology

This project follows **Spec-Driven Development (SDD)** using [GitHub Spec Kit](https://github.com/github/spec-kit), where specifications in `.specify/memory/specs/` drive implementation.

## Getting Started

### Prerequisites

- .NET 10 SDK
- Visual Studio 2025 or VS Code
- Azure AD App Registration (for Microsoft Graph access)
- Windows 10+ (deployment target)

### Setup

1. Clone the repository
2. Configure OAuth credentials in Windows Credential Manager
3. Update `appsettings.json` with your Azure AD app details
4. Build the solution: `dotnet build`
5. Run tests: `dotnet test`
6. Launch with Aspire: `dotnet run --project src/SpamRemovalAgent.AppHost`

## Constitutional Principles

All development must comply with `.specify/memory/constitution.md`:

- **Conservative deletion** - Never delete legitimate emails
- **Microsoft Graph only** - No IMAP/POP3
- **Agent Framework integration** - Use `AIAgent` interface
- **Comprehensive logging** - Log all decisions with reasoning

## Security

- OAuth tokens stored in Windows Credential Manager (never plaintext)
- Minimal permissions: `Mail.ReadWrite` only
- Local processing - no external service transmission
- No sensitive data in logs

## Performance

- **Target**: 100+ emails/minute
- **Rate limiting**: Respects Graph API quotas
- **Retry logic**: Exponential backoff (2s → 32s max)
- **Circuit breaker**: Graceful degradation on failures

## License

[Add your license here]

## Contributing

This project follows spec-driven development. See `.specify/memory/project-docs/spec-driven-development.md` for the workflow.
