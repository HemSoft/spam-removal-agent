---
title: "AGENTS.md"
version: "1.0.3"
lastModified: "2025-09-30"
author: "Franz Hemmer"
purpose: "Instructions for AI, LLM's, tools and agents."
---

# AGENTS.md -- Instructions for AI LLM's, Agents and Tools.

## Follow instructions contextually according to your task at hand.

## Policy precedence and tool-agnostic use

- These repository instructions are guidance. If any instruction here conflicts with your host's system policies, platform rules, compliance constraints, or safety guardrails, those host/system policies take precedence.
- Treat specific tool names and agent capabilities referenced here as examples. Use the closest equivalent tools and capabilities available in your runtime environment.
- When adapting due to a policy conflict, proceed with the spirit of these instructions, note 1–2 brief assumptions if needed, and avoid blocking unless the conflict is truly blocking.

## CRITICAL: Proactively Load Referenced Instructions into Context

**You MUST read the instruction files listed below into your context window at the start of any task.** These files contain essential guidance that will:

- Provide coding standards, testing requirements, and security practices
- Define workflows and debugging procedures specific to this repository
- Prevent common mistakes and ensure consistency with project standards
- Save time by answering questions before you need to ask them

**When to load these files:**
- **At the beginning of any new conversation or task** - Load all relevant files proactively
- **When starting any implementation work** - Read CODE-STANDARDS.md, TESTING.md, and WORKFLOW.md at minimum
- **When debugging or troubleshooting** - Read DEBUGGING.md and TOOL-USE.md
- **When working with tests or coverage** - Read TESTING.md and DOTCOVER.md
- **When making commits or managing branches** - Read GIT-WORKFLOW.md
- **When handling security-sensitive code** - Read SECURITY.md
- **When uncertain about project architecture** - Read PROJECT-DEFINITION.md

**Do not wait to be explicitly told to read these files.** They are your primary reference documentation and should be in your context for nearly all operations in this repository.

Read all the instruction files below and understand and follow them:

# Global Instructions:

| File | Purpose |
|------|---------|
| [CODE-STANDARDS.md](./agents/global/CODE-STANDARDS.md) | C# coding standards to adhere to |
| [DEBUGGING.md](./agents/global/DEBUGGING.md) | Comprehensive debugging tips and procedures for diagnosing issues |
| [DOTCOVER.md](./agents/global/DOTCOVER.md) | How to run dotcover.exe and derive line/branch coverage percentages |
| [GIT-WORKFLOW.md](./agents/global/GIT-WORKFLOW.md) | Version control standards, branching strategy, and commit conventions |
| [MEMORY-INSTRUCTIONS.md](./agents/global/MEMORY-INSTRUCTIONS.md) | Persistent memory implementation using version-controlled knowledge graphs |
| [PROJECT-DEFINITION.md](./agents/global/PROJECT-DEFINITION.md) | High-level project mission, architecture, and extension points |
| [RULES-AND-STANDARDS.md](./agents/global/RULES-AND-STANDARDS.md) | General standards and guardrails to follow |
| [SECURITY.md](./agents/global/SECURITY.md) | Security practices, secret management, and data protection |
| [TESTING.md](./agents/global/TESTING.md) | Guidance for writing and running tests |
| [TOOL-USE.md](./agents/global/TOOL-USE.md) | Guidance on using the terminal, MCP servers, and external tools |
| [WORKFLOW.md](./agents/global/WORKFLOW.md) | Step-by-step operational workflow for problem solving and implementation |

# Project Specific Instructions:

| File | Purpose |
|------|---------|
| [REFERENCE-PROJECTS.md](./agents/project/REFERENCE-PROJECTS.md) | Instructions for using POC projects in the ./ref folder |
