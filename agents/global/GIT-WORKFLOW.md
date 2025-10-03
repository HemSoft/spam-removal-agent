---
title: "Git Workflow"
version: "1.0.0"
lastModified: "2025-09-30"
author: "Franz Hemmer"
purpose: "Version control standards, branching strategy, and commit conventions for Relias Assistant."
---

# Git Workflow - Version control standards and practices

## Branching Strategy

### Main Branches

- **`main`** - Production-ready code; protected branch requiring PR reviews

### Feature Branches

**CRITICAL: All branches must be tied to either a JIRA ticket or a GitHub Issue.**

If you are creating a branch and do not have a JIRA ticket number or GitHub Issue number, **ask the user for this information before proceeding.**

#### Branch Naming Conventions

**For JIRA Tickets:**
```
feature/[TICKET-NUMBER]
bug/[TICKET-NUMBER]
```

**Examples:**
- `feature/PE-904` - Feature branch for JIRA ticket PE-904
- `bug/PE-905` - Bug fix branch for JIRA ticket PE-905

**For GitHub Issues:**
```
issue-[issue-number]-[short-description-title]
```

**Examples:**
- `issue-42-add-teams-integration` - GitHub Issue #42
- `issue-135-fix-slack-threading` - GitHub Issue #135

### Branch Naming Guidelines

- **Always tie branches to JIRA tickets or GitHub Issues** - No branch should be created without a tracking ticket/issue
- **Ask the user** if ticket/issue information is not provided
- Use lowercase with hyphens to separate words
- For JIRA: Use ticket number only (e.g., `feature/PE-904`)
- For GitHub Issues: Include issue number and short hyphenated description (e.g., `issue-42-add-feature`)
- Delete branches after merging to keep repository clean

### Commits and Pushes

**Commits and pushes are only performed when explicitly requested by the user.** Never automatically commit or push changes without user confirmation.

## Commit Message Conventions

### Format

Use conventional commit format for clear, scannable history:

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat** - New feature or functionality
- **fix** - Bug fix
- **refactor** - Code restructuring without changing behavior
- **docs** - Documentation changes
- **test** - Adding or updating tests
- **chore** - Maintenance tasks (dependencies, build config)
- **perf** - Performance improvements
- **style** - Code formatting, whitespace (not CSS)
- **ci** - CI/CD pipeline changes

### Examples

**Simple commit:**
```
feat: add Confluence search integration
```

**With scope:**
```
fix(slack): resolve message threading context issue
```

**With body:**
```
refactor(telemetry): simplify service initialization

Extract telemetry coordination logic into StartupTelemetryCoordinator
to reduce Program.cs complexity and improve testability.
```

**With breaking change:**
```
feat!: change configuration structure for AI services

BREAKING CHANGE: Configuration keys renamed from AzureOpenAI to OpenAI.
Migration guide in CONFIGURATION.md.
```

**With ticket reference:**
```
feat(vector-store): implement semantic search

Implements Azure AI Search integration for proprietary documentation.

Closes PE-681
```

### Guidelines

- Use imperative mood: "add feature" not "added feature"
- Keep subject line under 72 characters
- Capitalize subject line
- No period at the end of subject
- Separate subject from body with blank line
- Wrap body at 72 characters
- Explain **what** and **why**, not **how** (code shows how)
- Reference issues/tickets in footer

## Pull Request Workflow

### Creating Pull Requests

1. **Update from main** before creating PR:
   ```bash
   git checkout main
   git pull origin main
   git checkout feature/your-branch
   git merge main
   ```

2. **Run tests and build**:
   ```bash
   dotnet build Relias.Assistant.sln
   dotnet test Relias.Assistant.sln
   ```

3. **Create PR** using `gh` CLI or GitHub web interface:
   ```bash
   gh pr create --title "feat: your feature description" --body "$(cat <<'EOF'
   ## Summary
   - Bullet point summary of changes

   ## Test plan
   - [ ] Unit tests pass
   - [ ] Manual testing completed
   - [ ] Documentation updated

   EOF
   )"
   ```

### PR Title Format

Use commit message format for PR titles:
```
<type>: <description>
```

**Examples:**
- `feat: add Microsoft Teams integration`
- `fix: resolve configuration loading issue`
- `docs: update README with Docker instructions`

### PR Description Template

```markdown
## Summary
Brief description of changes and motivation.

## Changes
- Specific change 1
- Specific change 2
- Specific change 3

## Testing
- [ ] Unit tests added/updated
- [ ] All tests passing
- [ ] Manual testing completed
- [ ] No breaking changes (or documented if present)

## Related Issues
Closes #123
Relates to PE-681

## Additional Notes
Any additional context, screenshots, or considerations.
```

### Review Process

- Request at least one review before merging
- Address all review comments or provide rationale
- Keep PRs focused and reasonably sized (< 500 lines preferred)
- Update PR description if scope changes during review

## Common Git Operations

### Starting New Work

```bash
# Update main branch
git checkout main
git pull origin main

# Create and switch to new branch
git checkout -b feature/your-feature-name

# Verify branch
git status
```

### Committing Changes

```bash
# Stage specific files
git add src/Relias.Assistant.AI/NewService.cs
git add tests/Relias.Assistant.UnitTests/NewServiceTests.cs

# Or stage all changes (use with caution)
git add .

# Commit with message
git commit -m "feat(ai): add new AI service integration"

# Push to remote
git push -u origin feature/your-feature-name
```

### Keeping Branch Updated

```bash
# Option 1: Merge main into feature branch
git checkout feature/your-branch
git merge main

# Option 2: Rebase on main (rewrites history, use before pushing)
git checkout feature/your-branch
git rebase main
```

### Handling Conflicts

Merge conflicts should generally be handled by the user. Agents may guide users through resolving simple, straightforward conflicts (such as single-line changes with clear intent), but must avoid direct involvement in complex or ambiguous conflicts. In such cases, resolution is strictly the user's responsibility.

## Best Practices

### Commit Frequency

- Commit logically related changes together
- Avoid committing half-finished work
- Commit before switching contexts or branches
- Each commit should leave code in working state

### Code Review

- Review your own changes before creating PR
- Use `git diff` to see what's changed
- Ensure commit history is clean and logical
- Squash fixup commits before merging (if appropriate)

### Repository Hygiene

- Never commit secrets or credentials
- Add sensitive files to `.gitignore`
- Keep `.gitignore` updated
- Delete merged branches
- Tag releases with semantic versioning

### Related Documentation

- [CONFIGURATION.md](../../CONFIGURATION.md) - Setup and configuration
- [DEBUGGING.md](./DEBUGGING.md) - Debugging and troubleshooting
- [CODE-STANDARDS.md](./CODE-STANDARDS.md) - Code quality standards

### External Resources

- [Git Documentation](https://git-scm.com/doc)
- [GitHub CLI Documentation](https://cli.github.com/manual/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [Semantic Versioning](https://semver.org/)
