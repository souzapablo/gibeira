# LESSONS - auto-maintained by scripts/lessons.py

> Machine-owned. Do NOT hand-edit. Changes are overwritten on the next `lessons.py` write.
> Canonical state lives in `.specs/lessons.json`. Edit lessons only via the script.
> promote_threshold=2 distinct features · window_days=45 · quarantine_threshold=2

## Confirmed (load these at Plan/Checks)

Corroborated across multiple features. Safe to apply as guidance.

_none_

## Candidates (under observation - do NOT load as guidance yet)

Seen once or not yet corroborated. Tracked, not trusted.

### L-001 - When a rule accepts a closed list of values, test every value in the list, not a sample
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `hooks` · harmful: 0
- features: 001-monorepo-scaffold
- evidence: C21 tests/Gibeira.RepositoryTests/CommitMessageHookTests.cs (hooks)
- last seen: 2026-10-29T20:09:08Z

### L-002 - When a rule names several subjects, give each subject its own violating fixture and assertion
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `architecture-tests` · harmful: 0
- features: 001-monorepo-scaffold
- evidence: C11 tests/Gibeira.ArchitectureTests/LayerRuleTests.cs (architecture-tests)
- last seen: 2026-10-29T20:09:08Z

### L-003 - Every forbidden target a rule lists needs a fixture assembly that depends on it
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `architecture-tests` · harmful: 0
- features: 001-monorepo-scaffold
- evidence: C13 tests/Gibeira.ArchitectureTests/BoundaryRuleTests.cs (architecture-tests)
- last seen: 2026-10-29T20:09:08Z

### L-004 - A name parser that returns null for unexpected input must be paired with a rule that reports those names, or every rule built on it skips them silently
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `architecture-tests` · harmful: 0
- features: 001-monorepo-scaffold
- evidence: C36 tests/Gibeira.ArchitectureTests/NamingRules.cs (architecture-tests)
- last seen: 2026-10-29T20:09:08Z

### L-005 - A pre-commit check must read the staged blob, not the working copy, and be tested with a file whose staged and working versions differ
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `hooks` · harmful: 0
- features: 001-monorepo-scaffold
- evidence: C37 .husky/format-staged.sh (hooks)
- last seen: 2026-10-29T20:09:08Z

### L-006 - Prove a rule that depends on a date window through the component that picks the window, not only through the helper it calls
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `web-ledger` · harmful: 0
- features: 002-home-page
- evidence: Mockup/InMemoryLedgerStore.cs:31 (C19) (web-ledger)
- last seen: 2026-10-30T21:16:06Z

### L-007 - Test a cents rule on amounts where cutting and rounding give different results
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `money` · harmful: 0
- features: 002-home-page
- evidence: Mockup/Schedule.cs:48 (C53) (money)
- last seen: 2026-10-30T21:16:07Z

### L-008 - Test a breakpoint at a width where the base style alone would give a different result
- signal: `surviving_mutant` · recurrence: 1 feature(s) · scope: `browser-tests` · harmful: 0
- features: 002-home-page
- evidence: UI/Drawer.razor.css:39 (C71) (browser-tests)
- last seen: 2026-10-30T21:16:07Z

### L-009 - Apply a picker choice only when its follow-up step is saved, so dismissing the step keeps the previous value
- signal: `ac_gap` · recurrence: 1 feature(s) · scope: `web-ui` · harmful: 0
- features: 002-home-page
- evidence: Home/AddItemModal.razor:149 (ADD-25) (web-ui)
- last seen: 2026-10-30T21:16:07Z

### L-010 - Prove each UI criterion through user gestures at the level the Test Coverage Matrix names, not only on the state class behind it
- signal: `spec_precision_gap` · recurrence: 1 feature(s) · scope: `web-ui` · harmful: 0
- features: 002-home-page
- evidence: C59, C63 (web-ui)
- last seen: 2026-10-30T21:16:07Z

### L-011 - Read a server-rendered change in a browser test with a waiting assertion, never a single read right after the action
- signal: `gate_fail` · recurrence: 1 feature(s) · scope: `browser-tests` · harmful: 0
- features: 002-home-page
- evidence: C68 ReloadDiscardsChanges (browser-tests)
- last seen: 2026-10-30T21:16:07Z

## Quarantined (failed when applied - ignore)

A confirmed lesson that recurred alongside failure. Kept for the maintainer to review.

_none_
