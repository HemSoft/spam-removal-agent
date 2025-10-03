# Agents Memories

This directory contains the persistent memory system for AI agents working on this repository.

## Overview

The memory system stores knowledge as a version-controlled knowledge graph, enabling:
- **Persistent Context**: AI agents can build on previous conversations
- **Team Collaboration**: Share accumulated knowledge across team members
- **Local-First**: All data remains in the repository, under your control
- **Bi-directional**: Both humans and AI can read and write memories

## Directory Structure

```
/agents/memories/
├── entities/              # Memory entities organized by type
│   ├── projects/         # Software projects and features
│   ├── people/           # Team members and stakeholders
│   ├── decisions/        # Architectural and design decisions
│   ├── concepts/         # Technical concepts and patterns
│   └── resources/        # Documentation and references
├── schemas/              # JSON schemas for validation
│   ├── entity.json       # Entity metadata schema
│   ├── observation.json  # Observation schema
│   └── relation.json     # Relation schema
└── .memoryconfig         # Configuration file
```

## Quick Start

### Reading Memories

To search for information:
```
"Find information about [topic]"
"What do we know about [concept]"
"Show me recent work on [project]"
```

### Writing Memories

To create or update memories:
```
"Create a note about [topic]"
"Record this decision: [decision]"
"Update [entity] with this information"
```

### Examples

See the example entity:
- [`entities/projects/memory-system-implementation.md`](entities/projects/memory-system-implementation.md)

## Memory Format

Entities are stored as Markdown files with YAML frontmatter:

```markdown
---
title: Entity Title
type: entity-type
permalink: unique-identifier
created: 2025-10-02T12:00:00Z
updated: 2025-10-02T12:00:00Z
tags:
  - tag1
  - tag2
---

# Entity Title

## Observations

- [category] Observation content #tag (context)

## Relations

- relation_type [[Related Entity]] (context)

## Notes

Additional free-form content...
```

## Memory Types

### Entity Types
- **project**: Software projects, features, or modules
- **person**: Team members, stakeholders, or contacts
- **decision**: Architectural or design decisions
- **concept**: Technical concepts, patterns, or approaches
- **issue**: Problems, bugs, or concerns
- **resource**: Documentation, tools, or references
- **meeting**: Meeting notes or discussions
- **task**: Work items or action items

### Observation Categories
- `[fact]`: Objective information
- `[decision]`: Choices made and rationale
- `[preference]`: User or team preferences
- `[constraint]`: Limitations or requirements
- `[tip]`: Best practices or recommendations
- `[issue]`: Problems or concerns
- `[resource]`: External references
- `[question]`: Open questions
- `[experiment]`: Trials or tests

### Common Relations
- `depends_on`: Technical or logical dependencies
- `relates_to`: General associations
- `implements`: Implementation relationships
- `supersedes`: Replacements or evolution
- `documents`: Documentation relationships
- `used_by`: Usage relationships
- `owned_by`: Ownership or responsibility
- `blocked_by`: Impediments

## Configuration

Edit `.memoryconfig` to customize:
- Storage paths
- Search parameters
- Git integration settings
- Pruning policies
- Privacy controls

## Best Practices

### For AI Agents
1. Start conversations by retrieving relevant context
2. Update memories during conversations
3. Create new entities for new topics
4. Establish relations as they become clear
5. Use quality over quantity

### For Humans
1. Review memory files regularly in your editor
2. Correct inaccuracies as you find them
3. Use consistent terminology
4. Commit changes with clear messages
5. Don't store sensitive data (credentials, PII)

## Documentation

For comprehensive documentation, see:
- [`SHOULD-I-RECORD.md`](./SHOULD-I-RECORD.md) - Decision tree for recording memories 🎯
- [`QUICK-START.md`](./QUICK-START.md) - Quick reference for AI agents ⚡
- [`MEMORY-INSTRUCTIONS.md`](../global/MEMORY-INSTRUCTIONS.md) - Complete implementation guide
- [`VERSIONING.md`](./VERSIONING.md) - Schema versioning and migration strategy

## Maintenance

### Indexing
Memory files are indexed for fast search. Indices are stored in `index/` (gitignored).

### Validation
Entities are validated against JSON schemas in `schemas/` to ensure consistency.

### Archiving
Old or unused entities can be archived to keep the active memory focused.

## Getting Help

Questions or issues with the memory system?
1. Check the [MEMORY-INSTRUCTIONS.md](../global/MEMORY-INSTRUCTIONS.md) documentation
2. Review example entities in `entities/`
3. Consult JSON schemas in `schemas/`
4. Ask the AI agent for help with memory operations

---

**Note**: The `index/` directory is gitignored as it contains derived data that can be regenerated from the source Markdown files.
