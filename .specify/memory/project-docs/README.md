# Project-Specific Documentation

This directory contains **project-specific reference documentation** for the Spam Removal Agent. These files are **NOT part of the standard spec-kit structure** and were created specifically for this project.

## 📂 Directory Purpose

Standard spec-kit provides:
- `.specify/memory/constitution.md` - Project principles (standard)
- `.specify/templates/` - Specification templates (standard)
- `.specify/scripts/` - Automation scripts (standard)
- `.specify/memory/specs/[feature]/` - Feature-specific documentation (standard)

This `project-docs/` directory contains additional project-specific references that supplement the standard spec-kit workflow.

## 📄 Files in This Directory

### Core References
- **`agent-framework-reference.md`** - Microsoft Agent Framework implementation guide
  - Complete patterns and code examples
  - Agent lifecycle management
  - Multi-agent orchestration
  - Best practices

- **`spec-driven-development.md`** - Spec-Driven Development methodology reference
  - 7-phase workflow explanation
  - Command reference
  - Template usage guide
  - AI-first development practices

### Quick References
- **`quick-reference.md`** - Developer quick reference card
  - Core principles reminder
  - Common code patterns
  - Package dependencies
  - Decision cheat sheet

### Change Documentation
- **`update-summary.md`** - Documentation of initial setup changes
  - What was updated
  - Integration details
  - Constitutional changes

## 🎯 How to Use These Documents

1. **Start with**: `constitution.md` (parent directory) - immutable principles
2. **Learn workflow**: `spec-driven-development.md` - understand the process
3. **Implement features**: `agent-framework-reference.md` - technical patterns
4. **Quick lookup**: `quick-reference.md` - daily development reference

## 📋 Standard vs. Project-Specific

| File/Directory | Type | Location |
|----------------|------|----------|
| `constitution.md` | **Standard** | `.specify/memory/` |
| `specs/[feature]/` | **Standard** | `.specify/memory/specs/` |
| `templates/` | **Standard** | `.specify/templates/` |
| `scripts/` | **Standard** | `.specify/scripts/` |
| `project-docs/` | **Project-Specific** | `.specify/memory/project-docs/` |

## ✅ Naming Conventions

All files follow lowercase-with-hyphens naming:
- ✅ `agent-framework-reference.md`
- ✅ `spec-driven-development.md`
- ✅ `quick-reference.md`
- ✅ `update-summary.md`
- ❌ ~~`QUICK-REFERENCE.md`~~ (old, inconsistent)
- ❌ ~~`UPDATE-SUMMARY.md`~~ (old, inconsistent)

This matches the standard spec-kit convention (e.g., `constitution.md`, `spec.md`, `plan.md`).

---

**Note**: These files can be modified or removed without affecting the standard spec-kit workflow. They exist solely to provide additional context for this specific project.
