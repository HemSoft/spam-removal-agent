# Scaffolding Validation Contract

**Feature**: 001-scaffold-out-the | **Date**: 2025-10-05
**Contract Type**: File System Validation

## Overview
This contract defines the validation rules for successful project scaffolding. It specifies what files, folders, and configurations MUST exist and be valid after the scaffolding process completes.

---

## 1. Prerequisite Validation Contract

### Contract: PrerequisiteCheck
**Purpose**: Verify system prerequisites before scaffolding

**Inputs**:
- System environment

**Outputs**:
- `SUCCESS` with SDK version OR `ERROR` with missing prerequisite details

**Validation Rules**:

#### Rule 1.1: .NET 10 SDK Must Be Installed
```yaml
Check: dotnet --list-sdks
Condition: Output contains version matching "10\."
Error Message: |
  ERROR: .NET 10 SDK not found.
  Required: .NET 10.0.x or higher
  Installed SDKs: [list from command]
  Download: https://dotnet.microsoft.com/download/dotnet/10.0
Exit Code: 1
```

**Test Scenario**:
```bash
# Test prerequisite check
GIVEN .NET 10 SDK is not installed
WHEN prerequisite validation runs
THEN validation fails with error message containing download link
AND exit code is 1
AND no files are created
```

---

## 2. Conflict Detection Contract

### Contract: ConflictCheck
**Purpose**: Detect existing files that would conflict with scaffolding

**Inputs**:
- Repository root path: `F:\github\HemSoft\spam-removal-agent\`

**Outputs**:
- `SUCCESS` (no conflicts) OR `ERROR` with list of conflicting files

**Validation Rules**:

#### Rule 2.1: Solution File Must Not Exist
```yaml
Check: File exists SpamRemovalAgent.sln
Condition: File MUST NOT exist
Conflict: SpamRemovalAgent.sln
```

#### Rule 2.2: Main Project Must Not Exist
```yaml
Check: File exists src/SpamRemovalAgent/SpamRemovalAgent.csproj
Condition: File MUST NOT exist
Conflict: src/SpamRemovalAgent/SpamRemovalAgent.csproj
```

#### Rule 2.3: AppHost Project Must Not Exist
```yaml
Check: File exists src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
Condition: File MUST NOT exist
Conflict: src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
```

#### Rule 2.4: Test Project Must Not Exist
```yaml
Check: File exists tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
Condition: File MUST NOT exist
Conflict: tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
```

**Error Format**:
```
ERROR: Conflicting files detected. Manual cleanup required.
Conflicts:
  - SpamRemovalAgent.sln
  - src/SpamRemovalAgent/SpamRemovalAgent.csproj
  - src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
Action: Remove or rename conflicting files before re-running scaffolding.
Exit Code: 2
```

**Test Scenario**:
```bash
# Test conflict detection
GIVEN SpamRemovalAgent.sln already exists in repository root
WHEN conflict detection runs
THEN validation fails with error listing SpamRemovalAgent.sln
AND exit code is 2
AND no new files are created
AND existing files are not modified
```

---

## 3. Solution File Contract

### Contract: SolutionFileExists
**Purpose**: Validate solution file creation and structure

**Inputs**: None (post-scaffolding validation)

**Outputs**:
- `SUCCESS` OR `ERROR` with validation failure details

**Validation Rules**:

#### Rule 3.1: Solution File Must Exist
```yaml
File: SpamRemovalAgent.sln
Location: Repository root (F:\github\HemSoft\spam-removal-agent\)
Condition: MUST exist
Error: "Solution file SpamRemovalAgent.sln not found"
```

#### Rule 3.2: Solution Must Reference Main Project
```yaml
File: SpamRemovalAgent.sln
Content Check: Contains "SpamRemovalAgent.csproj"
Path Pattern: "src\\SpamRemovalAgent\\SpamRemovalAgent.csproj"
Error: "Solution does not reference main console project"
```

#### Rule 3.3: Solution Must Reference AppHost Project
```yaml
File: SpamRemovalAgent.sln
Content Check: Contains "SpamRemovalAgent.AppHost.csproj"
Path Pattern: "src\\SpamRemovalAgent.AppHost\\SpamRemovalAgent.AppHost.csproj"
Error: "Solution does not reference Aspire AppHost project"
```

#### Rule 3.4: Solution Must Reference Test Project
```yaml
File: SpamRemovalAgent.sln
Content Check: Contains "SpamRemovalAgent.Tests.csproj"
Path Pattern: "tests\\SpamRemovalAgent.Tests\\SpamRemovalAgent.Tests.csproj"
Error: "Solution does not reference test project"
```

#### Rule 3.5: Solution Must Build Successfully
```yaml
Command: dotnet build SpamRemovalAgent.sln
Condition: Exit code MUST be 0
Error: "Solution build failed"
```

**Test Scenario**:
```bash
# Test solution creation
GIVEN scaffolding completed successfully
WHEN solution validation runs
THEN SpamRemovalAgent.sln exists at repository root
AND solution file contains all three project references
AND `dotnet build SpamRemovalAgent.sln` succeeds with exit code 0
```

---

## 4. Main Console Project Contract

### Contract: MainProjectStructure
**Purpose**: Validate main console project creation and configuration

**Validation Rules**:

#### Rule 4.1: Project File Must Exist
```yaml
File: src/SpamRemovalAgent/SpamRemovalAgent.csproj
Condition: MUST exist
Error: "Main project file not found"
```

#### Rule 4.2: Project Must Target .NET 10
```xml
File: SpamRemovalAgent.csproj
XPath: /Project/PropertyGroup/TargetFramework
Expected Value: "net10.0"
Error: "Project does not target .NET 10"
```

#### Rule 4.3: Project Must Be Console Application
```xml
File: SpamRemovalAgent.csproj
XPath: /Project/PropertyGroup/OutputType
Expected Value: "Exe"
Error: "Project is not configured as console application"
```

#### Rule 4.4: Nullable Must Be Enabled
```xml
File: SpamRemovalAgent.csproj
XPath: /Project/PropertyGroup/Nullable
Expected Value: "enable"
Error: "Nullable reference types not enabled"
```

#### Rule 4.5: Program.cs Must Exist
```yaml
File: src/SpamRemovalAgent/Program.cs
Condition: MUST exist
Content Check: Contains "Host.CreateApplicationBuilder"
Error: "Program.cs missing or invalid"
```

#### Rule 4.6: Configuration File Must Exist
```yaml
File: src/SpamRemovalAgent/appsettings.json
Condition: MUST exist
Format: Valid JSON
Error: "appsettings.json missing or invalid JSON"
```

#### Rule 4.7: Required Folders Must Exist
```yaml
Folders:
  - src/SpamRemovalAgent/Agents/
  - src/SpamRemovalAgent/Services/
  - src/SpamRemovalAgent/Authentication/
  - src/SpamRemovalAgent/Hosting/
  - src/SpamRemovalAgent/Observability/
  - src/SpamRemovalAgent/Models/
Condition: ALL folders MUST exist
Error: "Missing folder: {folder_name}"
```

#### Rule 4.8: Project Must Build Successfully
```yaml
Command: dotnet build src/SpamRemovalAgent/SpamRemovalAgent.csproj
Condition: Exit code MUST be 0
Error: "Main project build failed"
```

#### Rule 4.9: Project Must Run Successfully
```yaml
Command: dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
Condition: Exit code MUST be 0
Output Check: Contains "Spam Removal Agent v1.0.0"
Error: "Main project execution failed or incorrect output"
```

**Test Scenario**:
```bash
# Test main project structure
GIVEN scaffolding completed successfully
WHEN main project validation runs
THEN SpamRemovalAgent.csproj exists with correct configuration
AND all required folders exist (Agents, Services, Authentication, Hosting, Observability, Models)
AND Program.cs contains host builder setup
AND appsettings.json is valid JSON
AND `dotnet build` succeeds
AND `dotnet run` outputs "Spam Removal Agent v1.0.0"
```

---

## 5. Aspire AppHost Project Contract

### Contract: AspireProjectStructure
**Purpose**: Validate Aspire AppHost project creation and configuration

**Validation Rules**:

#### Rule 5.1: AppHost Project File Must Exist
```yaml
File: src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
Condition: MUST exist
Error: "AppHost project file not found"
```

#### Rule 5.2: AppHost Must Target .NET 10
```xml
File: SpamRemovalAgent.AppHost.csproj
XPath: /Project/PropertyGroup/TargetFramework
Expected Value: "net10.0"
Error: "AppHost does not target .NET 10"
```

#### Rule 5.3: AppHost Must Reference Aspire SDK
```xml
File: SpamRemovalAgent.AppHost.csproj
XPath: /Project/ItemGroup/PackageReference[@Include='Aspire.Hosting.AppHost']
Condition: MUST exist
Error: "AppHost does not reference Aspire.Hosting.AppHost package"
```

#### Rule 5.4: AppHost Must Reference Main Project
```xml
File: SpamRemovalAgent.AppHost.csproj
XPath: /Project/ItemGroup/ProjectReference[@Include='../SpamRemovalAgent/SpamRemovalAgent.csproj']
Condition: MUST exist
Error: "AppHost does not reference main console project"
```

#### Rule 5.5: AppHost Program.cs Must Exist
```yaml
File: src/SpamRemovalAgent.AppHost/Program.cs
Condition: MUST exist
Content Check: Contains "DistributedApplication"
Error: "AppHost Program.cs missing or invalid"
```

#### Rule 5.6: AppHost Must Build Successfully
```yaml
Command: dotnet build src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
Condition: Exit code MUST be 0
Error: "AppHost build failed"
```

**Test Scenario**:
```bash
# Test Aspire AppHost structure
GIVEN scaffolding completed successfully
WHEN AppHost validation runs
THEN SpamRemovalAgent.AppHost.csproj exists with Aspire SDK reference
AND AppHost references main console project
AND Program.cs contains DistributedApplication builder
AND `dotnet build` succeeds
```

---

## 6. Test Project Contract

### Contract: TestProjectStructure
**Purpose**: Validate test project creation and configuration

**Validation Rules**:

#### Rule 6.1: Test Project File Must Exist
```yaml
File: tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
Condition: MUST exist
Error: "Test project file not found"
```

#### Rule 6.2: Test Project Must Target .NET 10
```xml
File: SpamRemovalAgent.Tests.csproj
XPath: /Project/PropertyGroup/TargetFramework
Expected Value: "net10.0"
Error: "Test project does not target .NET 10"
```

#### Rule 6.3: Test Project Must Reference xUnit
```xml
File: SpamRemovalAgent.Tests.csproj
XPath: /Project/ItemGroup/PackageReference[@Include='xunit']
Condition: MUST exist
Error: "Test project does not reference xunit package"
```

#### Rule 6.4: Test Project Must Reference xUnit Runner
```xml
File: SpamRemovalAgent.Tests.csproj
XPath: /Project/ItemGroup/PackageReference[@Include='xunit.runner.visualstudio']
Condition: MUST exist
Error: "Test project does not reference xunit.runner.visualstudio"
```

#### Rule 6.5: Test Project Must Reference Test SDK
```xml
File: SpamRemovalAgent.Tests.csproj
XPath: /Project/ItemGroup/PackageReference[@Include='Microsoft.NET.Test.Sdk']
Condition: MUST exist
Error: "Test project does not reference Microsoft.NET.Test.Sdk"
```

#### Rule 6.6: Test Project Must Reference Main Project
```xml
File: SpamRemovalAgent.Tests.csproj
XPath: /Project/ItemGroup/ProjectReference
Condition: MUST reference ../../../src/SpamRemovalAgent/SpamRemovalAgent.csproj
Error: "Test project does not reference main console project"
```

#### Rule 6.7: Test Folders Must Exist
```yaml
Folders:
  - tests/SpamRemovalAgent.Tests/unit/
  - tests/SpamRemovalAgent.Tests/integration/
  - tests/SpamRemovalAgent.Tests/utilities/
Condition: ALL folders MUST exist
Error: "Missing test folder: {folder_name}"
```

#### Rule 6.8: Test Project Must Build Successfully
```yaml
Command: dotnet build tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
Condition: Exit code MUST be 0
Error: "Test project build failed"
```

#### Rule 6.9: Test Project Must Be Testable
```yaml
Command: dotnet test tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj
Condition: Exit code MUST be 0
Output Check: No test failures (may have 0 tests initially)
Error: "Test execution failed"
```

**Test Scenario**:
```bash
# Test project structure validation
GIVEN scaffolding completed successfully
WHEN test project validation runs
THEN SpamRemovalAgent.Tests.csproj exists with xUnit packages
AND test project references main console project
AND all test folders exist (unit, integration, utilities)
AND `dotnet build` succeeds
AND `dotnet test` succeeds with zero failures
```

---

## 7. Configuration File Contract

### Contract: ConfigurationValidity
**Purpose**: Validate configuration files are valid and contain required settings

**Validation Rules**:

#### Rule 7.1: Main Project Config Must Be Valid JSON
```yaml
File: src/SpamRemovalAgent/appsettings.json
Parser: JSON parser
Condition: MUST parse without errors
Error: "Invalid JSON in appsettings.json"
```

#### Rule 7.2: Main Project Config Must Have Logging Section
```json
File: src/SpamRemovalAgent/appsettings.json
JSONPath: $.Logging
Condition: MUST exist
Error: "Missing Logging configuration section"
```

#### Rule 7.3: Main Project Config Must Have Version
```json
File: src/SpamRemovalAgent/appsettings.json
JSONPath: $.SpamRemovalAgent.Version
Expected Value: "1.0.0"
Error: "Missing or incorrect version in configuration"
```

#### Rule 7.4: Main Project Config Must Not Contain Secrets
```yaml
File: src/SpamRemovalAgent/appsettings.json
Pattern Check: MUST NOT contain "password", "secret", "token", "clientsecret"
Error: "Configuration file contains potential secrets - use Windows Credential Manager"
```

#### Rule 7.5: AppHost Config Must Be Valid JSON
```yaml
File: src/SpamRemovalAgent.AppHost/appsettings.json
Parser: JSON parser
Condition: MUST parse without errors
Error: "Invalid JSON in AppHost appsettings.json"
```

**Test Scenario**:
```bash
# Test configuration validity
GIVEN scaffolding completed successfully
WHEN configuration validation runs
THEN appsettings.json files are valid JSON
AND main config contains Logging and Version sections
AND no secrets are present in configuration files
```

---

## 8. End-to-End Scaffolding Contract

### Contract: CompleteScaffoldingValidation
**Purpose**: Full validation that scaffolding produced a working solution

**Validation Rules**:

#### Rule 8.1: Full Solution Build
```yaml
Command: dotnet build SpamRemovalAgent.sln --configuration Release
Condition: Exit code MUST be 0
Output Check: "Build succeeded" appears
Error: "Full solution build failed"
```

#### Rule 8.2: Main Application Execution
```yaml
Command: timeout 5s dotnet run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj
Condition: Exit code MUST be 0 (within 5 second timeout)
Output Check: Contains "Spam Removal Agent v1.0.0"
Error: "Main application failed to execute or produce expected output"
```

#### Rule 8.3: Test Suite Execution
```yaml
Command: dotnet test SpamRemovalAgent.sln
Condition: Exit code MUST be 0
Output Check: "Total tests: 0" or "Test Run Successful"
Error: "Test suite execution failed"
```

#### Rule 8.4: Aspire Dashboard Launch
```yaml
Command: timeout 10s dotnet run --project src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj
Condition: SHOULD launch (not mandatory for automation)
Output Check: Contains "Dashboard" or "http://localhost"
Note: Manual verification preferred - dashboard launches in browser
```

**Test Scenario**:
```bash
# Test complete scaffolding
GIVEN scaffolding completed successfully
WHEN end-to-end validation runs
THEN full solution builds in Release configuration
AND main application runs and outputs version string
AND test suite executes without failures
AND Aspire AppHost can launch (manual verification)
```

---

## Contract Test Implementation Guide

### Test File Organization
```
tests/SpamRemovalAgent.Tests/
└── integration/
    └── ScaffoldingValidationTests.cs    # Implements all contract tests
```

### Test Implementation Pattern
```csharp
public class ScaffoldingValidationTests
{
    private const string RepoRoot = @"F:\github\HemSoft\spam-removal-agent";

    [Fact]
    public void PrerequisiteCheck_DotNet10SDK_MustBeInstalled()
    {
        // Implements Contract 1: PrerequisiteCheck Rule 1.1
        var sdks = RunCommand("dotnet", "--list-sdks");
        Assert.Contains("10.", sdks);
    }

    [Fact]
    public void ConflictCheck_SolutionFile_MustNotExistBeforeScaffolding()
    {
        // Implements Contract 2: ConflictCheck Rule 2.1
        var solutionPath = Path.Combine(RepoRoot, "SpamRemovalAgent.sln");

        // This test assumes scaffolding hasn't run yet
        // In actual implementation, would run in isolated environment
        Assert.False(File.Exists(solutionPath),
            "Solution file exists - conflicts detected");
    }

    [Fact]
    public void SolutionFile_MustExist_AfterScaffolding()
    {
        // Implements Contract 3: SolutionFileExists Rule 3.1
        var solutionPath = Path.Combine(RepoRoot, "SpamRemovalAgent.sln");
        Assert.True(File.Exists(solutionPath),
            "Solution file not found after scaffolding");
    }

    [Fact]
    public void MainProject_MustBuildSuccessfully()
    {
        // Implements Contract 4: MainProjectStructure Rule 4.8
        var result = RunCommand("dotnet", "build src/SpamRemovalAgent/SpamRemovalAgent.csproj");
        Assert.Equal(0, result.ExitCode);
    }

    [Fact]
    public void MainProject_MustOutputVersionString_OnExecution()
    {
        // Implements Contract 4: MainProjectStructure Rule 4.9
        var result = RunCommand("dotnet", "run --project src/SpamRemovalAgent/SpamRemovalAgent.csproj");
        Assert.Contains("Spam Removal Agent v1.0.0", result.Output);
    }

    // Additional tests for remaining contracts...
}
```

---

## Summary

**Total Contracts**: 8
**Total Validation Rules**: 36
**Coverage**:
- ✅ Prerequisite validation (SDK verification)
- ✅ Conflict detection (existing files)
- ✅ Solution structure validation
- ✅ Main console project validation
- ✅ Aspire AppHost validation
- ✅ Test project validation
- ✅ Configuration file validation
- ✅ End-to-end execution validation

**Constitutional Alignment**: All validation rules trace to functional requirements (FR-001 through FR-023) in spec.md.

---

*Contract completed: 2025-10-05*
*Next: Generate quickstart.md with step-by-step scaffolding instructions*
