# Research: Project Structure Scaffolding

**Feature**: 001-scaffold-out-the | **Date**: 2025-10-05

## Research Summary
This document consolidates technical research for scaffolding the Spam Removal Agent project structure. All technical decisions are based on .NET 10 best practices, constitutional requirements, and modern enterprise console application patterns.

---

## Research Areas

### 1. .NET 10 Solution Structure Best Practices

**Decision**: Multi-project solution with `src/` and `tests/` directories

**Rationale**:
- **Industry Standard**: Microsoft and .NET community widely adopt `src/` and `tests/` separation for clarity
- **Scalability**: Supports multiple projects (main app, Aspire host, future libraries) without cluttering root
- **Build Isolation**: Clear separation between production code and test code
- **Aspire Integration**: .NET Aspire AppHost projects are conventionally placed in `src/` alongside application projects

**Alternatives Considered**:
- **Flat structure** (projects at root): Clutters repository root, harder to navigate with multiple projects
- **Apps/libs separation**: Overly complex for this project scope (no shared libraries yet)
- **Tests inside src**: Violates separation of concerns, complicates deployment exclusions

**References**:
- Microsoft Docs: [.NET project structure guidelines](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- .NET Aspire samples: https://github.com/dotnet/aspire-samples

---

### 2. .NET Aspire Integration Pattern

**Decision**: Separate AppHost project that references main console application

**Rationale**:
- **Orchestration**: Aspire AppHost orchestrates service startup and provides unified dashboard
- **Development Observability**: Aspire dashboard shows logs, traces, and metrics in real-time during development
- **Zero-downtime Restart**: Aspire can restart services without rebuilding entire solution
- **Production Independence**: AppHost is dev-only - main console app runs independently in production
- **Constitutional Compliance**: Satisfies "Technology Stack Requirements" mandate for Aspire integration

**Alternatives Considered**:
- **No Aspire**: Would lose development-time observability dashboard and require manual service management
- **Aspire SDK in main project**: AppHost pattern is cleaner - keeps orchestration separate from business logic
- **Docker Compose**: Aspire provides .NET-native experience with better Visual Studio integration

**Implementation Details**:
- AppHost references main console project using `AddProject<>`
- AppHost includes Aspire.Hosting.AppHost SDK
- Main project remains independent console application
- Configuration flows from AppHost to services via environment variables

**References**:
- .NET Aspire documentation: https://learn.microsoft.com/en-us/dotnet/aspire/
- Aspire AppHost patterns: https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/app-host-overview

---

### 3. Console Application Hosting Pattern

**Decision**: Use `Microsoft.Extensions.Hosting` with `IHostedService` pattern

**Rationale**:
- **Modern .NET Pattern**: Recommended approach for long-running console applications
- **Dependency Injection**: Built-in DI container for service registration
- **Graceful Shutdown**: Handles SIGTERM/SIGINT for clean termination
- **Logging Infrastructure**: Integrated `ILogger<T>` support for structured logging
- **Background Execution**: `IHostedService` enables autonomous agent operation
- **Constitutional Compliance**: Supports "Autonomous Operation" and "Agent Framework Architecture" requirements

**Alternatives Considered**:
- **Plain Console.ReadLine loop**: No graceful shutdown, manual DI setup, harder to test
- **Windows Service**: Overcomplicates initial scaffolding (can be added later)
- **Top-level statements only**: No hosting infrastructure for background services

**Implementation Pattern**:
```csharp
// Program.cs
var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<SpamRemovalAgentService>();
// Register other services...
var host = builder.Build();
await host.RunAsync();
```

**References**:
- Microsoft Docs: [.NET Generic Host](https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host)
- Background services: https://learn.microsoft.com/en-us/dotnet/core/extensions/hosted-services

---

### 4. Folder Structure Organization

**Decision**: Vertical slice folders (Agents, Services, Authentication, Hosting, Observability, Models)

**Rationale**:
- **Constitutional Alignment**: Directly maps to architecture in `.github/copilot-instructions.md`
- **Feature Cohesion**: Each folder represents a distinct concern in the spam removal workflow
- **Discoverability**: Developers immediately understand where to place new code
- **Separation of Concerns**: Clear boundaries between agent logic, services, auth, hosting, telemetry
- **Future-Proof**: Supports growth without restructuring (e.g., add new agents, services)

**Alternatives Considered**:
- **Flat structure**: Doesn't scale beyond trivial applications
- **Feature folders** (e.g., SpamDetection/, EmailProcessing/): Premature for scaffolding phase
- **Clean Architecture layers** (Domain/, Application/, Infrastructure/): Too complex for console agent

**Folder Purposes**:
- `Agents/`: AIAgent implementations and middleware (Microsoft Agent Framework)
- `Services/`: Business logic (GraphService, SpamDetectionService, RuleEngine)
- `Authentication/`: OAuth token management, Windows Credential Manager integration
- `Hosting/`: IHostedService implementations for background execution
- `Observability/`: Application Insights setup, custom telemetry, logging configuration
- `Models/`: Domain entities (Email, SpamClassification, DetectionRule)

---

### 5. Test Framework Selection

**Decision**: xUnit as primary test framework

**Rationale** (from spec clarification):
- **Modern Framework**: xUnit is the most modern .NET test framework with excellent async/await support
- **Async-First**: Native support for `async Task` test methods (critical for Graph API integration tests)
- **Parallel Execution**: Runs tests in parallel by default for faster feedback
- **Community Adoption**: Widely used in .NET ecosystem including Microsoft's own projects
- **Extensibility**: Easy to create custom test fixtures and data sources
- **Theory Support**: Parameterized tests via `[Theory]` and `[InlineData]` for comprehensive coverage

**Alternatives Considered** (from clarification session):
- **NUnit**: Older framework, less async-friendly, requires more setup for parallel execution
- **MSTest**: Microsoft's framework but less feature-rich than xUnit, slower evolution

**Package Requirements**:
- `xunit` - Core framework
- `xunit.runner.visualstudio` - Visual Studio Test Explorer integration
- `Microsoft.NET.Test.Sdk` - .NET test hosting

**Test Organization**:
- `unit/`: Fast, isolated tests for spam detection logic, rule evaluation, confidence calculation
- `integration/`: Slower tests with external dependencies (Graph API, OAuth)
- `utilities/`: Shared test helpers, mocks, test data builders

**References**:
- xUnit documentation: https://xunit.net/
- Async testing best practices: https://xunit.net/docs/comparisons

---

### 6. Project File Configuration

**Decision**: .NET 10 target framework with modern C# features enabled

**Configuration Details**:
```xml
<TargetFramework>net10.0</TargetFramework>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

**Rationale**:
- **Constitutional Requirement**: ".NET 10 C# Console Application"
- **Modern Language**: Latest C# version for top-level statements, records, pattern matching
- **Null Safety**: Nullable reference types prevent null reference exceptions
- **Implicit Usings**: Reduces boilerplate for common namespaces
- **Performance**: .NET 10 includes performance improvements over previous versions

**Alternatives Considered**:
- **.NET 8 LTS**: Would miss .NET 10's performance and language improvements
- **Disabled nullable**: Would lose compile-time null safety benefits

---

### 7. Configuration Management

**Decision**: appsettings.json with placeholder values (no secrets)

**Rationale**:
- **.NET Convention**: Standard configuration pattern using `Microsoft.Extensions.Configuration`
- **Environment Overrides**: appsettings.Development.json and environment variables for local dev
- **Security**: No credentials stored in config files (use Windows Credential Manager per constitution)
- **Future Extension**: Ready for Application Insights connection strings, logging levels, feature flags

**Placeholder Structure**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  },
  "SpamRemovalAgent": {
    "Version": "1.0.0"
  }
}
```

**Alternatives Considered**:
- **Hardcoded values**: Not configurable, violates security requirements
- **Environment variables only**: Harder to document and discover configuration options
- **User secrets**: Appropriate for future auth configuration (not needed at scaffolding)

---

### 8. Prerequisite Verification

**Decision**: Scaffolding script must verify .NET 10 SDK before creating projects

**Rationale**:
- **Fail Fast**: Prevents confusing errors during `dotnet new` execution
- **Clear Messaging**: User knows exactly what's missing (SDK version)
- **Constitutional Compliance**: Error handling requirement from spec (FR-023)

**Verification Command**:
```bash
dotnet --list-sdks | grep "10\."
```

**Error Message Format**:
```
ERROR: .NET 10 SDK not found.
Required: .NET 10.0.x or higher
Installed SDKs: [list from dotnet --list-sdks]
Download: https://dotnet.microsoft.com/download/dotnet/10.0
```

---

### 9. Conflict Detection Strategy

**Decision**: Check for existing files before creating any projects

**Rationale**:
- **Constitutional Compliance**: Spec requirement (FR-021, FR-022) - fail immediately on conflicts
- **Clean State**: Prevents partial scaffolding that leaves repository in inconsistent state
- **Explicit Control**: Forces developer to consciously resolve conflicts

**Conflict Check Files**:
- `SpamRemovalAgent.sln`
- `src/SpamRemovalAgent/SpamRemovalAgent.csproj`
- `src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj`
- `tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj`

**Error Behavior**:
```
ERROR: Conflicting files detected. Manual cleanup required.
Conflicts:
  - SpamRemovalAgent.sln
  - src/SpamRemovalAgent/SpamRemovalAgent.csproj
Action: Remove or rename conflicting files before re-running scaffolding.
```

---

### 10. Initial Program.cs Output

**Decision**: Display "Spam Removal Agent v1.0.0" via logging infrastructure on startup

**Rationale** (from spec clarification):
- **Verification**: Confirms application executes successfully after scaffolding
- **Logging Infrastructure Test**: Validates ILogger<T> configuration works
- **Version Visibility**: Shows version number for future diagnostics
- **Constitutional Compliance**: Satisfies FR-013 requirement

**Implementation Pattern**:
```csharp
// Program.cs hosted service startup
public class SpamRemovalAgentService : IHostedService
{
    private readonly ILogger<SpamRemovalAgentService> _logger;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Spam Removal Agent v1.0.0");
        // Future: Agent initialization
    }
}
```

**Alternatives Considered**:
- **Console.WriteLine**: Bypasses logging infrastructure, not testable
- **No output**: Harder to verify scaffolding success

---

## Dependencies Summary

### Main Console Application
- `Microsoft.Extensions.Hosting` - Generic host and IHostedService
- `Microsoft.Extensions.Logging` - Structured logging
- `Microsoft.Extensions.Configuration.Json` - JSON configuration support

### Aspire AppHost
- `Aspire.Hosting.AppHost` - Aspire orchestration SDK

### Test Project
- `xunit` - Test framework
- `xunit.runner.visualstudio` - Visual Studio integration
- `Microsoft.NET.Test.Sdk` - Test hosting
- Project reference to main console application

### Future Dependencies (Not in Scaffolding)
- `Microsoft.Graph` - Graph API client (future feature)
- `Microsoft.Agents.AI` - Agent Framework (future feature)
- `Azure.Monitor.OpenTelemetry.AspNetCore` - Application Insights (future feature)

---

## Build and Execution Verification

### Build Verification Commands
```bash
# Build entire solution
dotnet build SpamRemovalAgent.sln

# Build specific projects
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
dotnet build tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

### Execution Verification
```bash
# Run main application
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
# Expected output: "info: SpamRemovalAgentService[0] Spam Removal Agent v1.0.0"

# Run via Aspire AppHost (with dashboard)
dotnet run --project src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
# Opens Aspire dashboard in browser
```

### Test Verification
```bash
# Run all tests
dotnet test SpamRemovalAgent.sln

# Run specific test project
dotnet test tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

---

## Constitutional Alignment Verification

| Constitutional Principle | Scaffolding Support | Evidence |
|-------------------------|-------------------|----------|
| Autonomous Operation | ✅ | Hosting/ folder + IHostedService pattern |
| Microsoft Graph Integration | ✅ | Services/ folder prepared for GraphService |
| Agent Framework Architecture | ✅ | Agents/ folder for AIAgent implementations |
| Conservative Spam Detection | ✅ | Services/ folder for detection logic |
| Comprehensive Logging | ✅ | Observability/ folder + ILogger in Program.cs |
| Technology Stack | ✅ | .NET 10 + Aspire AppHost + planned App Insights |
| Security | ✅ | Authentication/ folder for token management |
| Quality Standards | ✅ | Test project structure with unit/integration/utilities |
| Spec-Driven Development | ✅ | Aligns with documented architecture in copilot instructions |

---

## Phase 0 Completion Checklist

- [x] Solution structure pattern researched and decided
- [x] Aspire integration pattern researched and decided
- [x] Console hosting pattern researched and decided
- [x] Folder organization researched and decided
- [x] Test framework researched and decided (xUnit from clarification)
- [x] Project file configuration researched and decided
- [x] Configuration management researched and decided
- [x] Prerequisite verification strategy researched and decided
- [x] Conflict detection strategy researched and decided
- [x] Initial output requirement researched and decided (from clarification)
- [x] Dependencies identified and documented
- [x] Build/execution verification approach defined
- [x] Constitutional alignment verified
- [x] All technical unknowns resolved

**Status**: ✅ Phase 0 Research Complete - Ready for Phase 1 Design

---

*Research completed: 2025-10-05*
*Next phase: Design & Contracts (data-model.md, contracts/, quickstart.md)*
