---
name: rumble-code-review
description: Use this skill for production-minded code reviews of the current Rumble Tumble codebase, including bug investigation reviews, broad implementation health checks, Unity-specific risk checks, improvement proposals, and optional Markdown review reports stored in the repository after user confirmation.
---

# Rumble Code Review

Use this skill when the user wants to inspect the current program state, investigate a problem, or run a broader code review after development has progressed. This skill is not PR-centered; review the local repository state and any specific files, systems, symptoms, or goals the user names.

## Review Stance

- Treat the work as serious production work. Be direct, practical, and respectful.
- Lead with the most important risks, then meaningful improvement opportunities.
- For each substantive point, explain:
  - what is concerning
  - why it can matter in this project
  - what change is likely to improve it
- Avoid lecture tone, generic praise, and style-only nitpicks unless they affect maintainability or team velocity.
- If a finding depends on an assumption, say so clearly.

## Review Modes

Choose the mode from the user's request. If the mode is unclear, infer it from context unless one short clarification would materially improve the review.

- **Bug investigation review**: Start from the symptom. Trace likely execution paths, state transitions, Unity lifecycle order, serialized references, scene/prefab dependencies, and recent code that could explain the issue.
- **Code health review**: Inspect the current implementation for risks that could slow production or cause future bugs. Include architecture, state ownership, Unity integration, performance, maintainability, and tests or verification gaps.
- **Focused review**: When the user names a file, feature, or subsystem, keep the review scoped there and only expand when dependencies are necessary to judge the risk.

## Process

1. Discover scope from the user's request and the repository layout.
2. Inspect relevant code and configuration before making claims.
3. For Unity work, check both code and likely asset/configuration dependencies when relevant.
4. Prioritize findings by production impact, not by how easy they are to fix.
5. For broad reviews, group results by category so the team can act on them.
6. Finish with a concise verdict and the next few highest-value actions.
7. Ask whether to save the review as a repository Markdown report. Do not create the report unless the user confirms.

## Categories For Broad Reviews

Use categories only when they make the review easier to scan. Merge or omit categories that do not apply.

- `Critical Behavior Risks`
- `Architecture And State Ownership`
- `Unity Lifecycle And Scene Integration`
- `Performance And Frame Stability`
- `Maintainability And Team Velocity`
- `Testing And Verification Gaps`
- `Follow-Up Investigation`

## Severity

- `Critical`: Can block play, corrupt state, lose data, or break core gameplay frequently.
- `High`: Likely user-facing bug, fragile production dependency, or hard-to-debug state issue.
- `Medium`: Meaningful maintainability, performance, or reliability risk.
- `Low`: Helpful cleanup or clarity improvement with limited immediate risk.

## Output Format

For focused reviews:

```md
## Findings

### [High] Short title
- Location: `path/to/file.cs:line`
- Problem: What is wrong or fragile.
- Why it matters: Why this can hurt gameplay, production speed, debugging, or stability.
- Suggestion: The most practical fix or next step.

## Improvement Ideas
- Targeted improvements that are useful but not the main risk.

## Open Questions
- Assumptions or missing context that could change the recommendation.

## Verdict
Short overall read of the current state and the next highest-value action.
```

For broad reviews:

```md
## Review Summary
- Scope: What was reviewed.
- Mode: Bug investigation / code health / focused review.
- Overall verdict: Short production-minded assessment.

## Critical Behavior Risks

### [High] Short title
- Location: `path/to/file.cs:line`
- Problem:
- Why it matters:
- Suggestion:

## Architecture And State Ownership
...

## Recommended Next Actions
1. Highest-value action.
2. Next action.
3. Next action.

## Open Questions
- Anything that needs playtesting, scene inspection, or team confirmation.
```

If no concrete issues are found, say that clearly and list the remaining review limits, such as untested runtime paths, scene/prefab assumptions, or missing playtest coverage.

## Optional Repository Report

After presenting the review, ask whether to save it as a Markdown report in the repository.

If the user confirms, save under `docs/reviews/` using this naming style:

- `YYYY-MM-DD_code-health-review.md`
- `YYYY-MM-DD_bug-investigation-short-topic.md`
- `YYYY-MM-DD_focused-review-short-topic.md`

The saved report should include:

- title
- review date
- scope
- findings or categorized findings
- improvement ideas
- open questions
- recommended next actions

Do not auto-commit the report unless the user explicitly asks.

## References

- Read `references/general-review.md` for general review criteria and prioritization details.
- Read `references/unity-review.md` when reviewing Unity-specific code, scenes, prefabs, assets, runtime lifecycle, physics, animation, or performance behavior.
