# Zero Build Warnings - Quickstart Guide

## Current Status

✅ **COMPLETED**: Added `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` to all project files:
- `src/SpamRemovalAgent/SpamRemovalAgent.csproj`
- `src/SpamRemovalAgent.AppHost/SpamRemovalAgent.AppHost.csproj`
- `tests/SpamRemovalAgent.Tests/SpamRemovalAgent.Tests.csproj`

❌ **BLOCKING**: Build currently fails due to NuGet security vulnerabilities:

### Active Security Vulnerabilities

1. **NU1903: System.Text.Json 6.0.0** (HIGH severity)
   - CVE: https://github.com/advisories/GHSA-8g4q-xg66-9fp4
   - Affects: `SpamRemovalAgent` and `SpamRemovalAgent.Tests`
   - Root Cause: Transitive dependency via Microsoft.Graph 5.50.0 → Microsoft.Kiota.* libraries
   - Kiota Constraint: Requires `System.Text.Json >= 6.0.0 && < 9.0.0`

2. **NU1903: System.Linq.Dynamic.Core 1.3.12** (HIGH severity)
   - CVE: https://github.com/advisories/GHSA-4cv2-4hjh-77rx
   - Affects: `SpamRemovalAgent.Tests`
   - Root Cause: Transitive dependency via WireMock.Net 1.5.58

## Resolution Strategy

### Option 1: Update Microsoft.Graph SDK (RECOMMENDED)
Microsoft.Graph v5.x uses Kiota libraries with System.Text.Json < 9.0.0 constraint.

**Action**: Research if newer Microsoft.Graph versions (5.93.0 latest) or Kiota library updates support System.Text.Json 8.x or 9.x.

**Investigation Needed**:
```bash
# Check Microsoft.Graph latest version dependencies
dotnet list package --include-transitive

# Check if we can update to Microsoft.Graph 5.93.0
dotnet add package Microsoft.Graph --version 5.93.0
```

### Option 2: Pin System.Text.Json to Safe Version < 9.0
If Kiota constraint cannot be overcome, use the latest secure version within the constraint.

**Action**: Add explicit System.Text.Json reference to override vulnerable transitive dependency:
```xml
<PackageReference Include="System.Text.Json" Version="8.0.5" />
```

**Note**: System.Text.Json 8.0.5 is the latest version in the 8.x line, which should be compatible with Kiota's `< 9.0.0` constraint while avoiding the 6.0.0 vulnerability.

### Option 3: Update WireMock.Net
Update test dependency to resolve System.Linq.Dynamic.Core vulnerability.

**Action**:
```bash
cd tests/SpamRemovalAgent.Tests
dotnet add package WireMock.Net --version 1.6.13  # Check for latest version
```

## Immediate Next Steps

1. **Research Microsoft.Graph SDK versions**:
   - Check if 5.93.0 or later has updated Kiota dependencies
   - Verify System.Text.Json compatibility

2. **Test Package Updates**:
   - Update Microsoft.Graph to 5.93.0
   - Update WireMock.Net to latest stable
   - Add explicit System.Text.Json 8.0.5 reference if needed

3. **Verify Build Success**:
   ```bash
   dotnet restore SpamRemovalAgent.sln
   dotnet build SpamRemovalAgent.sln
   ```

4. **Run Tests**:
   ```bash
   dotnet test SpamRemovalAgent.sln
   ```

5. **Update CI/CD**: Ensure GitHub Actions workflow fails on warnings

## Links

- Constitution Article VI: `.specify/memory/constitution.md`
- Feature Spec: `specs/003-zero-build-warnings/spec.md`
- System.Text.Json Security Advisory: https://github.com/advisories/GHSA-8g4q-xg66-9fp4
- System.Linq.Dynamic.Core Security Advisory: https://github.com/advisories/GHSA-4cv2-4hjh-77rx

## Questions for Clarification

Before proceeding with the `/plan` phase, we need to clarify:

1. **Security Vulnerability Policy**: When NuGet vulnerabilities are inevitable (e.g., Kiota constraint), should we:
   - Add explicit version overrides to force safer versions within constraints?
   - Document justification for staying on constrained versions?
   - Wait for upstream library updates?

2. **Warning Suppression Policy**: How should justified warning suppressions be documented and approved?

3. **Transitive Dependency Updates**: Should we add explicit PackageReferences for vulnerable transitive dependencies to override versions?

4. **Developer Experience**: Should there be an escape hatch for local development (e.g., `-p:TreatWarningsAsErrors=false`), or is the rule absolute?
