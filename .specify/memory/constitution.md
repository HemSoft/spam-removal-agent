<!--
Sync Impact Report:
- Version change: 1.0.0 → 1.1.0 (Added .NET Aspire and Application Insights requirements)
- Modified principles: V. Comprehensive Logging & Observability (expanded with Application Insights)
- Added sections: Technology Stack Requirements
- Removed sections: none
- Templates requiring updates: ✅ plan template updated / ✅ copilot instructions updated
- Follow-up TODOs: none
-->

# Spam Removal Agent Constitution

## Core Principles

### I. Autonomous Operation
The agent MUST operate autonomously without human intervention. It continuously monitors the mailbox and removes spam until all spam is eliminated or explicitly stopped.

**Rationale**: Continuous protection without manual oversight.

### II. Microsoft Graph Integration
All email operations MUST use Microsoft Graph .NET SDK. IMAP/POP3 connections are prohibited. Authentication MUST use OAuth 2.0 with Mail.ReadWrite permission.

**Rationale**: Secure, standardized Outlook access.

### III. Agent Framework Architecture
The application MUST be built using the Microsoft Agent Framework (https://github.com/microsoft/agent-framework) to ensure proper agent lifecycle management, decision-making capabilities, and extensibility.

**Implementation Requirements**:
- Use `Microsoft.Agents.AI` core package for `AIAgent` abstractions
- Implement agents using `AIAgentBuilder` or provider-specific extensions
- Use `AgentThread` for maintaining conversation context and email processing history
- Integrate agents with .NET hosting infrastructure (`IHostedService`)
- Design agents with middleware support for cross-cutting concerns (logging, telemetry, rate limiting)
- Follow Agent Framework patterns:
  - **Single Agent Pattern**: Primary spam detection agent with autonomous decision-making
  - **Thread-based Execution**: Use agent threads for maintaining processing context
  - **Tool Integration**: Expose spam detection functions as agent tools
  - **Future-Ready**: Design with multi-agent workflows in mind for future enhancements

**Rationale**: Microsoft Agent Framework provides proven patterns for autonomous AI agent development with proper lifecycle management, enabling sophisticated spam detection logic with extensibility for future AI/ML enhancements. The framework's multi-agent orchestration capabilities support future advanced scenarios.

### IV. Conservative Spam Detection (NON-NEGOTIABLE)
The agent MUST prioritize false negatives over false positives. NEVER delete legitimate emails. Require ≥0.90 confidence threshold. Log all deletions with reasoning.

**Rationale**: Better to miss spam than delete important messages.

### V. Comprehensive Logging & Observability
All agent actions MUST be logged with structured logging: emails processed, decisions, confidence scores, errors. Application Insights MUST be integrated for telemetry and tracing.

**Rationale**: Enable monitoring, debugging, and continuous improvement.

## Technology Stack Requirements

- **.NET 10 C# Console Application** with modern language features
- **.NET Aspire** for orchestration and development observability
- **Application Insights** for production telemetry and tracing
- **Local Windows 10 deployment** (no cloud/containers required)

**Rationale**: Aspire for dev-time visibility, App Insights for production monitoring.

## Security Requirements

- Store OAuth tokens in Windows Credential Manager (never plaintext)
- Request minimal permissions (Mail.ReadWrite only)
- Process emails locally (no external service transmission)

## Quality Standards

- Graceful error handling with exponential backoff
- Target 100+ emails/minute respecting API quotas
- Comprehensive unit and integration tests

## Development Methodology

### Spec-Driven Development (SDD)
This project follows the Spec-Driven Development methodology using GitHub Spec Kit (https://github.com/github/spec-kit), where specifications are executable artifacts that drive implementation rather than passive documentation.

**Core SDD Principles**:
1. **Specifications First**: All features begin with clear specifications defining WHAT and WHY before HOW
2. **Executable Specifications**: Specifications are detailed enough to directly generate implementation plans and code
3. **Constitution-Guided**: This constitution provides immutable architectural principles that guide all specifications
4. **Continuous Refinement**: Specifications evolve through structured clarification and validation
5. **Test-First Thinking**: Acceptance criteria and test scenarios are integral to specifications

**SDD Workflow**:
1. **`/constitution`** - Establish or update architectural principles (this document)
2. **`/specify`** - Create baseline feature specifications in `.specify/memory/specs/`
3. **`/clarify`** - Run structured clarification to de-risk ambiguous requirements
4. **`/plan`** - Generate detailed technical implementation plans from specifications
5. **`/tasks`** - Break down plans into actionable task lists
6. **`/analyze`** - Validate consistency and coverage across artifacts
7. **`/implement`** - Execute implementation following the plan

**Specification Storage**: All specifications reside in `.specify/memory/specs/[feature-name]/` with supporting documentation (plans, tasks, data models, contracts).

**AI-First Development**: Specifications are written for AI interpretation, using structured templates that guide AI agents toward high-quality, consistent outputs aligned with constitutional principles.

**Rationale**: SDD transforms specifications from static documentation into executable artifacts that drive implementation. This ensures alignment between intent and code, enables rapid iteration, and supports parallel exploration of implementation approaches. The methodology is particularly effective with AI-powered development tools.

## Governance

This constitution supersedes all other development practices and architectural decisions. All code changes MUST comply with these principles. Any deviation requires explicit justification and documentation.

**Amendment Process**: Constitution changes require version increment and impact analysis. Breaking changes to core principles require major version bump. New principles or clarifications warrant minor version increment.

**Compliance Review**: All pull requests MUST verify adherence to constitutional principles, especially conservative spam detection and security requirements. Code reviews MUST validate proper Graph API usage, Agent Framework patterns, and logging implementation.

**Specification Alignment**: All implementations MUST trace back to approved specifications in `.specify/memory/specs/`. No features shall be implemented without prior specification and plan approval.

**Runtime Guidance**: Use `.github/copilot-instructions.md` for day-to-day development guidance that aligns with these constitutional principles.

**Version**: 1.2.0 | **Ratified**: 2025-10-03 | **Last Amended**: 2025-10-05
