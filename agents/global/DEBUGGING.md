---
title: "Debugging Guide"
version: "1.0.1"
lastModified: "2025-09-30"
author: "Franz Hemmer"
purpose: "Instructions and best practices for debugging the Relias Assistant application."
---

# Debugging - Instructions for debugging this application.

## General Debugging Principles

### Root Cause Analysis
- When debugging, always try to determine the root cause rather than addressing symptoms
- Debug for as long as needed to identify the root cause and identify a fix
- Revisit your assumptions if unexpected behavior occurs
- Consider the full context: dependencies, configuration, environment, and external services

### Making Changes
- Make code changes only if you have high confidence they can solve the problem
- Test hypotheses systematically rather than making multiple changes at once
- Document your debugging process and findings

## Debugging Techniques

### 1. Console Output Analysis
- Check the console output in the terminal for any errors or issues
- Pay attention to the color-coded logging: INFO (default), WARNING (yellow), ERROR (red), DEBUG (cyan)
- Look for exception stack traces, error messages, and warning indicators
- Review structured event logging for detailed operation tracking

### 2. Inspection and Logging
- Use print statements, logs, or temporary code to inspect program state
- Include descriptive statements or error messages to understand what's happening
- Leverage the existing logging system in `Relias.Assistant.Common` for structured output
- Add temporary debug-level logging to trace execution flow

### 3. Test-Driven Debugging
- Write failing tests that reproduce the issue
- Use tests to isolate the problematic behavior
- Add test statements or functions to test hypotheses
- Ensure tests pass after the fix is implemented

### 4. Incremental Verification
- After making changes, verify with `dotnet build` to check for compilation errors
- Run relevant tests with `dotnet test` to ensure no regressions
- Test one component at a time when possible

## Application-Specific Debugging

### Slack Integration Issues
- Verify Slack tokens are properly configured (`Slack:AppLevelToken`, `Slack:BotToken`)
- Check socket mode connection status in console output
- Review message event handling in `Relias.Assistant.Slack` project
- Test thread-based conversation context is maintained correctly

### AI Service Issues
- Verify Azure OpenAI configuration (`AzureOpenAI:Key`, `AzureOpenAI:Url`)
- Check that tool functions are being registered correctly
- Review AI function callback responses and formatting
- Look for rate limiting or quota issues in Application Insights

### External Service Integration Issues
- Check optional service configurations (Confluence, JIRA, GitHub, VectorStore)
- Review service factory initialization telemetry for success/failure status
- Verify API credentials and endpoints are correct
- Test service degradation (app should continue with reduced functionality if optional services fail)

### Configuration Issues
- Review configuration priority: command-line args > environment variables > user secrets > appsettings.Development.json > appsettings.json
- Check for missing required configuration at startup
- Verify `.env` files are loaded correctly from output directory or project root
- Use `StartupTelemetryCoordinator` output to diagnose configuration load issues

### Telemetry and Observability
- Check Application Insights connection string if telemetry is expected
- Review null telemetry fallback behavior (console-only logging)
- Use telemetry events to trace operation flow and identify bottlenecks
- Look for `TrackCriticalError`, `TrackHandledError`, `TrackServiceError` events

## Iterating Over Issues

### Incremental Fixes
1. Identify the specific issue from console output or test failures
2. Address **one issue at a time**
3. Return control to the user to run the app and verify the fix
4. Do not attempt to fix multiple unrelated issues in one iteration

### Running the Application
- **Do not run the Console application yourself** as it expects user input and runs as a long-lived process
- If you need to test the application, ask the user to run it
- Request the user to test with actual Slack input to verify end-to-end functionality
- Use `.\run-bot.ps1` script for quick local testing (when user runs it)

### CI/CD Debugging
- Review GitHub Actions workflow output for build/test failures
- Check for environment-specific issues (dependencies, runtime versions)
- Verify Docker container builds if using containerized deployment
- Review infrastructure deployment logs in `.iac/` related resources

## Common Issues and Solutions

### Build Failures
- Run `dotnet build Relias.Assistant.sln` to identify compilation errors
- Check for missing using statements or namespace issues
- Verify all project references are correct
- Ensure NuGet packages are restored

### Test Failures
- Run `dotnet test --logger "console;verbosity=minimal"` for focused output
- Review test assertion messages for expected vs actual values
- Check for environment-specific test failures (config, external dependencies)
- Verify mock setups are correct for isolated unit tests

### Runtime Exceptions
- Check for null reference exceptions (uninitialized services or configuration)
- Review exception stack traces to identify the originating component
- Verify dependency injection is configured correctly in `Program.cs`
- Check for unhandled exceptions in async operations

### Performance Issues
- Review telemetry timing data for slow operations
- Check for blocking calls that should be async
- Look for excessive logging or telemetry overhead
- Consider connection pooling and caching for external services

## Tools and Commands

### Build and Test
```bash
# Build the solution
dotnet build Relias.Assistant.sln

# Run all tests
dotnet test Relias.Assistant.sln

# Run tests with minimal output
dotnet test --logger "console;verbosity=minimal"

# Run specific test
dotnet test --filter "FullyQualifiedName~Namespace.Class.MethodName"
```

### Docker Debugging
```bash
# Build and run with docker-compose
docker-compose up --build

# View container logs
docker logs <container-name>

# Execute commands in running container
docker exec -it <container-name> /bin/bash
```

### Git for Context
```bash
# See recent changes
git log --oneline -10

# View changes in a file
git log -p <file-path>

# Check current branch status
git status
```

## Prevention and Best Practices

- Write tests before implementing fixes (TDD approach)
- Use descriptive error messages and logging
- Validate configuration early at startup
- Fail fast for critical errors, gracefully degrade for optional features
- Document assumptions and edge cases
- Keep methods small and testable (single concern per method)
- Use dependency injection for testability and isolation
