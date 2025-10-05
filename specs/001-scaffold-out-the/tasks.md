# Tasks: Project Structure Scaffolding

**Feature**: 001-scaffold-out-the | **Date**: 2025-10-05
**Input**: Design documents from `F:\github\HemSoft\spam-removal-agent\specs\001-scaffold-out-the\`
**Prerequisites**: plan.md, research.md, data-model.md, contracts/scaffolding-validation.md, quickstart.md

## Overview

This task list provides an executable implementation plan for scaffolding the complete .NET 10 project structure for the Spam Removal Agent. Unlike typical TDD-based feature development, this is **infrastructure scaffolding** that follows a linear build-test-validate pattern.

**Estimated Total Time**: 30-45 minutes (including manual verification steps)
**Total Tasks**: 22
**Parallel Tasks**: 7 (folder creation and some validations)

## Task Execution Notes

- **Linear Execution**: Most tasks must execute in order due to dependencies
- **Parallel Opportunities**: Marked with [P] - only folder creation and independent validations
- **Verification**: Each task includes explicit verification steps
- **Fail-Fast**: Stop execution immediately if any task fails
- **Constitutional Compliance**: All tasks align with `.specify/memory/constitution.md`

---

## Phase 1: Prerequisite Validation

### T001: Verify .NET 10 SDK Installation
**Depends on**: None
**Can run in parallel**: No
**Estimated time**: 1 minute
**File**: N/A (system prerequisite)

**Description**:
Verify that .NET 10 SDK is installed on the system before attempting any project creation. This satisfies FR-023 (error handling) and implements Contract 1 (PrerequisiteCheck).

**Steps**:
1. Run `dotnet --list-sdks` command
2. Parse output to check for SDK version matching pattern `10.`
3. If not found, display error message with download link
4. Exit with error code 1 if SDK missing

**Verification**:
```bash
dotnet --list-sdks | grep "10\."
# Expected: Output contains "10.0.xxx" or similar
```

**Error Message (if fails)**:
```
ERROR: .NET 10 SDK not found.
Required: .NET 10.0.x or higher
Installed SDKs: [list from dotnet --list-sdks]
Download: https://dotnet.microsoft.com/download/dotnet/10.0
```

**Success Criteria**:
- Command exits with code 0
- Output contains .NET 10 SDK version

---

### T002: Check for Conflicting Files
**Depends on**: T001
**Can run in parallel**: No
**Estimated time**: 1 minute
**Files to check**:
- `F:\github\HemSoft\spam-removal-agent\SpamRemovalAgent.sln`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\SpamRemovalAgent.csproj`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent.AppHost\SpamRemovalAgent.AppHost.csproj`
- `F:\github\HemSoft\spam-removal-agent\tests\SpamRemovalAgent.Tests\SpamRemovalAgent.Tests.csproj`

**Description**:
Detect any existing files that would conflict with scaffolding. This satisfies FR-021, FR-022 and implements Contract 2 (ConflictCheck). Must fail immediately if conflicts exist to prevent partial scaffolding.

**Steps**:
1. Check if `SpamRemovalAgent.sln` exists at repository root
2. Check if `src/SpamRemovalAgent/SpamRemovalAgent.csproj` exists
3. Check if `src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj` exists
4. Check if `tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj` exists
5. If ANY file exists, collect list and display error
6. Exit with error code 2 if conflicts found

**Verification**:
```bash
ls SpamRemovalAgent.sln 2>/dev/null && echo "CONFLICT" || echo "OK"
ls src/SpamRemovalAgent/SpamRemovalAgent.csproj 2>/dev/null && echo "CONFLICT" || echo "OK"
ls src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj 2>/dev/null && echo "CONFLICT" || echo "OK"
ls tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj 2>/dev/null && echo "CONFLICT" || echo "OK"
# All should output "OK"
```

**Error Message (if fails)**:
```
ERROR: Conflicting files detected. Manual cleanup required.
Conflicts:
  - [list detected files]
Action: Remove or rename conflicting files before re-running scaffolding.
```

**Success Criteria**:
- No conflicting files exist
- Task exits with code 0

---

## Phase 2: Solution and Project Creation

### T003: Create Solution File
**Depends on**: T002
**Can run in parallel**: No
**Estimated time**: 1 minute
**File**: `F:\github\HemSoft\spam-removal-agent\SpamRemovalAgent.sln`

**Description**:
Create the top-level .NET solution file that will contain all projects. This satisfies FR-001 and implements Entity 1 (Solution File) from data-model.md.

**Steps**:
1. Navigate to repository root: `F:\github\HemSoft\spam-removal-agent\`
2. Run `dotnet new sln --name SpamRemovalAgent`
3. Verify solution file created

**Commands**:
```bash
cd /f/github/HemSoft/spam-removal-agent
dotnet new sln --name SpamRemovalAgent
```

**Verification**:
```bash
ls SpamRemovalAgent.sln
cat SpamRemovalAgent.sln | grep "Microsoft Visual Studio Solution File"
# Expected: File exists and contains solution header
```

**Success Criteria**:
- `SpamRemovalAgent.sln` exists at repository root
- File contains "Microsoft Visual Studio Solution File, Format Version 12.00"

---

### T004: Create Directory Structure for Projects
**Depends on**: T003
**Can run in parallel**: No
**Estimated time**: 1 minute
**Directories**:
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent.AppHost\`
- `F:\github\HemSoft\spam-removal-agent\tests\SpamRemovalAgent.Tests\`

**Description**:
Create the directory structure for all projects before creating the projects themselves. This follows .NET best practices with `src/` and `tests/` separation as documented in research.md.

**Steps**:
1. Create `src/SpamRemovalAgent/` directory
2. Create `src/SpamRemovalAgent.AppHost/` directory
3. Create `tests/SpamRemovalAgent.Tests/` directory

**Commands**:
```bash
mkdir -p src/SpamRemovalAgent
mkdir -p src/SpamRemovalAgent.AppHost
mkdir -p tests/SpamRemovalAgent.Tests
```

**Verification**:
```bash
tree -d -L 3 src tests
# Expected: All three directories exist
```

**Success Criteria**:
- All three directories exist
- Directory structure matches documented architecture

---

### T005: Create Main Console Application Project
**Depends on**: T004
**Can run in parallel**: No
**Estimated time**: 2 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\SpamRemovalAgent.csproj`

**Description**:
Create the primary console application project with .NET 10 target framework. This satisfies FR-002, FR-004, FR-006 and implements Entity 2 (Main Console Project) from data-model.md.

**Steps**:
1. Navigate to `src/SpamRemovalAgent/`
2. Run `dotnet new console --framework net10.0 --name SpamRemovalAgent --use-program-main false`
3. Return to repository root
4. Add project to solution: `dotnet sln add src/SpamRemovalAgent/SpamRemovalAgent.csproj`
5. Build project to verify creation

**Commands**:
```bash
cd src/SpamRemovalAgent
dotnet new console --framework net10.0 --name SpamRemovalAgent --use-program-main false
cd ../..
dotnet sln add src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Verification**:
```bash
ls src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
# Expected: Build succeeded
```

**Success Criteria**:
- Project file exists
- Project added to solution
- Project builds successfully (exit code 0)

---

### T006: Configure Main Project Properties and Dependencies
**Depends on**: T005
**Can run in parallel**: No
**Estimated time**: 2 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\SpamRemovalAgent.csproj`

**Description**:
Enable modern C# features (nullable reference types, implicit usings, latest language version) and add required hosting/logging dependencies. This satisfies FR-014 and implements research decisions 6 and 7.

**Steps**:
1. Edit `src/SpamRemovalAgent/SpamRemovalAgent.csproj`
2. Verify/add properties: `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<LangVersion>latest</LangVersion>`
3. Add package references:
   - `Microsoft.Extensions.Hosting` (version 10.0.0)
   - `Microsoft.Extensions.Logging` (version 10.0.0)
   - `Microsoft.Extensions.Configuration.Json` (version 10.0.0)
4. Run `dotnet restore` and `dotnet build`

**Commands**:
```bash
cd src/SpamRemovalAgent
dotnet add package Microsoft.Extensions.Hosting --version 10.0.0
dotnet add package Microsoft.Extensions.Logging --version 10.0.0
dotnet add package Microsoft.Extensions.Configuration.Json --version 10.0.0
cd ../..
dotnet restore src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Manual Edit** (verify in .csproj):
```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

**Verification**:
```bash
cat src/SpamRemovalAgent/SpamRemovalAgent.csproj | grep "Nullable"
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
# Expected: Contains <Nullable>enable</Nullable> and build succeeds
```

**Success Criteria**:
- All properties configured correctly
- All package references added
- Project builds without errors

---

### T007: Create Aspire AppHost Project
**Depends on**: T006
**Can run in parallel**: No
**Estimated time**: 2 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent.AppHost\SpamRemovalAgent.AppHost.csproj`

**Description**:
Create the .NET Aspire AppHost project for development-time orchestration and observability. This satisfies FR-018, FR-019 and implements Entity 3 (Aspire AppHost) from data-model.md.

**Steps**:
1. Navigate to `src/SpamRemovalAgent.AppHost/`
2. Run `dotnet new aspire-apphost --framework net10.0 --name SpamRemovalAgent.AppHost`
3. Return to repository root
4. Add project to solution
5. Add project reference to main console project
6. Build project to verify

**Commands**:
```bash
cd src/SpamRemovalAgent.AppHost
dotnet new aspire-apphost --framework net10.0 --name SpamRemovalAgent.AppHost
cd ../..
dotnet sln add src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
cd src/SpamRemovalAgent.AppHost
dotnet add reference ../SpamRemovalAgent/SpamRemovalAgent.csproj
cd ../..
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
```

**Note**: If `aspire-apphost` template is not found, run: `dotnet new install Aspire.ProjectTemplates`

**Verification**:
```bash
ls src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
# Expected: Project exists and builds successfully
```

**Success Criteria**:
- AppHost project file exists
- Project added to solution
- Project references main console project
- Project builds successfully

---

### T008: Create Test Project with xUnit
**Depends on**: T006
**Can run in parallel**: No
**Estimated time**: 2 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\tests\SpamRemovalAgent.Tests\SpamRemovalAgent.Tests.csproj`

**Description**:
Create the xUnit test project with reference to main console project. This satisfies FR-003, FR-009, FR-010 and implements Entity 4 (Test Project) from data-model.md.

**Steps**:
1. Navigate to `tests/SpamRemovalAgent.Tests/`
2. Run `dotnet new xunit --framework net10.0 --name SpamRemovalAgent.Tests`
3. Return to repository root
4. Add project to solution
5. Add project reference to main console project
6. Build and test project to verify

**Commands**:
```bash
cd tests/SpamRemovalAgent.Tests
dotnet new xunit --framework net10.0 --name SpamRemovalAgent.Tests
cd ../..
dotnet sln add tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
cd tests/SpamRemovalAgent.Tests
dotnet add reference ../../src/SpamRemovalAgent/SpamRemovalAgent.csproj
cd ../..
dotnet build tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
dotnet test tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

**Verification**:
```bash
ls tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
dotnet test tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
# Expected: Test run completes (0 tests initially is OK)
```

**Success Criteria**:
- Test project file exists
- Project added to solution
- Project references main console project
- Project builds and test execution succeeds (0 tests OK)

---

## Phase 3: Folder Structure Creation

### T009 [P]: Create Main Project Core Folders
**Depends on**: T006
**Can run in parallel**: Yes (with T010)
**Estimated time**: 1 minute
**Directories**:
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Agents\`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Services\`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Authentication\`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Hosting\`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Observability\`
- `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Models\`

**Description**:
Create the vertical slice folder structure for the main console project. This satisfies FR-005 and implements Entity 5 (Folder Structure) from data-model.md. Folders represent separation of concerns per research decision 4.

**Steps**:
1. Create `Agents/` folder (for AIAgent implementations)
2. Create `Services/` folder (for business logic)
3. Create `Authentication/` folder (for OAuth token management)
4. Create `Hosting/` folder (for IHostedService implementations)
5. Create `Observability/` folder (for Application Insights)
6. Create `Models/` folder (for domain entities)

**Commands**:
```bash
mkdir -p src/SpamRemovalAgent/Agents
mkdir -p src/SpamRemovalAgent/Services
mkdir -p src/SpamRemovalAgent/Authentication
mkdir -p src/SpamRemovalAgent/Hosting
mkdir -p src/SpamRemovalAgent/Observability
mkdir -p src/SpamRemovalAgent/Models
```

**Verification**:
```bash
tree -d -L 2 src/SpamRemovalAgent
# Expected: All six folders visible
ls -d src/SpamRemovalAgent/{Agents,Services,Authentication,Hosting,Observability,Models}
```

**Success Criteria**:
- All six folders exist
- Folder structure matches documented architecture in `.github/copilot-instructions.md`

---

### T010 [P]: Create Test Project Organization Folders
**Depends on**: T008
**Can run in parallel**: Yes (with T009)
**Estimated time**: 1 minute
**Directories**:
- `F:\github\HemSoft\spam-removal-agent\tests\SpamRemovalAgent.Tests\unit\`
- `F:\github\HemSoft\spam-removal-agent\tests\SpamRemovalAgent.Tests\integration\`
- `F:\github\HemSoft\spam-removal-agent\tests\SpamRemovalAgent.Tests\utilities\`

**Description**:
Create test organization folders for unit tests, integration tests, and test utilities. This satisfies FR-011 and implements test structure from research decision 5.

**Steps**:
1. Create `unit/` folder (for fast, isolated unit tests)
2. Create `integration/` folder (for tests with external dependencies)
3. Create `utilities/` folder (for test helpers, mocks, test data)
4. Optionally remove default `UnitTest1.cs` file

**Commands**:
```bash
mkdir -p tests/SpamRemovalAgent.Tests/unit
mkdir -p tests/SpamRemovalAgent.Tests/integration
mkdir -p tests/SpamRemovalAgent.Tests/utilities
rm tests/SpamRemovalAgent.Tests/UnitTest1.cs 2>/dev/null || true
```

**Verification**:
```bash
tree -d tests/SpamRemovalAgent.Tests
# Expected: Three folders visible (unit, integration, utilities)
ls -d tests/SpamRemovalAgent.Tests/{unit,integration,utilities}
```

**Success Criteria**:
- All three folders exist
- Folder structure ready for test organization

---

## Phase 4: Configuration and Entry Points

### T011: Create Program.cs with Hosting Infrastructure
**Depends on**: T009
**Can run in parallel**: No
**Estimated time**: 3 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\Program.cs`

**Description**:
Create the application entry point with .NET Generic Host, dependency injection, and a temporary startup service that outputs version string. This satisfies FR-007, FR-013 and implements Entity 7 (Entry Point Files) from data-model.md.

**Steps**:
1. Replace/create `src/SpamRemovalAgent/Program.cs` with hosting infrastructure
2. Include top-level statements with Host.CreateApplicationBuilder
3. Configure console logging
4. Register temporary StartupService (outputs version then exits)
5. Build and run to verify

**File Content** (`src/SpamRemovalAgent/Program.cs`):
```csharp
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.AddConsole();

// Register hosted service (will be replaced with actual agent service in future features)
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
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
# Expected output: "info: StartupService[0] Spam Removal Agent v1.0.0"
```

**Success Criteria**:
- Program.cs contains Host.CreateApplicationBuilder
- Application builds successfully
- Application runs and outputs "Spam Removal Agent v1.0.0"

---

### T012: Create Main Project Configuration File
**Depends on**: T009
**Can run in parallel**: No
**Estimated time**: 2 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent\appsettings.json`

**Description**:
Create application configuration file with logging settings and version information. This satisfies FR-008 and implements Entity 6 (Configuration Files) from data-model.md, following research decision 7.

**Steps**:
1. Create `src/SpamRemovalAgent/appsettings.json`
2. Add Logging configuration section
3. Add SpamRemovalAgent section with Version
4. Update project file to copy config to output directory
5. Verify JSON validity

**File Content** (`src/SpamRemovalAgent/appsettings.json`):
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

**Project File Update** (add to `SpamRemovalAgent.csproj`):
```xml
<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

**Verification**:
```bash
cat src/SpamRemovalAgent/appsettings.json | jq .
# Expected: Valid JSON parsed successfully
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Success Criteria**:
- appsettings.json is valid JSON
- Configuration contains Logging and Version sections
- No secrets present in configuration
- File copied to output directory on build

---

### T013: Configure Aspire AppHost Program.cs
**Depends on**: T007
**Can run in parallel**: No
**Estimated time**: 2 minutes
**File**: `F:\github\HemSoft\spam-removal-agent\src\SpamRemovalAgent.AppHost\Program.cs`

**Description**:
Configure Aspire AppHost to orchestrate the main console application. This satisfies FR-020 and implements Aspire integration pattern from research decision 2.

**Steps**:
1. Edit `src/SpamRemovalAgent.AppHost/Program.cs`
2. Add project reference using `AddProject<Projects.SpamRemovalAgent>`
3. Build to verify configuration

**File Content** (`src/SpamRemovalAgent.AppHost/Program.cs`):
```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Add reference to main console application
var spamAgent = builder.AddProject<Projects.SpamRemovalAgent>("spam-agent");

builder.Build().Run();
```

**Verification**:
```bash
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
# Expected: Build succeeded
```

**Success Criteria**:
- Program.cs contains DistributedApplication.CreateBuilder
- AppHost references main console project via AddProject
- AppHost builds successfully

---

## Phase 5: Build Verification

### T014: Build All Projects Individually
**Depends on**: T011, T012, T013
**Can run in parallel**: No
**Estimated time**: 2 minutes

**Description**:
Build each project individually to ensure they compile correctly before building the entire solution. This satisfies FR-012 and Contract 4 Rule 4.8, Contract 5 Rule 5.6, Contract 6 Rule 6.8.

**Steps**:
1. Build main console project
2. Build Aspire AppHost project
3. Build test project
4. Verify all builds succeed

**Commands**:
```bash
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj --configuration Release
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj --configuration Release
dotnet build tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj --configuration Release
```

**Verification**:
```bash
# Check each build exit code
echo "Main project: $?"
echo "AppHost project: $?"
echo "Test project: $?"
# All should be 0
```

**Success Criteria**:
- All three projects build successfully in Release configuration
- No compilation errors or warnings (warnings acceptable for scaffolding)
- Exit code 0 for all builds

---

### T015: Build Complete Solution
**Depends on**: T014
**Can run in parallel**: No
**Estimated time**: 2 minutes

**Description**:
Build the entire solution to verify all projects and dependencies are correctly configured. This satisfies FR-012, Contract 3 Rule 3.5, and Contract 8 Rule 8.1.

**Steps**:
1. Clean solution
2. Restore packages
3. Build solution in Release configuration
4. Verify build success

**Commands**:
```bash
dotnet clean SpamRemovalAgent.sln
dotnet restore SpamRemovalAgent.sln
dotnet build SpamRemovalAgent.sln --configuration Release
```

**Verification**:
```bash
dotnet build SpamRemovalAgent.sln --configuration Release
echo $?
# Expected: 0 (success)
# Output should contain "Build succeeded"
```

**Success Criteria**:
- Solution builds successfully in Release configuration
- Output contains "Build succeeded"
- Exit code 0
- 0 errors (warnings acceptable)

---

## Phase 6: Execution Validation

### T016: Run Main Application and Verify Output
**Depends on**: T015
**Can run in parallel**: No
**Estimated time**: 1 minute

**Description**:
Execute the main console application and verify it outputs the version string as required. This satisfies FR-013, Contract 4 Rule 4.9, and Contract 8 Rule 8.2.

**Steps**:
1. Run main application
2. Capture output
3. Verify output contains "Spam Removal Agent v1.0.0"
4. Verify application exits cleanly

**Commands**:
```bash
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

**Expected Output**:
```
info: StartupService[0]
      Spam Removal Agent v1.0.0
```

**Verification**:
```bash
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj 2>&1 | grep "Spam Removal Agent v1.0.0"
# Should find the version string
```

**Success Criteria**:
- Application runs without exceptions
- Output contains "Spam Removal Agent v1.0.0"
- Application exits with code 0
- Execution completes within 5 seconds

---

### T017: Run Test Suite
**Depends on**: T015
**Can run in parallel**: No
**Estimated time**: 1 minute

**Description**:
Execute the test suite to verify test infrastructure is working. This satisfies Contract 6 Rule 6.9 and Contract 8 Rule 8.3.

**Steps**:
1. Run all tests in solution
2. Verify test execution completes
3. Verify no test failures (0 tests initially is acceptable)

**Commands**:
```bash
dotnet test SpamRemovalAgent.sln --verbosity normal
```

**Expected Output**:
```
Test Run Successful.
Total tests: 0
  Passed: 0
  Failed: 0
  Skipped: 0
```

**Verification**:
```bash
dotnet test SpamRemovalAgent.sln
echo $?
# Expected: 0 (success)
```

**Success Criteria**:
- Test execution completes successfully
- Exit code 0
- No test failures
- 0 tests is acceptable at this stage

---

### T018: Launch Aspire Dashboard (Manual Verification)
**Depends on**: T015
**Can run in parallel**: No
**Estimated time**: 2 minutes (manual)

**Description**:
Launch the Aspire AppHost to verify the dashboard opens and shows the registered application. This satisfies FR-019 and Contract 8 Rule 8.4. Note: This is primarily manual verification.

**Steps**:
1. Run AppHost project
2. Wait for dashboard to open in browser
3. Verify "spam-agent" service is visible in dashboard
4. Stop dashboard (Ctrl+C)

**Commands**:
```bash
dotnet run --project src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
# Wait for browser to open with dashboard
# Verify service is listed
# Press Ctrl+C to stop
```

**Expected Behavior**:
- Dashboard opens in default browser
- URL typically: `http://localhost:15XXX` or `https://localhost:17XXX`
- Dashboard shows "spam-agent" service
- Service status visible

**Verification** (manual):
- [ ] Browser opens automatically
- [ ] Dashboard loads successfully
- [ ] "spam-agent" appears in service list
- [ ] No errors in console output

**Success Criteria**:
- AppHost launches without errors
- Dashboard accessible in browser
- Service correctly registered (manual verification)

**Note**: Automated verification difficult due to browser interaction - primarily manual task.

---

## Phase 7: Contract Validation

### T019 [P]: Validate Solution Structure (Contract 3)
**Depends on**: T016, T017
**Can run in parallel**: Yes (with T020, T021, T022)
**Estimated time**: 1 minute

**Description**:
Validate that the solution file exists, references all projects, and builds successfully. Implements all rules from Contract 3 (SolutionFileExists).

**Validation Checks**:
1. ✅ Rule 3.1: Solution file exists at repository root
2. ✅ Rule 3.2: Solution references main console project
3. ✅ Rule 3.3: Solution references AppHost project
4. ✅ Rule 3.4: Solution references test project
5. ✅ Rule 3.5: Solution builds successfully

**Commands**:
```bash
# Check existence
ls SpamRemovalAgent.sln

# Check project references
cat SpamRemovalAgent.sln | grep "SpamRemovalAgent.csproj"
cat SpamRemovalAgent.sln | grep "SpamRemovalAgent.AppHost.csproj"
cat SpamRemovalAgent.sln | grep "SpamRemovalAgent.Tests.csproj"

# Check build
dotnet build SpamRemovalAgent.sln
```

**Success Criteria**:
- All 5 validation rules pass
- Solution file correctly structured
- All project references present

---

### T020 [P]: Validate Main Project Structure (Contract 4)
**Depends on**: T016
**Can run in parallel**: Yes (with T019, T021, T022)
**Estimated time**: 2 minutes

**Description**:
Validate the main console project configuration, folder structure, and execution. Implements all rules from Contract 4 (MainProjectStructure).

**Validation Checks**:
1. ✅ Rule 4.1: Project file exists
2. ✅ Rule 4.2: Targets .NET 10 (`net10.0`)
3. ✅ Rule 4.3: OutputType is Exe
4. ✅ Rule 4.4: Nullable enabled
5. ✅ Rule 4.5: Program.cs exists and contains host builder
6. ✅ Rule 4.6: appsettings.json exists and is valid JSON
7. ✅ Rule 4.7: All required folders exist (Agents, Services, Authentication, Hosting, Observability, Models)
8. ✅ Rule 4.8: Project builds successfully
9. ✅ Rule 4.9: Project runs and outputs version string

**Commands**:
```bash
# Check project file
cat src/SpamRemovalAgent/SpamRemovalAgent.csproj | grep "net10.0"
cat src/SpamRemovalAgent/SpamRemovalAgent.csproj | grep "Exe"
cat src/SpamRemovalAgent/SpamRemovalAgent.csproj | grep "enable"

# Check files
ls src/SpamRemovalAgent/Program.cs
cat src/SpamRemovalAgent/Program.cs | grep "Host.CreateApplicationBuilder"
cat src/SpamRemovalAgent/appsettings.json | jq .

# Check folders
ls -d src/SpamRemovalAgent/{Agents,Services,Authentication,Hosting,Observability,Models}

# Check build and execution
dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj | grep "v1.0.0"
```

**Success Criteria**:
- All 9 validation rules pass
- Project correctly configured per constitutional requirements

---

### T021 [P]: Validate Aspire AppHost Structure (Contract 5)
**Depends on**: T017
**Can run in parallel**: Yes (with T019, T020, T022)
**Estimated time**: 1 minute

**Description**:
Validate the Aspire AppHost project configuration and references. Implements all rules from Contract 5 (AspireProjectStructure).

**Validation Checks**:
1. ✅ Rule 5.1: AppHost project file exists
2. ✅ Rule 5.2: Targets .NET 10
3. ✅ Rule 5.3: References Aspire.Hosting.AppHost package
4. ✅ Rule 5.4: References main console project
5. ✅ Rule 5.5: Program.cs exists and contains DistributedApplication
6. ✅ Rule 5.6: AppHost builds successfully

**Commands**:
```bash
# Check project file
cat src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj | grep "net10.0"
cat src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj | grep "Aspire.Hosting.AppHost"
cat src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj | grep "SpamRemovalAgent.csproj"

# Check Program.cs
cat src/SpamRemovalAgent.AppHost/Program.cs | grep "DistributedApplication"

# Check build
dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
```

**Success Criteria**:
- All 6 validation rules pass
- AppHost correctly configured for orchestration

---

### T022 [P]: Validate Test Project Structure (Contract 6)
**Depends on**: T017
**Can run in parallel**: Yes (with T019, T020, T021)
**Estimated time**: 2 minutes

**Description**:
Validate the test project configuration, folder structure, and test execution. Implements all rules from Contract 6 (TestProjectStructure).

**Validation Checks**:
1. ✅ Rule 6.1: Test project file exists
2. ✅ Rule 6.2: Targets .NET 10
3. ✅ Rule 6.3: References xunit package
4. ✅ Rule 6.4: References xunit.runner.visualstudio
5. ✅ Rule 6.5: References Microsoft.NET.Test.Sdk
6. ✅ Rule 6.6: References main console project
7. ✅ Rule 6.7: Test folders exist (unit, integration, utilities)
8. ✅ Rule 6.8: Test project builds successfully
9. ✅ Rule 6.9: Tests can be executed (0 tests OK)

**Commands**:
```bash
# Check project file
cat tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj | grep "net10.0"
cat tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj | grep "xunit\""
cat tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj | grep "xunit.runner.visualstudio"
cat tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj | grep "Microsoft.NET.Test.Sdk"
cat tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj | grep "SpamRemovalAgent.csproj"

# Check folders
ls -d tests/SpamRemovalAgent.Tests/{unit,integration,utilities}

# Check build and test
dotnet build tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
dotnet test tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

**Success Criteria**:
- All 9 validation rules pass
- Test infrastructure ready for TDD in future features

---

## Dependencies Graph

```
T001 (Prerequisites: .NET 10 SDK)
  └─> T002 (Conflict Check)
       └─> T003 (Solution File)
            └─> T004 (Directories)
                 ├─> T005 (Main Project)
                 │    └─> T006 (Main Config & Deps)
                 │         ├─> T007 (AppHost) ──────────────────┐
                 │         │    └─> T013 (AppHost Program.cs)   │
                 │         │                                     │
                 │         ├─> T009 [P] (Main Folders)          │
                 │         │    ├─> T011 (Program.cs)           │
                 │         │    └─> T012 (appsettings.json)     │
                 │         │                                     │
                 │         └─> T008 (Test Project)              │
                 │              └─> T010 [P] (Test Folders)     │
                 │                                               │
                 └──> T011, T012, T013 ──────> T014 (Build Projects)
                                                 └─> T015 (Build Solution)
                                                      ├─> T016 (Run App)
                                                      ├─> T017 (Run Tests)
                                                      └─> T018 (Aspire Dashboard - Manual)
                                                           │
                     ┌─────────────────────────────────────┴───────────────────────┐
                     │                                                             │
                     ├─> T019 [P] (Validate Solution - Contract 3)                │
                     ├─> T020 [P] (Validate Main Project - Contract 4)            │
                     ├─> T021 [P] (Validate AppHost - Contract 5)                 │
                     └─> T022 [P] (Validate Test Project - Contract 6)            │
```

---

## Parallel Execution Examples

### Parallel Group 1: Folder Creation (after T006, T008)
```bash
# Can execute simultaneously - different directories, no dependencies
Task: "Create main project core folders in src/SpamRemovalAgent/{Agents,Services,Authentication,Hosting,Observability,Models}"
Task: "Create test organization folders in tests/SpamRemovalAgent.Tests/{unit,integration,utilities}"
```

### Parallel Group 2: Contract Validation (after T016, T017)
```bash
# Can execute simultaneously - independent validation checks
Task: "Validate solution structure per Contract 3"
Task: "Validate main project structure per Contract 4"
Task: "Validate AppHost structure per Contract 5"
Task: "Validate test project structure per Contract 6"
```

---

## Validation Checklist

After completing all tasks, verify the following:

### File System Structure
- [ ] ✅ SpamRemovalAgent.sln exists at repository root
- [ ] ✅ src/SpamRemovalAgent/SpamRemovalAgent.csproj exists
- [ ] ✅ src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj exists
- [ ] ✅ tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj exists
- [ ] ✅ All main project folders exist (6 folders)
- [ ] ✅ All test folders exist (3 folders)

### Build Success
- [ ] ✅ Main console project builds successfully
- [ ] ✅ Aspire AppHost builds successfully
- [ ] ✅ Test project builds successfully
- [ ] ✅ Full solution builds successfully in Release configuration

### Execution Success
- [ ] ✅ Main application runs and outputs "Spam Removal Agent v1.0.0"
- [ ] ✅ Test suite executes successfully (0 tests is OK)
- [ ] ✅ Aspire AppHost launches dashboard (manual verification)

### Configuration Validation
- [ ] ✅ appsettings.json files are valid JSON
- [ ] ✅ No secrets in configuration files
- [ ] ✅ Configuration contains required sections

### Constitutional Compliance
- [ ] ✅ All projects target .NET 10 (net10.0)
- [ ] ✅ Main project uses IHostedService pattern
- [ ] ✅ Aspire AppHost integrated for orchestration
- [ ] ✅ Test project uses xUnit framework
- [ ] ✅ Folder structure matches documented architecture in `.github/copilot-instructions.md`

### Contract Validation (All 8 Contracts)
- [ ] ✅ Contract 1: Prerequisite Check - .NET 10 SDK installed
- [ ] ✅ Contract 2: Conflict Check - No conflicting files
- [ ] ✅ Contract 3: Solution File - Exists and references all projects
- [ ] ✅ Contract 4: Main Project - Correctly configured and executable
- [ ] ✅ Contract 5: Aspire AppHost - References main project and builds
- [ ] ✅ Contract 6: Test Project - xUnit configured and testable
- [ ] ✅ Contract 7: Configuration Files - Valid JSON, no secrets
- [ ] ✅ Contract 8: End-to-End - Full solution builds, runs, tests pass

---

## Success Criteria Summary

Upon completion of all 22 tasks, the following functional requirements are satisfied:

| Requirement | Tasks | Status |
|-------------|-------|--------|
| FR-001: Solution file created | T003 | ✅ |
| FR-002: Main console project | T005, T006 | ✅ |
| FR-003: Test project structure | T008, T010 | ✅ |
| FR-004: .NET 10 target framework | T005, T006, T007, T008 | ✅ |
| FR-005: Main project folders | T009 | ✅ |
| FR-006: Console app configuration | T006 | ✅ |
| FR-007: Entry point with DI | T011 | ✅ |
| FR-008: Configuration files | T012 | ✅ |
| FR-009-011: Test project with xUnit | T008, T010 | ✅ |
| FR-012: Solution buildable | T014, T015 | ✅ |
| FR-013: Application executable with output | T016 | ✅ |
| FR-014: Properly configured project files | T006 | ✅ |
| FR-015-017: Architecture alignment | T009, T010 | ✅ |
| FR-018-020: Aspire AppHost | T007, T013, T018 | ✅ |
| FR-021-023: Error handling | T001, T002 | ✅ |

**Total Requirements Satisfied**: 23/23 (100%)

---

## Notes for Implementation

1. **Order Matters**: Most tasks are sequential due to dependencies. Follow the task order strictly.

2. **Parallel Execution**: Only T009/T010 and T019/T020/T021/T022 can run in parallel. Do not parallelize other tasks.

3. **Verification is Critical**: Each task includes verification steps. Do not proceed if verification fails.

4. **Manual Steps**: Task T018 (Aspire Dashboard) requires manual browser verification.

5. **Constitutional Compliance**: All tasks designed to satisfy constitutional principles. No deviations.

6. **Error Handling**: Tasks T001 and T002 implement fail-fast error detection. Scaffolding stops on prerequisite failure.

7. **Future Features**: This scaffolding establishes foundation. Future features will add:
   - Microsoft Graph SDK integration (Services/ folder)
   - Agent Framework implementation (Agents/ folder)
   - OAuth authentication (Authentication/ folder)
   - Application Insights (Observability/ folder)

8. **Testing Infrastructure**: Test project ready for TDD in future features. Current scaffolding has 0 tests (expected).

9. **Version Control**: Consider committing after major milestones:
   - After T003 (solution created)
   - After T008 (all projects created)
   - After T015 (solution builds)
   - After T022 (all validation passes)

10. **Troubleshooting**: See `quickstart.md` troubleshooting section for common issues and solutions.

---

## Task Generation Metadata

**Generated from**:
- plan.md: Tech stack, structure decisions, Phase 2 task planning approach
- data-model.md: 7 entities with properties and relationships
- research.md: 10 technical decisions and best practices
- contracts/scaffolding-validation.md: 8 contracts with 36 validation rules
- quickstart.md: 15-step implementation guide with verification steps

**Task Generation Rules Applied**:
- ✅ Linear execution for dependencies (solution → projects → folders → config → build → validation)
- ✅ Parallel markers [P] only for independent tasks (folders, validations)
- ✅ Each task specifies exact file paths
- ✅ Verification steps for every task
- ✅ Contract validation tasks map to contract documents
- ✅ Quickstart steps map to implementation tasks
- ✅ Constitutional principles reflected in all tasks

**Validation**:
- ✅ All contracts have validation tasks (T019-T022 cover all 8 contracts)
- ✅ All entities have creation/configuration tasks
- ✅ All folder structures documented and created
- ✅ Parallel tasks truly independent (different files/directories)
- ✅ Each task includes explicit verification steps
- ✅ No task modifies same file as another [P] task

---

**Status**: ✅ **TASKS READY FOR EXECUTION**

**Next Step**: Begin implementation starting with T001 (Verify .NET 10 SDK Installation)

---

*Tasks generated: 2025-10-05*
*Total tasks: 22*
*Estimated completion time: 30-45 minutes*
*Based on: plan.md v1.0, data-model.md v1.0, contracts/scaffolding-validation.md v1.0, quickstart.md v1.0*
