---
mode: agent
---
Goal
- Find conflicting or duplicated instructions in /AGENTS.md and every file it explicitly references. Apply precedence: host/system policies > repository docs (per /AGENTS.md). Report only conflicts (not duplicates) in the count.

Definitions
- Conflict: Two instructions that cannot both be followed in the same situation (e.g., must vs must-not, mutually exclusive priorities, incompatible constraints).
- Duplicate: Substantially identical guidance repeated with no material difference.

Method
1) Read /AGENTS.md and only the files it explicitly references. Ignore unreferenced files.
2) Identify both conflicts and duplicates. Group each unique contradiction into one conflict group.
3) Sort conflict groups deterministically by the pair of source file paths (lexicographic) to determine the “first conflict.”

Output (strict)
- Line 1: Just the integer N = number of conflict groups found.
- If N = 0, output two lines: line 1 is "0" and line 2 is a short positive confirmation (e.g., "No conflicts found; instructions are consistent.").
- If N > 0, output ONLY details of the first conflict, in exactly this order:
	- Source A: file path and (if known) section/heading — "verbatim excerpt (<=300 chars)"
	- Source B: file path and (if known) section/heading — "verbatim excerpt (<=300 chars)"
	- Nature: one concise sentence explaining the conflict
	- Fix: one concise, actionable suggestion

Rules
- Keep the entire output after line 1 under 10 lines total.
- Quote verbatim; truncate long quotes with … if needed. If section/line numbers aren’t available, use the nearest heading.
- If N = 0, include exactly one additional positive confirmation line after "0"; total output must not exceed 2 lines.
- When a conflict involves host/system policies vs repo docs, state host/system policies take precedence and suggest amending repo docs accordingly.

Note
- You must still check for duplicates to avoid misclassifying them as conflicts; however, do not include duplicates in the conflict count or output unless they are part of a true contradiction.
