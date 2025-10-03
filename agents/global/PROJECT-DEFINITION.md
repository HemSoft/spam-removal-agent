---
title: "Project Definition"
version: "1.0.1"
lastModified: "2025-09-30"
author: "Franz Hemmer"
purpose: "High-level mission, architecture, extension points, and operational model of Relias Assistant."
---

# PROJECT DEFINITION – Relias Assistant

Purpose-built reference guide for future automation and contributors. Complements (does not duplicate) existing agent docs. This file focuses on: domain intent, high‑level architecture, extension seams, operational model, and telemetry conventions.

---
## TL;DR – Quick Reference

**What:** Slack-integrated AI assistant (codename "Yakob") providing conversational access to Relias engineering knowledge through Azure OpenAI.

**Core Stack:** .NET 8.0, Azure OpenAI (Microsoft.Extensions.AI), Slack Socket Mode, Azure AI Search

**Key Integrations:** Confluence, JIRA, GitHub (Octokit), Vector Store (proprietary docs)

**Architecture:** 4-layer modular design (Console host → Slack adapter → AI orchestration → Common utilities)

**Extension Pattern:** Add service interface → factory → conditional initialization → tool function registration → telemetry

**Resilience:** Optional services fail independently; null telemetry fallback; conditional tool registration

**Testing:** xUnit + Moq; test coverage via coverlet/ReportGenerator; see `agents/global/TESTING.md`

**Config Priority:** CLI args > env vars > user secrets > appsettings.Development.json > appsettings.json

**Required Config:** AzureOpenAI keys, Slack tokens. Optional: Confluence, JIRA, GitHub, VectorStore, Application Insights

**JIRA Epic:** [PE-681](https://relias.atlassian.net/browse/PE-681)

---

---
## 1. Problem Statement & Mission
Relias Assistant ("Yakob") provides a unified conversational interface (currently Slack) that lets engineering teams query organizational knowledge (Confluence, JIRA, GitHub, Vector Store documents, internal context) through Azure OpenAI–backed reasoning and structured tool calls. It reduces context switching and accelerates discovery, triage, and knowledge retrieval.

Primary objectives:
- Centralize knowledge access inside developer chat workflows.
- Blend deterministic system connectors with LLM reasoning.
- Provide observable, testable, low-friction extension points.
- Fail soft: degraded capability shouldn’t crash the bot.

Success indicators (informal): reduced time-to-answer, increased Slack usage of bot threads, low unhandled exception count, telemetry coverage of key operations.

Non‑goals (current scope): multi-tenant SaaS hardening, persistent conversation memory across process restarts, fine-grained access control enforcement beyond upstream service permissions.

---
## 2. Core Architectural Overview
High-level components (one project per responsibility):

| Layer | Project | Role |
|-------|---------|------|
| Entry / Runtime | `Relias.Assistant.Console` | Host bootstrap, configuration load, service initialization, startup telemetry, Slack bot launch. |
| Channel Adapter | `Relias.Assistant.Slack` | Socket Mode connection, mention/thread handling, message formatting, dispatch into AI client. |
| AI Orchestration & Tools | `Relias.Assistant.AI` | Wraps Azure OpenAI via `IChatClient`; injects tool functions for Confluence/JIRA/GitHub/Vector store searches; post‑processes model output. |
| Shared Utilities & Telemetry | `Relias.Assistant.Common` | Console logging helpers, telemetry abstraction (`ITelemetryService` + null implementation). |
| Tests | `tests/*` | Unit + integration patterns (telemetry, startup coordination, Slack handling). |

Execution flow (happy path):
1. Console host starts → loads `.env`, JSON, user secrets, env vars.
2. Conditional Application Insights wiring: real vs null telemetry service.
3. Startup telemetry & health check recorded via `StartupTelemetryCoordinator`.
4. External service factories initialize (Confluence, JIRA, GitHub, Vector Store) individually with success/failure telemetry.
5. Slack socket establishes → message events forwarded to `ReliasAssistantChatClient`.
6. Chat client builds tool list dynamically based on which services resolved successfully.
7. Model responses optionally invoke tools; vector results may override empty/low-confidence model replies (post‑processing heuristic).
8. Lifecycle & error events captured via telemetry abstraction; graceful shutdown flushes.

Resilience patterns:
- Service initialization isolation: each external connector can fail independently without aborting startup.
- Null object for telemetry keeps instrumentation calls side-effect-safe.
- Tool registration is conditional—no stub functions when a service is absent.
- Post‑response override logic ensures surfaced retrieval results aren’t lost if the model ignores tool outputs.

---
## 3. Telemetry Strategy (Complement – not redefined)
Use `ITelemetryService` everywhere instead of direct Application Insights calls. Key semantic methods:
- Startup: `TrackApplicationStartup`, `TrackApplicationStartupComplete`.
- Service lifecycle: `TrackServiceInitializationSuccess/Failure`.
- Domain interactions: `TrackSlackMessage`, `TrackAIInteraction`, `TrackDependency`.
- Error taxonomy: `TrackCriticalError`, `TrackHandledError`, `TrackSlackError`, `TrackServiceError` (adds contextual properties, rate limit awareness).

Guidelines:
- Prefer semantic wrappers over raw `TrackEvent` for consistency.
- Add new domain event? Extend interface + both implementations (real & null) and add unit tests mirroring existing pattern (`TelemetryServiceTests`).
- Flush only on shutdown or explicit health check.

---
## 4. Configuration & Runtime Model
Configuration layers merged in order (last wins):
1. `appsettings.json` (+ environment variant if present)
2. User secrets (development convenience)
3. `.env` (loaded from output dir or project root fallback)
4. Environment variables

Minimum required config (validated early):
- `azureOpenAIKey`, `azureOpenAIUrl`, `slackAppLevelToken`, `slackBotToken`
Optional but impactful: Application Insights connection string, VectorStore keys, Confluence/JIRA/GitHub credentials (feature gating via presence).

Operational characteristics:
- Long-lived process awaiting Slack events (cancellation via Ctrl+C).
- No background scheduled jobs (as of this definition).
- Concurrency largely I/O bound (async tool calls, Slack event pipeline). No explicit parallel fan‑out currently.

---
## 5. Extension Points
Add a new knowledge/tool source (e.g., Azure DevOps, PagerDuty):
1. Define interface in `Relias.Assistant.AI` (e.g., `IAdoService`). Keep surface minimal & task-based.
2. Implement service + factory (mirroring existing `*ServiceFactory` pattern) handling configuration validation & dependency injection.
3. Add conditional initialization in `Program.InitializeServicesAsync` + success/failure telemetry calls.
4. Inject into `ReliasAssistantChatClient` constructor parameter list.
5. Add new tool function via `AIFunctionFactory.Create` inside `GetResponseAsync` with descriptive `[Description]` attributes.
6. Provide formatting logic that is concise, markdown friendly, avoids leaking secrets.
7. Update tests: interface contract, failure tolerance, tool registration presence.

Introduce a new channel adapter (e.g., Teams):
- Create new project `Relias.Assistant.Teams` replicating Slack pattern: connection client, message event handler, formatting, thread/context strategy.
- Isolate transformation at boundary: convert channel event → normalized internal message → AI client.
- Reuse `Console` host or add separate host project if lifecycle differs.

Augment telemetry with new semantic events:
- Add strongly named method(s) to `ITelemetryService`.
- Implement in both concrete services.
- Add unit test parity & ensure no unhandled exceptions when telemetry disabled.

---
## 6. Error Handling Principles
- Catch narrow scope around external calls; log + downgrade capability instead of failing the whole pipeline.
- Distinguish critical vs handled: only infrastructure threats (startup failure, unrecoverable host logic) invoke `TrackCriticalError`.
- Provide contextual properties (component, operation, identifiers) to facilitate search & aggregation.

---
## 7. Testing Strategy (Augments `TESTING.md`)
Existing patterns:
- Interface parity testing (ensures null & real telemetry remain aligned).
- Lifecycle sequencing tests (`StartupTelemetryCoordinatorTests`).
- Slack event behavior tests for message handler.

When adding features:
- Favor constructor guard clause tests (argument validation) for new services.
- For tool functions: test formatting + fallback scenarios (empty result set, exception path returning user-friendly message).
- Provide regression test when adding post‑processing heuristics.

---
## 8. Performance & Scaling Considerations (Current Stage)
- Chat tool invocation is sequential; if tool fan‑out becomes necessary, introduce bounded parallelism with cancellation tokens.
- Vector store post-processing override helps avoid wasted tokens on “no result” hallucinations.
- Telemetry flush delay kept minimal (1s) only when real telemetry enabled—acceptable for current scale.

Future optimizations (deferred):
- Response streaming to channel.
- Caching of frequent documentation hits.
- Adaptive tool ordering based on prior success relevance.

---
## 9. Security & Data Hygiene
- Secrets never logged; only presence/state (Configured/NotConfigured) recorded.
- Anonymous user/machine hashing via SHA256 (first 16 hex chars) for correlation without PII exposure.
- Tool outputs trimmed & normalized to prevent Slack overflow; code ensures truncation at word boundary.
- Rate limit exception metadata extracted reflectively when present.

---
## 10. Assumptions & Open Clarifications
Assumptions made:
- Single-tenant internal deployment; no per-user authorization layer required yet.
- Slack is primary interaction surface; persistence between restarts out-of-scope.
- Azure OpenAI model deployment names stable via config.

Potential clarifications (feedback welcome):
1. Should conversation history window be bounded to mitigate token growth? (Currently accumulates indefinitely)
2. Are there retention / export requirements for telemetry (GDPR-like purging)?
3. Is there a roadmap for multi-channel or persistent thread memory beyond process lifespan?
4. Do we need feature flags for experimental tools instead of config presence heuristics?
5. Expected SLOs (latency, availability) for future productionization?

---
## 11. Quick Reference – Adding a Tool (Checklist)
- [ ] Interface & models defined
- [ ] Service + factory implemented with config validation
- [ ] Conditional initialization + telemetry success/failure
- [ ] Injected into chat client & tool registered with description
- [ ] Friendly error & empty response handling
- [ ] Unit tests for success, empty, error paths
- [ ] Telemetry coverage for calls & dependency timing (optional enhancement)

---
## 12. When NOT to Extend
Avoid expanding surface if:
- Data source is low-signal and increases latency significantly.
- Overlaps significantly with existing tool semantics (consider consolidation).
- Adds complex state without clear persistence requirements solved.

---
## 13. Glossary (Project-Specific)
- Tool Function: A callable operation exposed to the LLM via `AIFunctionFactory`.
- Vector Store: Azure AI Search backed embedding index storing proprietary markdown/internal docs.
- Null Telemetry: Console-only fallback when Application Insights not configured.
- Post-Processing Override: Logic replacing weak model answer with authoritative tool output.

---
## 14. Evolution Notes
The telemetry abstraction and startup coordination emerged from PR-driven refactoring to reduce method complexity in `Program.cs`. Future structural changes should preserve: isolation, conditional capability, and observability-first design.

---
## 15. Meta
This document is intentionally high-level, implementation-agnostic where lower-level standards already exist, and serves as the orientation map for autonomous or human contributors.

> Update cadence: revise upon major architectural shifts (new channel, persistence layer, orchestration redesign) rather than routine feature increments.
