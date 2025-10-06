# Quickstart: Project Structure Scaffolding

**Feature**: 001-scaffold-out-the | **Date**: 2025-10-05
**Estimated Time**: 5-10 minutes

## Overview
This quickstart guide walks you through scaffolding the complete .NET 10 project structure for the Spam Removal Agent application. By the end, you'll have a buildable solution with a main console application, Aspire AppHost for development observability, and a test project structure.

---

## Prerequisites

### Required
- ✅ **Windows 10** or later
- ✅ **.NET 10 SDK** installed ([Download](https://dotnet.microsoft.com/download/dotnet/10.0))
- ✅ **Git** (repository already cloned to `F:\github\HemSoft\spam-removal-agent`)

### Recommended
- ✅ **Visual Studio 2022** (17.11+) or **Visual Studio Code** with C# Dev Kit
- ✅ **PowerShell 7+** (for running scaffolding scripts)

### Verify Prerequisites
```bash
# Check .NET 10 SDK installation
dotnet --list-sdks

# Expected output includes:
# 10.0.xxx [...path...]

# Check Git
git --version

# Check current directory
pwd
# Expected: F:\github\HemSoft\spam-removal-agent
```

---

## Step 1: Verify Clean State

**Objective**: Ensure no conflicting files exist before scaffolding.

```bash
# Navigate to repository root (if not already there)
cd /f/github/HemSoft/spam-removal-agent

# Check for conflicting files
ls SpamRemovalAgent.sln 2>/dev/null || echo "✅ No solution file conflict"
ls src/SpamRemovalAgent/SpamRemovalAgent.csproj 2>/dev/null || echo "✅ No main project conflict"
ls src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj 2>/dev/null || echo "✅ No AppHost conflict"
ls tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj 2>/dev/null || echo "✅ No test project conflict"
```

**Expected Result**: All checks should output "✅ No ... conflict"

**If Conflicts Exist**:
```bash
# Manual cleanup required (example - adjust as needed)
rm SpamRemovalAgent.sln
rm -rf src/SpamRemovalAgent
rm -rf src/SpamRemovalAgent.AppHost
rm -rf tests/SpamRemovalAgent.Tests
```

---

## Step 2: Create Solution File

**Objective**: Create the top-level solution file that will contain all projects.

```bash
# Create solution at repository root
dotnet new sln --name SpamRemovalAgent

# Verify creation
ls SpamRemovalAgent.sln
```

**Expected Output**:
```
The template "Solution File" was created successfully.
```

**Verification**:
```bash
cat SpamRemovalAgent.sln | grep "Microsoft Visual Studio Solution File"
# Should output: Microsoft Visual Studio Solution File, Format Version 12.00
```

---

## Step 3: Create Directory Structure

**Objective**: Establish the folder structure for projects before creating them.

```bash
# Create source directories
mkdir -p src/SpamRemovalAgent
mkdir -p src/SpamRemovalAgent.AppHost

# Create test directories
mkdir -p tests/SpamRemovalAgent.Tests

# Verify directory creation
tree -d -L 3 src tests
```

**Expected Structure**:
```
src/
├── SpamRemovalAgent/
└── SpamRemovalAgent.AppHost/

tests/
└── SpamRemovalAgent.Tests/
```

---

## Step 4: Create Main Console Application Project

**Objective**: Create the primary console application with hosting infrastructure.

```bash
# Create console project
cd src/SpamRemovalAgent
dotnet new console --framework net10.0 --name SpamRemovalAgent --use-program-main false

# Return to repo root
cd ../..

# Add project to solution
dotnet sln add src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Expected Output**:
```
The template "Console App" was created successfully.
Project `src/SpamRemovalAgent/SpamRemovalAgent.csproj` added to the solution.
```

**Verification**:
```bash
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
# Should output: Build succeeded.
```

---

## Step 5: Configure Main Project Properties

**Objective**: Enable modern C# features and nullable reference types.

```bash
# Edit project file to add properties
# Open src/SpamRemovalAgent/SpamRemovalAgent.csproj in editor
```

**Add/Verify these properties in `<PropertyGroup>`**:
```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

**Add Package References**:
```bash
cd src/SpamRemovalAgent

# Add hosting support
dotnet add package Microsoft.Extensions.Hosting --version 10.0.0

# Add logging
dotnet add package Microsoft.Extensions.Logging --version 10.0.0

# Add JSON configuration
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0

cd ../..
```

**Verification**:
```bash
dotnet restore src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

---

## Step 6: Create Main Project Folder Structure

**Objective**: Establish the vertical slice folder organization.

```bash
# Create core folders
mkdir -p src/SpamRemovalAgent/Agents
mkdir -p src/SpamRemovalAgent/Services
mkdir -p src/SpamRemovalAgent/Authentication
mkdir -p src/SpamRemovalAgent/Hosting
mkdir -p src/SpamRemovalAgent/Observability
mkdir -p src/SpamRemovalAgent/Models

# Verify folder creation
tree -d -L 2 src/SpamRemovalAgent
```

**Expected Structure**:
```
src/SpamRemovalAgent/
├── Agents/
├── Services/
├── Authentication/
├── Hosting/
├── Observability/
└── Models/
```

---

## Step 7: Create Program.cs with Hosting Infrastructure

**Objective**: Set up the application entry point with dependency injection and hosting.

**Create/Replace `src/SpamRemovalAgent/Program.cs`**:
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.AddConsole();

// Register hosted service (will be implemented in future features)
// builder.Services.AddHostedService<SpamRemovalAgentService>();

// For now, add a simple startup service to output version
builder.Services.AddHostedService<StartupService>();

var host = builder.Build();
await host.RunAsync();

namespace HemSoft.SpamRemovalAgent;

// Temporary startup service to satisfy FR-013
public class StartupService : IHostedService
{
    private readonly ILogger<StartupService> _logger;
    private readonly IHostApplicationLifetime _lifetime;

    public StartupService(ILogger<StartupService> logger, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _lifetime = lifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Spam Removal Agent v1.0.0");

        // For scaffolding verification, exit immediately
        // In future features, this will be replaced with actual agent service
        _lifetime.StopApplication();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

**Verification**:
```bash
# Build
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj

# Run (should output version and exit)
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Expected Output**:
```
info: StartupService[0]
      Spam Removal Agent v1.0.0
```

---

## Step 8: Create Configuration File

**Objective**: Add application configuration with placeholders.

**Create `src/SpamRemovalAgent/appsettings.json`**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "SpamRemovalAgent": {
    "Version": "1.0.0",
    "Environment": "Development"
  }
}
```

**Update Project File to Include Config**:
```bash
# Edit src/SpamRemovalAgent/SpamRemovalAgent.csproj
# Add this ItemGroup:
```

```xml
<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

## Step 9: Create Aspire AppHost Project

**Objective**: Set up .NET Aspire orchestration for development observability.

```bash
# Create Aspire AppHost project
cd src/SpamRemovalAgent.AppHost
dotnet new aspire-apphost --framework net10.0 --name SpamRemovalAgent.AppHost

# Return to repo root
cd ../..

# Add to solution
dotnet sln add src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
```

**Expected Output**:
```
The template "Aspire Application Host" was created successfully.
Project `src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj` added to the solution.
```

---

## Step 10: Configure AppHost to Reference Main Project

**Objective**: Enable AppHost to orchestrate the main console application.

**Edit `src/SpamRemovalAgent.AppHost/Program.cs`**:
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add reference to main console application
var spamAgent = builder.AddProject<Projects.SpamRemovalAgent>("spam-agent");

builder.Build().Run();
```

**Add Project Reference**:
```bash
cd src/SpamRemovalAgent.AppHost
dotnet add reference ../SpamRemovalAgent/SpamRemovalAgent.csproj
cd ../..
```

**Verification**:
```bash
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
```

---

## Step 11: Create Test Project

**Objective**: Set up xUnit test project with proper structure.

```bash
# Create xUnit test project
cd tests/SpamRemovalAgent.Tests
dotnet new xunit --framework net10.0 --name SpamRemovalAgent.Tests

# Return to repo root
cd ../..

# Add to solution
dotnet sln add tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

**Expected Output**:
```
The template "xUnit Test Project" was created successfully.
Project `tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj` added to the solution.
```

---

## Step 12: Configure Test Project

**Objective**: Add project reference and configure test structure.

```bash
# Add reference to main project
cd tests/SpamRemovalAgent.Tests
dotnet add reference ../../src/SpamRemovalAgent/SpamRemovalAgent.csproj
cd ../..
```

**Create Test Folder Structure**:
```bash
# Create test organization folders
mkdir -p tests/SpamRemovalAgent.Tests/unit
mkdir -p tests/SpamRemovalAgent.Tests/integration
mkdir -p tests/SpamRemovalAgent.Tests/utilities

# Remove default test file (optional - can keep for reference)
rm tests/SpamRemovalAgent.Tests/UnitTest1.cs

# Verify structure
tree -d tests/SpamRemovalAgent.Tests
```

**Expected Structure**:
```
tests/SpamRemovalAgent.Tests/
├── unit/
├── integration/
└── utilities/
```

**Verification**:
```bash
dotnet build tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
dotnet test tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

**Expected Output**:
```
Test run for tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
Total tests: 0
```

---

## Step 13: Build and Verify Entire Solution

**Objective**: Ensure all projects build successfully together.

```bash
# Clean build
dotnet clean SpamRemovalAgent.sln
dotnet restore SpamRemovalAgent.sln
dotnet build SpamRemovalAgent.sln --configuration Release

# Verify build success
echo $?
# Should output: 0 (success)
```

**Expected Output**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## Step 14: Execute and Validate

**Objective**: Run the application and verify expected behavior.

### Test Main Application
```bash
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Expected Output**:
```
info: StartupService[0]
      Spam Removal Agent v1.0.0
```

### Test Aspire Dashboard (Manual)
```bash
dotnet run --project src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
```

**Expected Behavior**:
- Browser opens to Aspire dashboard
- Dashboard shows "spam-agent" service
- Service status visible in dashboard

**Note**: Press Ctrl+C to stop the dashboard.

### Test Suite Execution
```bash
dotnet test SpamRemovalAgent.sln --verbosity normal
```

**Expected Output**:
```
Test Run Successful.
Total tests: 0
```

---

## Step 15: Commit Scaffolding (Optional)

**Objective**: Commit the scaffolded structure to version control.

```bash
# Check git status
git status

# Stage scaffolded files
git add SpamRemovalAgent.sln
git add src/
git add tests/

# Review changes
git diff --cached

# Commit
git commit -m "feat: scaffold .NET 10 project structure with Aspire AppHost and test project

- Create solution with main console app, Aspire AppHost, and xUnit test project
- Configure hosting infrastructure with IHostedService pattern
- Establish vertical slice folder structure (Agents, Services, Authentication, Hosting, Observability, Models)
- Add test organization folders (unit, integration, utilities)
- Application outputs 'Spam Removal Agent v1.0.0' on execution

Satisfies FR-001 through FR-020 in spec.md"
```

---

## Verification Checklist

After completing all steps, verify the following:

### File System Structure
- [ ] ✅ `SpamRemovalAgent.sln` exists at repository root
- [ ] ✅ `src/SpamRemovalAgent/SpamRemovalAgent.csproj` exists
- [ ] ✅ `src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj` exists
- [ ] ✅ `tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj` exists
- [ ] ✅ All main project folders exist (Agents, Services, Authentication, Hosting, Observability, Models)
- [ ] ✅ All test folders exist (unit, integration, utilities)

### Build Success
- [ ] ✅ `dotnet build SpamRemovalAgent.sln` succeeds with 0 errors
- [ ] ✅ `dotnet build` succeeds for each individual project

### Execution Success
- [ ] ✅ Main application runs and outputs "Spam Removal Agent v1.0.0"
- [ ] ✅ Aspire AppHost launches dashboard (manual verification)
- [ ] ✅ `dotnet test` succeeds (0 tests, 0 failures)

### Configuration Validation
- [ ] ✅ `appsettings.json` is valid JSON
- [ ] ✅ Configuration contains Logging and Version sections
- [ ] ✅ No secrets in configuration files

### Constitutional Compliance
- [ ] ✅ Projects target .NET 10 (`net10.0`)
- [ ] ✅ Main project uses hosting infrastructure (IHostedService pattern)
- [ ] ✅ Aspire AppHost integrated for orchestration
- [ ] ✅ Test project uses xUnit framework
- [ ] ✅ Folder structure matches documented architecture

---

## Troubleshooting

### Issue: .NET 10 SDK Not Found
**Symptom**: `dotnet new` commands fail with "could not find SDK"

**Solution**:
```bash
# Download and install .NET 10 SDK
# Visit: https://dotnet.microsoft.com/download/dotnet/10.0

# After installation, verify:
dotnet --list-sdks | grep "10\."
```

### Issue: Build Fails with Package Restore Errors
**Symptom**: `dotnet build` fails with "package not found"

**Solution**:
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore SpamRemovalAgent.sln

# Retry build
dotnet build SpamRemovalAgent.sln
```

### Issue: Aspire Template Not Found
**Symptom**: `dotnet new aspire-apphost` fails

**Solution**:
```bash
# Install Aspire templates
dotnet new install Aspire.ProjectTemplates

# Retry AppHost creation
cd src/SpamRemovalAgent.AppHost
dotnet new aspire-apphost --framework net10.0 --name SpamRemovalAgent.AppHost
```

### Issue: Application Doesn't Output Version String
**Symptom**: `dotnet run` executes but no output visible

**Solution**:
```bash
# Check logging configuration in appsettings.json
# Ensure "Default" log level is "Information"

# Run with verbose logging
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj --verbosity detailed
```

### Issue: Conflict Detected During Scaffolding
**Symptom**: Files already exist error

**Solution**:
```bash
# List conflicting files
ls SpamRemovalAgent.sln src/SpamRemovalAgent/*.csproj 2>/dev/null

# Manual cleanup (BE CAREFUL - this deletes files)
rm -rf SpamRemovalAgent.sln src/ tests/

# Retry scaffolding from Step 2
```

---

## Next Steps

After completing this quickstart, you have successfully scaffolded the foundational project structure. Next steps:

1. **Review Architecture**: Familiarize yourself with the folder structure and purpose of each directory
2. **Run `/tasks` Command**: Generate the task list for implementing core agent functionality
3. **Implement Features**: Follow the Spec-Driven Development workflow for upcoming features:
   - OAuth authentication with Microsoft Graph
   - Agent Framework integration
   - Spam detection service
   - Email processing workflows

4. **Explore Aspire Dashboard**: Run the AppHost and explore the observability features
5. **Write First Test**: Add a simple unit test in `tests/unit/` to validate test infrastructure

---

## Success Criteria Met

Upon completion of this quickstart, you have satisfied the following functional requirements:

| Requirement | Status | Evidence |
|-------------|--------|----------|
| FR-001: Solution file created | ✅ | SpamRemovalAgent.sln exists and references all projects |
| FR-002: Main console project created | ✅ | src/SpamRemovalAgent/SpamRemovalAgent.csproj |
| FR-003: Test project structure created | ✅ | tests/SpamRemovalAgent.Tests with unit/integration/utilities |
| FR-004: .NET 10 target framework | ✅ | All projects target net10.0 |
| FR-005: Main project folder structure | ✅ | Agents, Services, Authentication, Hosting, Observability, Models |
| FR-006: Console application configuration | ✅ | OutputType=Exe in project file |
| FR-007: Entry point with DI and hosting | ✅ | Program.cs with Host.CreateApplicationBuilder |
| FR-008: Configuration files | ✅ | appsettings.json with placeholders |
| FR-009-011: Test project with xUnit | ✅ | xUnit packages, project reference, folder structure |
| FR-012: Solution buildable | ✅ | `dotnet build SpamRemovalAgent.sln` succeeds |
| FR-013: Application executable with output | ✅ | Outputs "Spam Removal Agent v1.0.0" |
| FR-014: Properly configured project files | ✅ | Correct SDK references and target frameworks |
| FR-015-017: Aligns with architecture docs | ✅ | Matches .github/copilot-instructions.md structure |
| FR-018-020: Aspire AppHost integration | ✅ | AppHost project references main console app |
| FR-021-023: Error handling | ✅ | Manual conflict detection in Step 1 |

**Result**: 🎉 **ALL FUNCTIONAL REQUIREMENTS SATISFIED**

---

*Quickstart completed: 2025-10-05*
*Estimated completion time: 5-10 minutes*
*Next: Run `/tasks` command to generate implementation task list*
