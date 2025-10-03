# Memory System Versioning

## Quick Reference

### Current Versions (as of 2025-10-02)

| Component | Version | Location |
|-----------|---------|----------|
| Memory System | 1.0.0 | `.memoryconfig` → `version` |
| Entity Schema | 1.0.0 | `schemas/entity.json` → `version` |
| Observation Schema | 1.0.0 | `schemas/observation.json` → `version` |
| Relation Schema | 1.0.0 | `schemas/relation.json` → `version` |
| Documentation | 1.0.0 | `MEMORY-INSTRUCTIONS.md` → frontmatter |

### What Gets Versioned?

✅ **Schema Formats** - Tracked in JSON schemas with semantic versioning
✅ **System Configuration** - Tracked in `.memoryconfig`
✅ **Documentation** - Tracked in MEMORY-INSTRUCTIONS.md frontmatter
✅ **Entity Content** - Tracked automatically by Git (no version field needed)

### Semantic Versioning

```
MAJOR.MINOR.PATCH
  |     |     |
  |     |     └─ Bug fixes, doc updates (backward-compatible)
  |     └─────── New features, optional fields (backward-compatible)
  └───────────── Breaking changes, removed fields (NOT backward-compatible)
```

### When Schema Changes

**Minor/Patch** (no action required):
- Existing entities continue working
- New features optional
- Update schema version
- Document in changelog

**Major** (migration required):
1. New schema version released
2. Migration script provided
3. 3-month transition period
4. Both versions supported
5. Deprecation warnings shown
6. Old version removed

### Why Not Version Individual Entities?

Git already provides complete version tracking:
- Full change history with diffs
- Blame/annotate for attribution
- Revert capability
- Branch/merge for collaboration
- Commit messages for context

Adding version fields would be redundant and maintenance overhead.

### Best Practices

1. **Schema Changes**: Bump version, document migration path
2. **Content Changes**: Write descriptive Git commit messages
3. **Breaking Changes**: Provide migration scripts and 3-month notice
4. **New Features**: Add as optional fields (backward-compatible)
5. **Tool Development**: Check schema version before processing

### Full Documentation

See [MEMORY-INSTRUCTIONS.md](../global/MEMORY-INSTRUCTIONS.md#versioning-and-evolution) for complete versioning and migration strategy.
