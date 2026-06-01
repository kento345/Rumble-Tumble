# General Review Criteria

Use this reference when judging general code quality, production risk, and improvement value.

## What To Prioritize

Review findings should prioritize production impact:

1. Bugs that can break core gameplay or player progress.
2. State ownership problems that can create inconsistent behavior.
3. Fragile dependencies that will slow future feature work.
4. Performance risks that can hurt frame stability.
5. Missing verification for behavior that is likely to regress.
6. Cleanup that improves readability or team velocity.

Avoid treating personal style preferences as findings. Only mention style when it affects correctness, maintainability, onboarding speed, or future change safety.

## Common Risk Areas

- **Behavior correctness**: boundary cases, invalid state, race/order issues, missing guards, failed assumptions.
- **State ownership**: multiple systems writing the same state, hidden global state, unclear source of truth, stale cached values.
- **Control flow**: large condition trees, duplicated transitions, special cases that bypass cleanup.
- **Data shape**: magic values, parallel arrays/lists, implicit index contracts, unchecked dictionary/list access.
- **Error handling**: null handling, missing fallback behavior, partial initialization, silent failure.
- **Performance**: repeated allocation, expensive per-frame work, repeated lookup, avoidable scene searches.
- **Maintainability**: names that hide intent, feature logic spread across unrelated classes, difficult-to-test coupling.
- **Verification**: no playtest path, no editor/test coverage for high-risk logic, missing reproduction steps.

## Finding Quality

A good finding is specific, actionable, and grounded in evidence from the code.

Include:

- `Location`: file and line when possible.
- `Problem`: the concrete risk, not just a vague smell.
- `Why it matters`: the production consequence.
- `Suggestion`: the most practical fix or investigation step.

Do not overstate certainty. Use language such as "can", "likely", or "appears" when the issue depends on runtime setup, scene wiring, or unavailable context.

## Improvement Ideas

Use improvement ideas for changes that are valuable but not urgent enough to be primary findings.

Good improvement ideas usually:

- simplify future feature work
- reduce repeated code
- clarify ownership boundaries
- make runtime behavior easier to debug
- add targeted checks around high-risk paths

Keep improvement ideas concrete. Prefer "Extract score update ownership into one service" over "Improve architecture".
