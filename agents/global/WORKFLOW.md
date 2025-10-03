---
title: "Execution Workflow"
version: "1.0.1"
lastModified: "2025-09-30"
author: "Franz Hemmer"
purpose: "Step-by-step operational workflow for problem solving, research, implementation, and validation."
---

# Workflow - The workflow you should follow

1. **Check Memory**: Search `/agents/memories` for relevant context about the user's request (projects, decisions, concepts, people). Use this context to inform your work.
2. Fetch any URL's provided by the user using available web fetch capabilities.
3. Understand the problem deeply. Carefully read the issue and think critically about what is required. Use sequential thinking to break down the problem into manageable parts. Consider the following:
   - What is the expected behavior?
   - What are the edge cases?
   - What are the potential pitfalls?
   - How does this fit into the larger context of the codebase?
   - What are the dependencies and interactions with other parts of the code?
3. Investigate the codebase. Explore relevant files, search for key functions, and gather context.
4. Research the problem on the internet by reading relevant articles, documentation, and forums.
5. Develop a clear, step-by-step plan. Break down the fix into manageable, incremental steps. Maintain a todo list (prefer your host/tooling's native planning mechanism when available) and surface a simple Markdown checkbox checklist to the user at key milestones or when requested.
6. Implement the fix incrementally. Make small, testable code changes.
7. Debug as needed. Use debugging techniques to isolate and resolve issues.
8. Test frequently. Run tests after each change to verify correctness.
9. Iterate until the root cause is fixed and all tests pass.
10. Reflect and validate comprehensively. After tests pass, think about the original intent, write additional tests to ensure correctness, and remember there are hidden tests that must also pass before the solution is truly complete.
11. **Update Memory**: Record important decisions, patterns, or insights in `/agents/memories` so future conversations can build on this work.

Refer to the detailed sections below for more information on each step.

## 0. Check Memory (Always Start Here)

Before beginning work, search the memory system for relevant context:

**When to check memory:**
- At the start of every conversation
- When encountering unfamiliar projects, concepts, or patterns
- Before making architectural decisions
- When debugging recurring issues

**How to search memory:**
- Semantic search: "Find information about [topic]"
- Text search: Look for specific terms or names
- Entity lookup: Check for existing project/person/decision entities
- Relation traversal: Follow connections between related entities

**What to look for:**
- Existing project context and status
- Previous architectural decisions and rationale
- Known issues and their solutions
- Team preferences and constraints
- Related concepts and patterns

**Example queries:**
```
"What do we know about the authentication system?"
"Find decisions related to API design"
"Show me information about relias-assistant project"
"What patterns have we used for error handling?"
```

**See:** `agents/global/MEMORY-INSTRUCTIONS.md` for complete memory usage guide.

## 1. Fetch Provided URLs
- If the user provides a URL, use available web fetch capabilities (subject to host/system policy) to retrieve the content.
- After fetching, review the content returned.
- If you find any additional URLs or links that are relevant, fetch those as well.
- Recursively gather all relevant information by fetching additional links until you have all the information you need.

## 2. Deeply Understand the Problem
Carefully read the issue and think hard about a plan to solve it before coding.

## 3. Codebase Investigation
- Explore relevant files and directories.
- Search for key functions, classes, or variables related to the issue.
- Read and understand relevant code snippets.
- Identify the root cause of the problem.
- Validate and update your understanding continuously as you gather more context.

## 4. Internet Research
- Use available web search or fetch capabilities (subject to host/system policy) to search for relevant information.
- After searching, review the content returned.
- You MUST fetch the contents of the most relevant links to gather information. Do not rely on the summary that you find in the search results.
- As you fetch each link, read the content thoroughly and fetch any additional links that you find within the content that are relevant to the problem.
- Recursively gather all relevant information by fetching links until you have all the information you need.

## 5. Develop a Detailed Plan
- Outline a specific, simple, and verifiable sequence of steps to fix the problem.
- Maintain your plan using the host/system's preferred task tracking (or a Markdown checklist if none exists).
- Check off steps as you complete them; show the updated checklist to the user at meaningful milestones or when they ask. If the user requests conciseness or the task is trivial, summarize progress instead of reposting the full checklist.

## 6. Making Code Changes
- Before editing, always read the relevant file contents or section to ensure complete context.
- Always read 2000 lines of code at a time to ensure you have enough context.
- If a patch is not applied correctly, attempt to reapply it.
- Make small, testable, incremental changes that logically follow from your investigation and plan.
- Whenever you detect that a project requires an environment variable (such as an API key or secret), always check if a .env file exists in the project root. If it does not exist, automatically create a .env file with a placeholder for the required variable(s) and inform the user. Do this proactively, without waiting for the user to request it.

## 7. Debugging
- Check for any problems in the code using available diagnostic tools
- Make code changes only if you have high confidence they can solve the problem
- When debugging, try to determine the root cause rather than addressing symptoms
- Debug for as long as needed to identify the root cause and identify a fix
- Use print statements, logs, or temporary code to inspect program state, including descriptive statements or error messages to understand what's happening
- To test hypotheses, you can also add test statements or functions
- Revisit your assumptions if unexpected behavior occurs
- See `agents/global/DEBUGGING.md` for comprehensive debugging guidance

## 11. Update Memory (Selectively)

After completing work, evaluate what's worth remembering. **Quality over quantity** - only record knowledge that will be valuable in future conversations.

### Memory Selection Criteria

**✅ HIGH VALUE - Record These:**

1. **Architectural Decisions**
   - Choices that affect multiple components
   - Trade-offs between competing approaches
   - Decisions with long-term implications
   - *Example*: "Why we chose microservices over monolith"

2. **Non-Obvious Solutions**
   - Complex problems with non-trivial solutions
   - Solutions that required significant research/debugging
   - Approaches that aren't documented elsewhere
   - *Example*: "How we fixed the race condition in webhook processing"

3. **Reusable Patterns**
   - Successful approaches to apply elsewhere
   - Team conventions and standards
   - Code patterns that should be consistent
   - *Example*: "Error handling pattern for async operations"

4. **Project Context**
   - Information needed across multiple conversations
   - Key stakeholders and their preferences
   - Cross-cutting concerns and constraints
   - *Example*: "Authentication requirements for all APIs"

5. **Learned Constraints**
   - Technical limitations discovered
   - Performance characteristics
   - Integration gotchas
   - *Example*: "Third-party API rate limits and retry strategy"

**❌ LOW VALUE - Don't Record:**

1. **Routine Operations**
   - Standard CRUD implementations
   - Trivial bug fixes (typos, simple logic errors)
   - Boilerplate code generation
   - *Why*: Obvious, doesn't need documentation

2. **Temporary Information**
   - Current PR status or task assignments
   - Transient environment issues
   - One-time configuration changes
   - *Why*: Will be stale quickly

3. **Easily Discoverable**
   - Information clearly in README or docs
   - Standard framework usage
   - Well-known best practices
   - *Why*: Redundant with existing documentation

4. **Over-Specific Details**
   - Exact line numbers or file locations
   - Implementation minutiae without broader lesson
   - Dependency version numbers
   - *Why*: Too granular, changes frequently

### The "Future Value Test"

Before creating a memory, ask:

1. **Reusability**: Will this help in future similar situations?
2. **Non-Obviousness**: Is this something I'd need to research again?
3. **Continuity**: Does this provide context for future conversations?
4. **Uniqueness**: Is this already documented elsewhere?
5. **Longevity**: Will this still be relevant in 3+ months?

**If YES to 3+ questions → Record it**
**If NO to most → Skip it**

### What to Record

**How to update memory:**
1. **Evaluate**: Apply the "Future Value Test" (above)
2. **Search first**: Check if entity already exists
3. **Update or create**: Add to existing or create new entity
4. **Be selective**: Only add high-value observations
5. **Establish relations**: Link to related entities
6. **Tag thoughtfully**: Use tags for discoverability

**Memory quality guidelines:**
- ✅ **Actionable**: Include enough context to use the knowledge
- ✅ **Rationale**: Explain *why*, not just *what*
- ✅ **Atomic**: One clear fact per observation
- ✅ **Evergreen**: Focus on lasting insights, not transient details
- ✅ **Linked**: Connect related concepts
- ❌ **Avoid clutter**: Don't record obvious or temporary information
- ❌ **Avoid redundancy**: Don't duplicate what's in docs/code comments

**Example memory updates:**
```markdown
"Update memory-system-implementation entity with observation:
[fact] Successfully integrated memory checks into WORKFLOW.md #integration"

"Create new decision entity for authentication-approach with:
[decision] Using JWT tokens for API authentication #security
[rationale] Stateless authentication supports microservices architecture"
```

**See:** `agents/global/MEMORY-INSTRUCTIONS.md` for complete writing guidelines.

## Task Tracking and Todo Lists

Use the host/system's preferred task tracking mechanism when available (e.g., TodoWrite tool). Fall back to Markdown checklists when no native task tracking is available or when the user explicitly requests a simple checklist.

### Using Native Task Tracking (Preferred)
If your environment provides a task tracking tool (like TodoWrite), use it to:
- Break down complex tasks into manageable steps
- Track progress with status updates (pending, in_progress, completed)
- Provide visibility into your work process
- Help organize multi-step implementations

### Using Markdown Checklists (Fallback)
When native task tracking is unavailable or inappropriate, use this markdown format:
```markdown
- [ ] Step 1: Description of the first step
- [ ] Step 2: Description of the second step
- [ ] Step 3: Description of the third step
```

**Guidelines:**
- Do not use HTML tags or other formatting for checklists
- Always wrap checklists in triple backticks for proper formatting
- Update checklists at meaningful milestones, not after every single step
- When policies conflict, host/system policies take precedence
- Prefer brevity when requested by the user
- Use discretion: don't always append the full checklist if the task is simple or the user prefers conciseness
