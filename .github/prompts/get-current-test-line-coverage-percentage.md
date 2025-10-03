---
mode: agent
name: test-coverage-reader
argument: []
---

## Goal
Read the project's dotCover instructions and output only the current test line-coverage percentage as a number (no symbols, no extra words).

## Instructions
1. Locate the dotCover guide in the repository:
   - Primary path: `/agents/DOTCOVER.md`
   - Fallback path: `/agents/global/DOTCOVER.md`
2. Follow the documented steps to run dotCover and generate the latest coverage report for the solution.
3. Extract the "line coverage" percentage value from the resulting report.
4. Output only the numeric percentage value, without any additional text, labels, or symbols.
   - Example output format: `73.4`

## Notes
- Use existing solution and scripts referenced in the DOTCOVER guide.
- Do not modify code or tests; this prompt is read/execute/report only.
- If multiple reports are generated, use the most recent one according to the guide.
- If the guide specifies a preferred report artifact (e.g., XML/HTML), parse that source for the line coverage metric.
- If the guide provides both line and branch coverage, return the line coverage value only.
