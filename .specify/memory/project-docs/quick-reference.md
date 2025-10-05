# Quick Reference - Spam Removal Agent

## 🎯 Core Principles
1. **Conservative**: ≥0.90 confidence to delete
2. **Graph SDK Only**: No IMAP/POP3
3. **Agent Framework**: Microsoft.Agents.AI
4. **Spec-Driven**: Start with `/specify`
5. **Test-First**: TDD mandatory

## 📋 SDD Commands
| Command | Output |
|---------|--------|
| `/constitution` | `constitution.md` |
| `/specify` | `spec.md` (what/why) |
| `/clarify` | De-risk ambiguities |
| `/plan` | `plan.md`, models, contracts |
| `/tasks` | `tasks.md` (ordered) |
| `/analyze` | Validation report |
| `/implement` | Code (TDD) |

## 🤖 Agent Patterns
```csharp
// Create agent
var agent = chatClient.CreateAIAgent(instructions, name, tools);

// Execute with thread
var thread = agent.GetNewThread();
var response = await agent.RunAsync("Analyze spam", thread, ct);

// Hosted service
public class AgentService : BackgroundService {
    protected override async Task ExecuteAsync(CancellationToken ct) {
        var thread = _agent.GetNewThread();
        while (!ct.IsCancellationRequested)
            await _agent.RunAsync("Check emails", thread, ct);
    }
}
```

## 📦 Key Packages
- `Microsoft.Agents.AI` (agent framework)
- `Microsoft.Graph` + `Azure.Identity` (email access)
- `Microsoft.Extensions.Hosting` (background service)
- `Microsoft.ApplicationInsights` (telemetry)

## 🏗️ Structure
```
src/SpamRemovalAgent/
├── Agents/          # AIAgent implementations
├── Services/        # GraphService, SpamDetectionService
├── Models/          # Email, SpamClassification
└── Hosting/         # BackgroundService

.specify/memory/
├── constitution.md  # Principles
└── specs/[feature]/ # spec.md, plan.md, tasks.md
```

## 🔍 Key Decisions
- **Threshold**: ≥0.90 confidence to delete
- **Auth**: OAuth 2.0, Mail.ReadWrite only
- **Thread**: Reset hourly
- **Telemetry**: Application Insights + Aspire
- **Rate Limiting**: Exponential backoff

## 💡 Code Snippets
```csharp
// Conservative deletion
if (result.IsSpam && result.Confidence >= 0.90)
    await DeleteEmailAsync(email);

// Retry with exponential backoff
for (int i = 1; i <= 3; i++) {
    try { return await operation(); }
    catch when (i < 3) { await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i))); }
}

// Thread reset
if (_thread == null || DateTime.UtcNow - _created > TimeSpan.FromHours(1))
    _thread = _agent.GetNewThread();
```

## ⚠️ Common Mistakes
- ❌ Skip `/specify` → ✅ Always start with spec
- ❌ Implementation before tests → ✅ TDD mandatory
- ❌ Use IMAP/POP3 → ✅ Graph SDK only
- ❌ Delete <0.90 confidence → ✅ Conservative threshold
- ❌ Plaintext tokens → ✅ Windows Credential Manager

## 🔗 Links
- **Spec Kit**: https://github.com/github/spec-kit
- **Agent Framework**: https://github.com/microsoft/agent-framework
- **Docs**: `.specify/memory/project-docs/`
