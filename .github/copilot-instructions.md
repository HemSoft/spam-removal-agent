# Spam Removal Agent - GitHub Copilot Instructions

## Project Overview
This is a .NET 10 C# Console Agent Application that autonomously connects to Outlook mailboxes via Microsoft Graph .NET SDK and removes spam emails. The application uses Microsoft Agent Framework for autonomous operation, .NET Aspire for orchestration and observability, and Application Insights for production telemetry.

**Development Methodology**: This project follows **Spec-Driven Development (SDD)** using GitHub Spec Kit (https://github.com/github/spec-kit), an AI-first approach where specifications are executable artifacts that drive implementation.

## Spec-Driven Development Workflow

All features begin with specifications in `.specify/memory/specs/`, followed by technical plans, and then implementation.

### Core Workflow Commands
1. `/constitution` - Establish or update project principles
2. `/specify` - Create feature specifications (WHAT and WHY, not HOW)
3. `/clarify` - De-risk specification with structured Q&A (run before `/plan`)
4. `/plan` - Generate technical implementation plans from specs
5. `/tasks` - Break down plans into actionable task lists
6. `/analyze` - Validate alignment and consistency (run after `/tasks`, before `/implement`)
7. `/implement` - Execute implementation following the plan

**📘 Detailed Workflow**: See `.specify/memory/project-docs/spec-driven-development.md`

## Constitutional Principles (MUST FOLLOW)

**⚠️ CRITICAL**: All development MUST comply with `.specify/memory/constitution.md`. Key non-negotiable principles:

### 1. **Conservative Spam Detection** (Article IV - NON-NEGOTIABLE)
- Confidence threshold: **≥0.90** (90%) required for deletion
- **NEVER** delete legitimate emails - prefer false negatives over false positives
- Multiple validation layers required
- Log ALL deletions with detailed reasoning

### 2. **Microsoft Graph Only** (Article II)
- Use **Microsoft Graph .NET SDK** exclusively for email operations
- **PROHIBITED**: IMAP, POP3, or other direct email protocols
- OAuth 2.0 authentication with minimal permissions (Mail.ReadWrite)

### 3. **Microsoft Agent Framework** (Article III)
- Use `Microsoft.Agents.AI` for all autonomous operations
- Implement `AIAgent` interface as foundation
- Use `AgentThread` for conversation context and email processing history
- Integrate with .NET hosting (`IHostedService` pattern)

### 4. **Autonomous Operation** (Article I)
- Design for continuous, unattended operation
- Graceful error handling - never crash
- Only terminate when no spam detected or explicitly stopped

### 5. **Comprehensive Logging** (Article V)
- Structured logging with `ILogger<T>` for ALL agent actions
- Include: timestamps, email metadata, confidence scores, decision rationale
- Application Insights integration required for production telemetry

**📜 Full Constitution**: See `.specify/memory/constitution.md` for complete details, rationale, and governance

## Technology Stack

### Core Framework
- **.NET 10 C# Console Application** - Modern C# with top-level statements
- **Microsoft Agent Framework** (https://github.com/microsoft/agent-framework)
  - `Microsoft.Agents.AI` - Core agent abstractions and AIAgent interface
  - `Microsoft.Agents.AI.Workflows` - Multi-agent orchestration (future)
  - `Microsoft.Extensions.AI` - Chat client abstractions
- **Microsoft Graph .NET SDK** - Secure Outlook mailbox access via OAuth 2.0
- **.NET Aspire** - Development orchestration and observability dashboard
- **Application Insights** - Production telemetry, distributed tracing, metrics

### Deployment Target
- **Windows 10 Local Deployment** - No cloud or containerization required
- **Background Service** - Runs as IHostedService with graceful shutdown

**📦 Complete Dependency List**: See project `.csproj` files and architecture documentation

## Microsoft Agent Framework - Quick Reference

**🔗 Complete Guide**: See `.specify/memory/project-docs/agent-framework-reference.md`

### Core Patterns to Use

**Agent Creation**:
```csharp
// Use AIAgentBuilder or provider extensions
AIAgent agent = chatClient.CreateAIAgent(
    instructions: "You are a spam detection specialist...",
    name: "spam-detector"
);
```

**Agent Execution**:
```csharp
// Thread-based conversation context
var thread = agent.GetNewThread();
var response = await agent.RunAsync(message, thread, cancellationToken);
```

**Hosting Integration**:
```csharp
// Register agent in DI container
services.AddSingleton<AIAgent>(sp => /* create agent */);
services.AddHostedService<AgentHostedService>(); // Background service
```

**Middleware**:
- Logging middleware for structured logging
- Rate limiting middleware for Graph API throttling
- Telemetry middleware for Application Insights

**📚 Detailed Patterns**: Agent initialization, execution loops, error handling, middleware implementation - all in `agent-framework-reference.md`

## Suggested Architecture

### High-Level Structure
```
src/
├── SpamRemovalAgent/              # Main console application
│   ├── Agents/                    # AIAgent implementations + middleware
│   ├── Services/                  # GraphService, SpamDetectionService, RuleEngine
│   ├── Authentication/            # OAuth token management + Windows Credential Manager
│   ├── Hosting/                   # IHostedService implementation
│   ├── Observability/             # Application Insights integration
│   ├── Models/                    # Email, SpamClassification, DetectionRule
│   └── Program.cs                 # Entry point with DI setup
├── SpamRemovalAgent.AppHost/      # .NET Aspire orchestration (optional)
└── Tests/
    ├── Unit/                      # Spam detection, rule engine, agent tests
    ├── Integration/               # Graph API, authentication tests
    └── TestUtilities/             # Mocks and test data
```

**🏗️ Detailed Architecture**: Create `docs/architecture.md` during `/plan` phase with complete component diagrams and interactions

## Code Guidelines & Quality Standards

### Security (NON-NEGOTIABLE)
- Store OAuth tokens in **Windows Credential Manager** (never plaintext, never config files)
- Request minimal permissions: **Mail.ReadWrite** only
- Process emails locally - no external service transmission

### Performance & Resilience
- **Target**: 100+ emails/minute processing
- **Rate Limiting**: Respect Graph API quotas with intelligent batching
- **Retry Logic**: Exponential backoff for API failures (2s, 4s, 8s, 16s, 32s max)
- **Circuit Breaker**: Stop retrying after persistent failures
- **Graceful Degradation**: Continue processing remaining emails on partial failures

### Testing Standards (TDD MANDATE)
- **Unit Tests**: All spam detection logic, rule evaluation, confidence calculation
- **Integration Tests**: Microsoft Graph API interactions, OAuth flows
- **Agent Tests**: Agent lifecycle, decision-making, thread management
- **Contract Tests**: Validate Graph API contracts
- **Test Data**: Diverse email samples (obvious spam, edge cases, legitimate emails)

## Development Focus Areas

When implementing features, prioritize these key areas:

### 1. **Conservative Spam Detection** (PRIORITY 1)
- Confidence threshold ≥0.90 required for any deletion
- Multi-layer validation: sender reputation + content analysis + pattern matching
- Detailed confidence scoring with explainability
- Extensible rule system for updates

### 2. **Agent Framework Integration** (PRIORITY 2)
- Use `AIAgent` interface as foundation
- `AgentThread` for email processing context
- `IHostedService` background execution
- Middleware for logging, rate limiting, telemetry

### 3. **Microsoft Graph Operations** (PRIORITY 3)
- OAuth 2.0 with token refresh
- Efficient batching of email operations
- Pagination for large mailboxes
- Rate limiting and exponential backoff

### 4. **Observability** (ONGOING)
- Structured logging with `ILogger<T>` for ALL decisions
- Application Insights: custom metrics, distributed tracing, performance monitoring
- .NET Aspire dashboard for dev-time insights

### 5. **Error Resilience** (ONGOING)
- Graceful network failure handling
- Circuit breaker for persistent failures
- Agent state persistence for recovery
- Never crash - log and continue
