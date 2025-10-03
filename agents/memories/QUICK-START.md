# Memory System Quick Start for AI Agents

## 🚀 Start Every Conversation

```
1. Search memory for context: "Find information about [user's topic]"
2. Use retrieved context to inform responses
3. Avoid re-explaining what's already in memory
```

## 📖 Reading Memory

**Quick searches:**
- `"What do we know about [project/concept/person]?"`
- `"Find decisions related to [topic]"`
- `"Show me information about [entity]"`

**Search the filesystem:**
- Browse `/agents/memories/entities/` directories
- Read relevant .md files
- Follow `[[Relations]]` between entities

## ✍️ Writing Memory (Be Selective!)

### The "Future Value Test"

Before writing, ask:
1. **Reusable?** Will this help in future similar situations?
2. **Non-obvious?** Would I need to research this again?
3. **Context?** Does this provide continuity across conversations?
4. **Unique?** Is this NOT already documented elsewhere?
5. **Lasting?** Will this be relevant in 3+ months?

**Need 3+ YES → Record it | Mostly NO → Skip it**

### When to Write

**✅ HIGH VALUE - Record:**
- Architectural decisions (with rationale)
- Complex problem solutions (non-obvious)
- Reusable patterns and conventions
- Project context spanning conversations
- Important constraints and gotchas
- Team preferences and standards

**❌ LOW VALUE - Skip:**
- Routine CRUD or boilerplate
- Trivial bug fixes (typos, simple errors)
- Temporary status or assignments
- Information in docs/README
- Over-specific details (line numbers)
- Obvious best practices

**Quick create:**
```markdown
Create: agents/memories/entities/[type]/[name].md

---
title: Entity Name
type: decision|project|concept|person|issue|resource
permalink: kebab-case-name
created: 2025-10-02T12:00:00Z
updated: 2025-10-02T12:00:00Z
tags: [tag1, tag2]
---

## Observations
- [category] Observation text #tag

## Relations
- relation_type [[Other Entity]] (context)
```

**Observation categories:**
- `[decision]` - Choices made
- `[fact]` - Objective information
- `[tip]` - Best practices
- `[issue]` - Problems encountered
- `[constraint]` - Limitations
- `[preference]` - Team preferences
- `[resource]` - External references
- `[question]` - Open questions

**Common relations:**
- `documented_in` - Documentation location
- `implements` - Implementation of concept
- `depends_on` - Dependencies
- `relates_to` - General association
- `supersedes` - Replaces older entity
- `inspired_by` - Source of ideas

## 💡 Best Practices

1. **Search before creating** - Check if entity exists
2. **Be specific** - Atomic observations, one fact per line
3. **Add context** - Include rationale and reasoning
4. **Use tags** - Make entities discoverable
5. **Link entities** - Establish relations
6. **Keep current** - Update as information changes
7. **Quality over quantity** - Don't record everything

## 📂 Entity Types

| Type | Use For | Location |
|------|---------|----------|
| `project` | Software projects, features | `entities/projects/` |
| `person` | Team members, stakeholders | `entities/people/` |
| `decision` | Architectural decisions | `entities/decisions/` |
| `concept` | Technical concepts, patterns | `entities/concepts/` |
| `issue` | Problems, bugs | `entities/issues/` |
| `resource` | Docs, tools, references | `entities/resources/` |
| `meeting` | Meeting notes | `entities/meetings/` |
| `task` | Work items | `entities/tasks/` |

## 🔍 Example Workflow

```
User: "How should we handle authentication?"

1. CHECK MEMORY:
   Search: "authentication system decisions"
   Found: entities/decisions/jwt-authentication-approach.md

2. USE CONTEXT:
   "Based on our previous decision (jwt-authentication-approach),
   we're using JWT tokens for stateless authentication..."

3. UPDATE MEMORY:
   Add observation: [fact] Applied JWT pattern to user service #implementation
```

## 📚 Full Documentation

- **Complete Guide**: `agents/global/MEMORY-INSTRUCTIONS.md`
- **Versioning**: `agents/memories/VERSIONING.md`
- **Examples**: `agents/memories/entities/projects/memory-system-implementation.md`

---

**Remember**: Memory makes you smarter over time. Use it consistently!
