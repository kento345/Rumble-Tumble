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

## ブロードレビューのカテゴリ

カテゴリはレビューを読みやすくする場合のみ使う。該当しないカテゴリは省略または統合する。

- `致命的な挙動リスク`
- `アーキテクチャと状態管理`
- `Unity ライフサイクルとシーン統合`
- `パフォーマンスとフレーム安定性`
- `保守性とチーム開発速度`
- `検証とテストのギャップ`
- `追加調査が必要な事項`

## 深刻度

- `致命的`: プレイを妨げる、状態を破壊する、コアゲームプレイを頻繁に壊す可能性がある。
- `高`: ユーザーに見えるバグ、本番への脆弱な依存、デバッグ困難な状態問題が起きやすい。
- `中`: 保守性・パフォーマンス・信頼性に対する意味のあるリスク。
- `低`: 限られたリスクで役立つ整理や明確化の改善。

## 出力フォーマット

出力はすべて日本語で書く。

フォーカスレビューの場合:

```md
## 指摘事項

### [高] 短いタイトル
- 場所: `path/to/file.cs:行番号`
- 問題: 何が問題か、何が壊れやすいか。
- なぜ重要か: ゲームプレイ・開発速度・デバッグ・安定性にどう影響するか。
- 改善案: 最も実践的な修正方法。コードの書き方や変更手順を具体的に示す。

## 改善アイデア
- 緊急ではないが有益な改善点。改善の方向性と具体的なやり方を示す。

## 未確認事項
- 前提や不足しているコンテキストで、推奨内容を変える可能性があるもの。

## 総評
現在の状態の簡潔な評価と、次に最も価値ある一手。
```

ブロードレビューの場合:

```md
## レビュー概要
- 対象: 何をレビューしたか。
- モード: バグ調査 / コードヘルス / フォーカスレビュー。
- 総評: 簡潔なプロダクション視点の評価。

## 致命的な挙動リスク

### [高] 短いタイトル
- 場所: `path/to/file.cs:行番号`
- 問題:
- なぜ重要か:
- 改善案: 修正方法を具体的なコードや手順とともに示す。

## アーキテクチャと状態管理
...

## 推奨アクション
1. 最も価値の高いアクション。具体的な対処方法も記載する。
2. 次のアクション。
3. 次のアクション。

## 未確認事項
- プレイテスト・シーン確認・チームへの確認が必要なもの。
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
