# Data Model: Project Structure Scaffolding

**Feature**: 001-scaffold-out-the | **Date**: 2025-10-05

## Overview
This document defines the entities and relationships for the project scaffolding feature. Since this is an infrastructure feature (creating projects and folders), the "data model" represents the file system artifacts and their relationships rather than runtime domain entities.

---

## Entities

### 1. Solution File
**Entity**: `SpamRemovalAgent.sln`
**Type**: File System Artifact
**Location**: Repository root (`F:\github\HemSoft\spam-removal-agent\`)

**Properties**:
- **Name**: `SpamRemovalAgent`
- **Format**: XML-based .NET solution file
- **Schema Version**: Visual Studio 2022 / .NET 10
- **Project References**: List of contained projects with GUIDs and paths

**Relationships**:
- CONTAINS → Main Console Project
- CONTAINS → Aspire AppHost Project
- CONTAINS → Test Project

**Validation Rules**:
- MUST exist at repository root
- MUST reference all created projects
- MUST use absolute or repository-relative paths
- MUST be buildable with `dotnet build`

**State Transitions**:
- None (immutable after creation until projects added/removed)

---

### 2. Main Console Project
**Entity**: `SpamRemovalAgent` (console application)
**Type**: .NET Project
**Location**: `src/SpamRemovalAgent/`

**Properties**:
- **ProjectFile**: `SpamRemovalAgent.csproj`
- **TargetFramework**: `net10.0`
- **OutputType**: `Exe` (console application)
- **RootNamespace**: `SpamRemovalAgent`
- **LangVersion**: `latest`
- **Nullable**: `enable`
- **ImplicitUsings**: `enable`

**Dependencies**:
- `Microsoft.Extensions.Hosting` (≥10.0.0)
- `Microsoft.Extensions.Logging` (≥10.0.0)
- `Microsoft.Extensions.Configuration.Json` (≥10.0.0)

**Folder Structure** (child folders):
- `Agents/` - AIAgent implementations
- `Services/` - Business logic services
- `Authentication/` - OAuth and token management
- `Hosting/` - IHostedService implementations
- `Observability/` - Application Insights integration
- `Models/` - Domain entities

**Files**:
- `Program.cs` - Application entry point with host builder
- `appsettings.json` - Configuration with placeholders

**Relationships**:
- CONTAINED_BY → Solution File
- REFERENCED_BY → Aspire AppHost Project
- REFERENCED_BY → Test Project

**Validation Rules**:
- MUST target .NET 10
- MUST have OutputType=Exe
- MUST include Microsoft.Extensions.Hosting
- MUST be runnable with `dotnet run`
- MUST output "Spam Removal Agent v1.0.0" on execution

**State Transitions**:
- Created → Buildable → Runnable

---

### 3. Aspire AppHost Project
**Entity**: `SpamRemovalAgent.AppHost` (orchestration)
**Type**: .NET Aspire Project
**Location**: `src/SpamRemovalAgent.AppHost/`

**Properties**:
- **ProjectFile**: `SpamRemovalAgent.AppHost.csproj`
- **TargetFramework**: `net10.0`
- **OutputType**: `Exe`
- **IsAspireHost**: `true`
- **RootNamespace**: `SpamRemovalAgent.AppHost`

**Dependencies**:
- `Aspire.Hosting.AppHost` (≥10.0.0)
- Project reference to Main Console Project

**Files**:
- `Program.cs` - Aspire dashboard and service registration
- `appsettings.json` - Aspire configuration

**Relationships**:
- CONTAINED_BY → Solution File
- REFERENCES → Main Console Project

**Validation Rules**:
- MUST reference Aspire.Hosting.AppHost SDK
- MUST reference Main Console Project
- MUST be runnable to launch Aspire dashboard
- MUST not be required for production deployment

**State Transitions**:
- Created → Buildable → Dashboard Launchable

---

### 4. Test Project
**Entity**: `SpamRemovalAgent.Tests` (test suite)
**Type**: xUnit Test Project
**Location**: `tests/SpamRemovalAgent.Tests/`

**Properties**:
- **ProjectFile**: `SpamRemovalAgent.Tests.csproj`
- **TargetFramework**: `net10.0`
- **IsPackable**: `false`
- **RootNamespace**: `SpamRemovalAgent.Tests`

**Dependencies**:
- `xunit` (≥2.9.0)
- `xunit.runner.visualstudio` (≥2.8.0)
- `Microsoft.NET.Test.Sdk` (≥17.11.0)
- Project reference to Main Console Project

**Folder Structure** (child folders):
- `unit/` - Fast, isolated unit tests
- `integration/` - Tests with external dependencies
- `utilities/` - Test helpers, mocks, builders

**Relationships**:
- CONTAINED_BY → Solution File
- REFERENCES → Main Console Project

**Validation Rules**:
- MUST use xUnit framework
- MUST reference Main Console Project
- MUST be discoverable by `dotnet test`
- MUST pass with zero tests initially (no failures)

**State Transitions**:
- Created → Buildable → Testable

---

### 5. Folder Structure
**Entity**: Logical folder organization within projects
**Type**: File System Directories

**Main Project Folders**:

#### Agents/
- **Purpose**: AIAgent implementations and middleware
- **Future Content**: SpamDetectionAgent, logging middleware, rate limiting middleware
- **Initial State**: Empty directory

#### Services/
- **Purpose**: Business logic and external integrations
- **Future Content**: GraphService, SpamDetectionService, RuleEngine
- **Initial State**: Empty directory

#### Authentication/
- **Purpose**: OAuth token management and secure storage
- **Future Content**: TokenManager, CredentialStore
- **Initial State**: Empty directory

#### Hosting/
- **Purpose**: IHostedService implementations for background execution
- **Future Content**: SpamRemovalAgentService
- **Initial State**: Empty directory

#### Observability/
- **Purpose**: Application Insights and telemetry
- **Future Content**: TelemetryInitializer, custom metrics
- **Initial State**: Empty directory

#### Models/
- **Purpose**: Domain entities
- **Future Content**: Email, SpamClassification, DetectionRule
- **Initial State**: Empty directory

**Test Project Folders**:

#### unit/
- **Purpose**: Fast, isolated tests with no external dependencies
- **Future Content**: SpamDetectionService tests, rule evaluation tests
- **Initial State**: Empty directory

#### integration/
- **Purpose**: Tests with external dependencies (Graph API, auth)
- **Future Content**: GraphService tests, OAuth flow tests
- **Initial State**: Empty directory

#### utilities/
- **Purpose**: Shared test infrastructure
- **Future Content**: TestEmailBuilder, MockGraphClient, test data fixtures
- **Initial State**: Empty directory

**Validation Rules**:
- All folders MUST exist after scaffolding
- Folders MAY be empty initially
- Folder names MUST match documented architecture

**Relationships**:
- CONTAINED_BY → Main Console Project (for Agents/, Services/, etc.)
- CONTAINED_BY → Test Project (for unit/, integration/, utilities/)

---

### 6. Configuration Files
**Entity**: Application configuration artifacts
**Type**: JSON Configuration Files

#### Main Project: appsettings.json
**Location**: `src/SpamRemovalAgent/appsettings.json`

**Schema**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "SpamRemovalAgent": {
    "Version": "1.0.0"
  }
}
```

**Validation Rules**:
- MUST be valid JSON
- MUST NOT contain secrets or credentials
- MUST include Logging configuration
- MUST include Version property

#### AppHost Project: appsettings.json
**Location**: `src/SpamRemovalAgent.AppHost/appsettings.json`

**Schema**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Aspire": "Information"
    }
  }
}
```

**Validation Rules**:
- MUST be valid JSON
- MUST configure Aspire logging levels

**Relationships**:
- USED_BY → Main Console Project / AppHost Project at runtime

---

### 7. Entry Point Files
**Entity**: Application entry points (Program.cs files)
**Type**: C# Source Files

#### Main Project: Program.cs
**Location**: `src/SpamRemovalAgent/Program.cs`

**Responsibilities**:
- Configure dependency injection
- Register hosted services
- Configure logging
- Build and run host

**Structure**:
```csharp
// Top-level statements
using SpamRemovalAgent.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Service registrations
builder.Services.AddHostedService<SpamRemovalAgentService>();

var host = builder.Build();
await host.RunAsync();
```

**Validation Rules**:
- MUST use top-level statements
- MUST create and run IHost
- MUST register at least one IHostedService
- MUST compile without errors

#### AppHost Project: Program.cs
**Location**: `src/SpamRemovalAgent.AppHost/Program.cs`

**Responsibilities**:
- Configure Aspire dashboard
- Register application projects
- Configure service orchestration

**Structure**:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

var spamAgent = builder.AddProject<Projects.SpamRemovalAgent>("spam-agent");

builder.Build().Run();
```

**Validation Rules**:
- MUST use Aspire DistributedApplication builder
- MUST reference main console project
- MUST compile without errors

**Relationships**:
- ENTRY_POINT_FOR → Main Console Project / AppHost Project

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    SpamRemovalAgent.sln                      │
│                      (Solution File)                         │
└────────────┬────────────────────────────┬───────────────────┘
             │ CONTAINS                   │ CONTAINS
             │                            │
             ▼                            ▼
┌─────────────────────────┐   ┌──────────────────────────────┐
│   SpamRemovalAgent      │   │ SpamRemovalAgent.AppHost     │
│   (Console Project)     │◄──│   (Aspire Project)           │
└─────────┬───────────────┘   └──────────────────────────────┘
          │ CONTAINS                      REFERENCES
          │
          ▼
┌─────────────────────────┐
│  Folder Structure       │
│  - Agents/              │
│  - Services/            │
│  - Authentication/      │
│  - Hosting/             │
│  - Observability/       │
│  - Models/              │
└─────────────────────────┘

             │ CONTAINS (from Solution)
             │
             ▼
┌─────────────────────────────────────────┐
│     SpamRemovalAgent.Tests              │
│        (Test Project)                   │
└─────────┬───────────────────────────────┘
          │ CONTAINS                REFERENCES Main Project
          │
          ▼
┌─────────────────────────┐
│  Test Folder Structure  │
│  - unit/                │
│  - integration/         │
│  - utilities/           │
└─────────────────────────┘
```

---

## Scaffolding Workflow Entity States

### Prerequisite Verification Phase
**State**: Pre-Scaffold
- .NET 10 SDK: Must be INSTALLED
- Conflicting files: Must be ABSENT

### Creation Phase
**State**: Scaffolding
1. Solution File: CREATING
2. Main Console Project: CREATING
3. Aspire AppHost Project: CREATING
4. Test Project: CREATING
5. Folders: CREATING
6. Configuration Files: CREATING

### Validation Phase
**State**: Post-Scaffold
1. Solution File: CREATED, VALID
2. Main Console Project: BUILDABLE, RUNNABLE
3. Aspire AppHost Project: BUILDABLE, RUNNABLE
4. Test Project: BUILDABLE, TESTABLE
5. Folders: EXIST
6. Configuration Files: VALID_JSON

---

## Invariants (Must Always Be True)

1. **Solution Integrity**: Solution file MUST always reference exactly the projects it contains
2. **Build Success**: All projects MUST build without errors on clean build
3. **Folder Completeness**: All documented folders MUST exist after scaffolding
4. **Test Project References**: Test project MUST reference main console project
5. **Aspire References**: AppHost MUST reference main console project
6. **Configuration Validity**: All JSON configuration files MUST be valid JSON
7. **No Secrets**: Configuration files MUST NOT contain credentials or secrets
8. **Framework Consistency**: All projects MUST target net10.0
9. **Entry Point Validity**: Program.cs files MUST compile and execute
10. **Constitutional Alignment**: Folder structure MUST match `.github/copilot-instructions.md` architecture

---

## Phase 1 Completion Checklist

- [x] Solution File entity defined with properties, relationships, validation
- [x] Main Console Project entity defined with dependencies and structure
- [x] Aspire AppHost Project entity defined with Aspire-specific configuration
- [x] Test Project entity defined with xUnit dependencies
- [x] Folder Structure entities defined with purposes and future content
- [x] Configuration Files entities defined with schemas and validation
- [x] Entry Point Files entities defined with structures and responsibilities
- [x] Entity Relationship Diagram created
- [x] Scaffolding workflow states documented
- [x] Invariants identified and documented
- [x] All entities trace to functional requirements in spec.md
- [x] All relationships explicit and validated

**Status**: ✅ Data Model Complete - Ready for Contract Generation

---

*Data model completed: 2025-10-05*
*Next: Generate contracts/ directory with file system validation schemas*
