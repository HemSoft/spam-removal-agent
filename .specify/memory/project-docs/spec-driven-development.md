# Spec-Driven Development (SDD) Reference

**Source**: https://github.com/github/spec-kit
**Methodology**: AI-First, Specification-Driven Software Development
**Last Updated**: 2025-10-05

## What is Spec-Driven Development?

Spec-Driven Development (SDD) is a methodology where **specifications become executable artifacts** that directly drive implementation rather than serving as passive documentation. Instead of code being king and specs being scaffolding, SDD inverts this relationship: **specifications drive code generation**.

### The Power Inversion

Traditional development treats specifications as guides for coding - they're written, then set aside as developers manually translate intent into implementation. This creates an inevitable gap between what was specified and what was built.

SDD eliminates this gap by making specifications precise, complete, and unambiguous enough to generate working systems. The specification becomes the primary artifact. Code becomes its expression in a particular language and framework.

### Key Principles

1. **Specifications as Lingua Franca**: The specification is the primary artifact; code is its expression
2. **Executable Specifications**: Specs must be precise enough to generate working systems
3. **Continuous Refinement**: Consistency validation happens continuously, not as a one-time gate
4. **Research-Driven Context**: Gather technical context throughout specification
5. **Bidirectional Feedback**: Production reality informs specification evolution
6. **Constitutional Guidance**: Immutable architectural principles guide all specifications

## The SDD Workflow

### Phase 1: Constitution
**Command**: `/constitution`

Establish immutable architectural principles that guide all development:
- Core design philosophy
- Non-negotiable requirements (e.g., conservative spam detection)
- Technology stack mandates (e.g., Microsoft Graph, Agent Framework)
- Security and quality standards
- Governance and amendment processes

**Output**: `.specify/memory/constitution.md`

**For Spam Removal Agent**:
```markdown
### IV. Conservative Spam Detection (NON-NEGOTIABLE)
Spam detection MUST prioritize false negatives over false positives.
The agent MUST NOT delete legitimate emails under any circumstances.
```

### Phase 2: Specification
**Command**: `/specify <feature-description>`

Create detailed feature specifications focusing on WHAT and WHY, not HOW:

**Template Structure**:
- Feature Overview & Goals
- User Stories & Personas
- Functional Requirements
- User Workflows
- Acceptance Criteria
- Non-Functional Requirements
- Out of Scope
- Dependencies & Constraints
- Success Metrics

**Guidelines**:
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement (no tech stack, APIs, code structure)
- 👥 Written for business stakeholders, not developers
- Mark ambiguities with `[NEEDS CLARIFICATION: specific question]`

**Output**: `.specify/memory/specs/[feature-name]/spec.md`

**Example for Spam Removal**:
```text
/specify Autonomous spam detection system that monitors Outlook mailbox
and removes spam emails. The system must be conservative in detection -
it's better to miss spam than delete legitimate emails. All deletions
must be logged with reasoning.
```

### Phase 3: Clarification
**Command**: `/clarify`

Run structured clarification workflow to de-risk ambiguous areas:
- Sequential, coverage-based questioning
- Records answers in Clarifications section
- Must be run before `/plan` unless explicitly skipped
- Reduces rework downstream

**When to Skip**: Explicitly state if doing exploratory/spike work where ambiguity is acceptable.

**Example Questions**:
```text
Q: What confidence threshold should trigger deletion?
A: Confidence must be >= 0.90 (90%) to delete

Q: How should the system handle network failures?
A: Gracefully retry with exponential backoff, never crash

Q: What happens to deleted emails?
A: Move to Deleted Items folder (soft delete), not permanent deletion
```

### Phase 4: Technical Planning
**Command**: `/plan <tech-stack-and-architecture>`

Generate comprehensive implementation plans from specifications:

**Template Structure**:
- Feature Requirements Summary
- Technical Approach & Architecture
- Technology Stack with Rationale
- Component Design
- Implementation Phases
- Testing Strategy
- Deployment Considerations
- Monitoring & Observability

**Constitutional Compliance**:
```markdown
### Phase -1: Pre-Implementation Gates
#### Simplicity Gate
- [ ] Using ≤3 projects?
- [ ] No future-proofing?

#### Anti-Abstraction Gate
- [ ] Using framework directly?
- [ ] Single model representation?
```

**Output**:
- `.specify/memory/specs/[feature-name]/plan.md`
- `.specify/memory/specs/[feature-name]/data-model.md`
- `.specify/memory/specs/[feature-name]/contracts/`
- `.specify/memory/specs/[feature-name]/research.md`
- `.specify/memory/specs/[feature-name]/quickstart.md`

**Example**:
```text
/plan Use .NET 10 console application with Microsoft Agent Framework
for autonomous operation. Microsoft Graph SDK for email access.
Application Insights for telemetry. Multi-layer spam detection with
confidence scoring. Conservative decision-making with threshold >= 0.90.
```

### Phase 5: Task Breakdown
**Command**: `/tasks`

Generate actionable task lists from implementation plan:

**Process**:
1. Analyze `plan.md`, `data-model.md`, `contracts/`
2. Convert requirements into specific tasks
3. Mark independent tasks with `[P]` for parallelization
4. Define dependency chains
5. Outline validation steps

**Output**: `.specify/memory/specs/[feature-name]/tasks.md`

**Task Structure**:
```markdown
## Phase 1: Project Setup [SERIAL]
- [P] Create solution structure
- [P] Add NuGet packages
- [P] Configure dependency injection
- [ ] Create configuration schema (depends on packages)

## Phase 2: Core Services [PARALLEL GROUP]
- [P] Implement GraphService wrapper
- [P] Implement SpamDetectionService
- [P] Implement RuleEngine
```

### Phase 6: Consistency Analysis
**Command**: `/analyze`

Validate alignment and consistency across all artifacts:

**Checks**:
- Requirements traceability (spec → plan → tasks)
- Constitutional compliance
- Missing edge cases or error handling
- Incomplete test coverage
- Ambiguous specifications

**When to Run**: After `/tasks`, before `/implement`

**Output**: Analysis report with recommendations

### Phase 7: Implementation
**Command**: `/implement`

Execute implementation following the plan and tasks:

**Process**:
1. Follow task list in order
2. Implement contract tests first (TDD)
3. Create implementation to pass tests
4. Validate against acceptance criteria
5. Update documentation as needed

**Test-First Mandate**:
```text
No implementation code shall be written before:
1. Unit tests are written
2. Tests are validated and approved
3. Tests are confirmed to FAIL (Red phase)
```

## Template-Driven Quality

Templates constrain AI behavior toward better specifications:

### 1. Preventing Premature Implementation Details
```text
- ✅ Focus on WHAT users need and WHY
- ❌ Avoid HOW to implement
```
Forces proper abstraction levels - specs remain stable even as tech changes.

### 2. Explicit Uncertainty Markers
```text
Mark all ambiguities: Use [NEEDS CLARIFICATION: specific question]
```
Prevents AI from making plausible but incorrect assumptions.

### 3. Structured Thinking Through Checklists
```markdown
### Review and Acceptance Checklist
- [ ] All functional requirements testable and unambiguous
- [ ] User workflows clearly documented
- [ ] Edge cases and error conditions addressed
- [ ] Success metrics defined
- [ ] Dependencies identified
```

### 4. Constitutional Compliance Gates
```markdown
#### Simplicity Gate (Article VII)
- [ ] Using ≤3 projects?
- [ ] No future-proofing?
```
Prevents over-engineering by requiring explicit justification for complexity.

### 5. Test-First Thinking
```markdown
### File Creation Order
1. Create `contracts/` with API specifications
2. Create test files: contract → integration → e2e → unit
3. Create source files to make tests pass
```

## Directory Structure

```
.specify/
├── memory/
│   ├── constitution.md              # Immutable architectural principles
│   ├── agent-framework-reference.md # Microsoft Agent Framework guide
│   ├── spec-driven-development.md   # This document
│   └── specs/                       # Feature specifications
│       └── [feature-name]/
│           ├── spec.md              # Feature specification
│           ├── plan.md              # Implementation plan
│           ├── tasks.md             # Task breakdown
│           ├── data-model.md        # Data structures
│           ├── research.md          # Technology research
│           ├── quickstart.md        # Key validation scenarios
│           └── contracts/           # API contracts
│               ├── api-spec.json
│               └── events-spec.md
├── templates/                       # Specification templates
│   ├── spec-template.md
│   ├── plan-template.md
│   └── tasks-template.md
└── scripts/                         # Automation scripts
    ├── specify.sh / specify.ps1
    ├── plan.sh / plan.ps1
    └── tasks.sh / tasks.ps1
```

## Example Workflow

```bash
# 1. Specify feature
/specify Multi-layer spam detection with confidence scoring

# 2. Clarify ambiguities
/clarify  # Resolve configuration, thresholds, notification requirements

# 3. Create technical plan
/plan Use strategy pattern for detection layers, IOptions for config

# 4. Generate tasks
/tasks  # Creates numbered, dependency-ordered task list

# 5. Validate alignment
/analyze  # Check spec-plan-task consistency

# 6. Implement
/implement  # Execute tasks using TDD
```

## Best Practices for AI-First Development

### 1. Be Explicit About Ambiguity
Don't let AI guess - mark everything unclear:
```text
User authentication method: [NEEDS CLARIFICATION: email/password, SSO, OAuth?]
```

### 2. Separate Concerns
- Specifications = WHAT and WHY (business logic)
- Plans = HOW (technical implementation)
- Tasks = WHEN (execution order)

### 3. Constitutional Alignment
Every plan must reference constitutional principles:
```markdown
## Constitutional Compliance
✅ Conservative Detection (Article IV): Threshold >= 0.90
✅ Comprehensive Logging (Article V): All decisions logged
✅ Microsoft Graph (Article II): Using Graph SDK, no IMAP
```

### 4. Iterative Refinement
Specifications evolve:
```text
v1: Basic spam detection
v2: Add multi-layer validation (this feature)
v3: Machine learning integration (future)
```

### 5. Traceability
Every line of code traces back to:
- Specification requirement
- Plan decision
- Task item
- Constitutional principle

## Command Reference

| Command | Purpose | Input | Output |
|---------|---------|-------|--------|
| `/constitution` | Define principles | Architectural requirements | `constitution.md` |
| `/specify` | Create spec | Feature description | `specs/[name]/spec.md` |
| `/clarify` | De-risk ambiguity | Specification | Updated spec with clarifications |
| `/plan` | Technical design | Tech stack preferences | `plan.md`, `data-model.md`, `contracts/` |
| `/tasks` | Task breakdown | Implementation plan | `tasks.md` |
| `/analyze` | Validate alignment | All artifacts | Analysis report |
| `/implement` | Execute tasks | Task list | Working implementation |

## Why SDD for AI Development

SDD optimizes AI-powered development through:
- **Structure**: Templates constrain AI toward consistent patterns
- **Context**: Constitutional principles guide decisions
- **Iteration**: Change spec → regenerate plan → update code
- **Traceability**: Every implementation traces to specification

## Resources

- **Spec Kit**: https://github.com/github/spec-kit
- **Templates**: `.specify/templates/`
- **Examples**: https://github.com/github/spec-kit/tree/main/examples
