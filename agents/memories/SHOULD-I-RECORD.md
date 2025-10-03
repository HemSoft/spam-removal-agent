# Should I Record This Memory?

## 🎯 Quick Decision Tree

```
┌─────────────────────────────────────────────────────┐
│  Is this information related to my current work?    │
└──────────────────┬──────────────────────────────────┘
                   │
                   ▼ YES
┌─────────────────────────────────────────────────────┐
│  THE FUTURE VALUE TEST                              │
│  Check each criterion (need 3+ YES to record):      │
├─────────────────────────────────────────────────────┤
│  1. REUSABILITY                                     │
│     Will this help in future similar situations?    │
│     □ YES   □ NO                                    │
│                                                      │
│  2. NON-OBVIOUSNESS                                 │
│     Would I need to research this again?            │
│     □ YES   □ NO                                    │
│                                                      │
│  3. CONTINUITY                                      │
│     Does this provide context across conversations? │
│     □ YES   □ NO                                    │
│                                                      │
│  4. UNIQUENESS                                      │
│     Is this NOT already documented elsewhere?       │
│     □ YES   □ NO                                    │
│                                                      │
│  5. LONGEVITY                                       │
│     Will this still be relevant in 3+ months?       │
│     □ YES   □ NO                                    │
└──────────────────┬──────────────────────────────────┘
                   │
         ┌─────────┴─────────┐
         ▼                   ▼
    3+ YES?              Mostly NO?
         │                   │
         ▼                   ▼
    ✅ RECORD           ❌ SKIP
    High-value          Low-value
    memory              noise
```

## 📊 Examples by Category

### ✅ RECORD (High Value)

| Situation | Why Record | What to Capture |
|-----------|------------|-----------------|
| Made architectural decision | ✅ 5/5 criteria | Decision + rationale + trade-offs |
| Solved complex bug after hours | ✅ 4/5 criteria | Problem + non-obvious solution |
| Established team convention | ✅ 5/5 criteria | Pattern + when to use + examples |
| Discovered performance limit | ✅ 4/5 criteria | Constraint + workaround |
| Learned stakeholder preference | ✅ 4/5 criteria | Who + what + why it matters |

### ❌ SKIP (Low Value)

| Situation | Why Skip | Alternative |
|-----------|----------|-------------|
| Fixed typo in variable name | ❌ 0/5 criteria | Just fix it, move on |
| Updated dependency version | ❌ 1/5 criteria | Track in package.json/commit |
| Current PR needs review | ❌ 0/5 criteria | Temporary status |
| Standard CRUD implementation | ❌ 1/5 criteria | Code is self-documenting |
| Documented in official README | ❌ 0/5 criteria | Already captured |

## 🚦 Red Flags (Don't Record)

- **"I just did X..."** → Probably routine
- **"Currently working on..."** → Temporary status
- **"This is in the docs..."** → Redundant
- **"Obviously..."** → Too obvious
- **"For now..."** → Temporary workaround
- **"Line 42 has..."** → Too specific

## 💎 Green Lights (Do Record)

- **"After much research, we decided..."** → Decision with context
- **"This isn't obvious, but..."** → Non-obvious insight
- **"We always do it this way because..."** → Team pattern
- **"Key constraint is..."** → Important limitation
- **"Future developers should know..."** → Lasting value

## 🎓 Learning Examples

### Example 1: Routine Fix
```
Situation: Fixed null pointer by adding null check
Future Value Test:
  Reusability? ❌ NO - standard practice
  Non-obvious? ❌ NO - obvious fix
  Continuity? ❌ NO - one-time bug
  Uniqueness? ❌ NO - basic programming
  Longevity? ❌ NO - temporary bug
Score: 0/5 → ❌ SKIP
```

### Example 2: Architectural Decision
```
Situation: Chose microservices over monolith for scalability
Future Value Test:
  Reusability? ✅ YES - affects all new services
  Non-obvious? ✅ YES - required analysis
  Continuity? ✅ YES - critical context
  Uniqueness? ✅ YES - project-specific decision
  Longevity? ✅ YES - long-term impact
Score: 5/5 → ✅ RECORD
Entity: decisions/microservices-architecture.md
Observations:
  [decision] Chose microservices for independent scaling
  [rationale] Monolith couldn't handle traffic spikes
  [constraint] Requires service mesh for communication
  [tip] New features should be separate services
```

### Example 3: Edge Case
```
Situation: Discovered API rate limit is 100/min not 1000/min
Future Value Test:
  Reusability? ✅ YES - all API integrations affected
  Non-obvious? ✅ YES - documentation was wrong
  Continuity? ✅ YES - recurring consideration
  Uniqueness? ✅ YES - specific to this API
  Longevity? ⚠️ MAYBE - might change
Score: 4/5 → ✅ RECORD
Entity: concepts/third-party-api-integration.md
Observation:
  [constraint] API rate limit is 100 requests/min not 1000 #rate-limiting
  [tip] Implement exponential backoff for retry logic
```

## 🤔 When in Doubt

**Default to NOT recording.** You can always add it later if it proves valuable. It's harder to clean up noise than to add missing information.

Ask yourself:
- "Will future-me thank me for recording this?"
- "Would this help a teammate in 6 months?"
- "Is this something I'd forget without writing it down?"

If all three are YES → Record it
If any are NO → Skip it

## 📈 Over Time

As the memory system matures:

**Month 1**: Record more liberally to establish baseline
**Month 2-3**: Refine based on what actually gets used
**Month 3+**: Very selective, only clear high-value items

**Review quarterly**: Archive outdated, consolidate related, prune noise

---

**Remember**: A lean, high-quality memory is far more valuable than a cluttered, comprehensive one.
