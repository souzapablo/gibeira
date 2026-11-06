# STATE

## Decisions

### AD-001
- **Decision**: Gibeira is a modular monolith: one Blazor host; each module has Domain, Application and Infrastructure projects named `Gibeira.<Module>.<Layer>`; no module references another module.
- **Reason**: User chose "Modular monolith + layers"; finance areas grow independently inside one local app.
- **Trade-off**: More projects per module; cross-module needs (a `Contracts` project) must be added deliberately.
- **Scope**: all of `src/`; enforced by `tests/Gibeira.ArchitectureTests`
- **Date**: 2026-10-29
- **Status**: active

### AD-002
- **Decision**: Every architecture rule is a function over a name prefix and is proven by a fixture assembly that violates it.
- **Reason**: With no modules yet, rules over the real solution pass vacuously.
- **Trade-off**: Fixture projects to maintain alongside each new rule.
- **Scope**: `tests/Gibeira.ArchitectureTests`, `tests/Gibeira.ArchitectureTests.Fixtures`
- **Date**: 2026-10-29
- **Status**: active

### AD-003 (active)
- **Decision**: The web UI takes its look (palette, fonts, surfaces, radii, buttons) from the "Gibeira — saldos" design canvas. Kind colors: entrada #E8B04B, saída #DF6A51, diário #D65DB1, economia #37A675, cartão #8C7CF0. The saldo cell shows positive vs negative as a muted tint of #37A675 / #DF6A51 instead of a solid green/red fill.
- **Reason**: User asked for the home page in the canvas style. Asked "Which colors should the home page use?", the user chose "Canvas palette, tinted saldo (Recommended)".
- **Trade-off**: Economia and positive saldo share a hue.
- **Scope**: `src/Host/Gibeira.Web` styles and components; feature 002-home-page
- **Date**: 2026-10-29
- **Status**: active

### AD-004 (active)
- **Decision**: The home page adds the canvas top bar (gibeira logo and an "adicionar" button) and the canvas highlight of today's row to the ledger layout. The canvas left panel and view tabs are not added.
- **Reason**: Asked "What should the home page include?", the user chose the ledger layout plus the canvas top bar (Recommended).
- **Trade-off**: Adds chrome above the period bar.
- **Scope**: feature 002-home-page
- **Date**: 2026-10-29
- **Status**: active

### AD-005 (active)
- **Decision**: The home page uses the labels written in spec 002 (dia, entradas, saídas, diários, economias, cartão, saldos, adicionar, repetir, vencimento, ...), not the canvas vocabulary (cofrinho, bolsinho, colocar na gibeira).
- **Reason**: Asked which wording the home page should use, the user chose the spec's labels (Recommended).
- **Trade-off**: The page reads plainer than the canvas.
- **Scope**: feature 002-home-page
- **Date**: 2026-10-29
- **Status**: active

### AD-006 (active)
- **Decision**: Web UI values live only in `src/Host/Gibeira.Web/wwwroot/css/tokens.css`; shared components live in `Components/UI/` and take parameters only; UI state and formats are plain C# helpers outside the components; components are tested with bUnit in `tests/Gibeira.Web.Tests` and layout with Playwright in `tests/Gibeira.IntegrationTests`.
- **Reason**: Spec 002 asks for tokens and components the next pages reuse.
- **Trade-off**: A token test fails on any color literal outside `tokens.css`, including in template files.
- **Scope**: `src/Host/Gibeira.Web`; every later UI feature
- **Date**: 2026-10-30
- **Status**: active

### AD-007 (active)
- **Decision**: Every piece of feature 002 that is not frontend (mock data, sums, saldos, repetitions, card due dates) is a temporary stand-in that lives in `src/Host/Gibeira.Web/Mockup/` behind `ILedgerStore`, with its tests in `tests/Gibeira.Web.Tests/Mockup/`. It is deleted when a finance module implements `ILedgerStore`; it is not a domain model and sets no pattern.
- **Reason**: User: "any logic other than the frontend is temporary to display the frontend mockup". Asked where the ledger rules should live, the user chose "Plain C# in Gibeira.Web (Recommended)".
- **Trade-off**: The mockup's rules are written and tested only to the depth the spec's criteria need, and are thrown away later.
- **Scope**: feature 002-home-page; the first finance module
- **Date**: 2026-10-30
- **Status**: active

### AD-008 (active)
- **Decision**: A LedgerEntry is one complete record of one day's money movement (day, type, amount, description, card, tags, origin). A RecurringRule aggregate creates the LedgerEntries of an endless repetition on demand, up to the last date read, and its generated range only moves forward. Parcelado creates its N LedgerEntries at once, with no rule.
- **Reason**: User: "I want the transaction to have all information. The day, type, amount etc". Asked how a salary repeating monthly with no end date should work, the user chose "One per day, on demand (Recommended)".
- **Trade-off**: Reading can create records; a repetition has two aggregates to keep in step until persistence settles how they save together.
- **Scope**: `src/Modules/Ledger`; feature 003-ledger-entry
- **Date**: 2026-11-05
- **Status**: active

### AD-009 (active)
- **Decision**: Every cartão LedgerEntry names a card and every other type has none. `sem cartão` leaves the add modal, replacing ADD-13 and ADD-17 of spec 002.
- **Reason**: Asked "Can a Transaction of type cartão have no card?", the user chose "Cartão requires a card" over the recommended "Card is optional".
- **Trade-off**: The add button stays disabled on cartão until a card is chosen.
- **Scope**: `src/Modules/Ledger`, `src/Host/Gibeira.Web/Components/Home/AddItemModal.razor`; feature 003-ledger-entry
- **Date**: 2026-11-05
- **Status**: active

### AD-010 (active)
- **Decision**: The entity is named `LedgerEntry` in code, in the `Gibeira.Ledger` module. Screens keep the word `lançamento`.
- **Reason**: `LedgerEntry` is the closest match for `lançamento`, and it also fits records that are only projected. A bare `Entry` reads like the screen's `entrada`, and the user chose: "Yeah, we can go with LedgerEntry".
- **Trade-off**: Longer than `Entry`, and banking apps usually say "transaction".
- **Scope**: `src/Modules/Ledger`; feature 003-ledger-entry
- **Date**: 2026-11-05
- **Status**: active

### AD-011 (active)
- **Decision**: Every code identifier is in English, in new and existing code: types, enum values, members, test names, CSS classes, CSS tokens and internal keys. Portuguese appears only in text users see on screens, including sample data shown on the page. Spec 003's naming table maps each screen word to its code name.
- **Reason**: User: "do not use portuguese on the code, only for screens that users will face".
- **Trade-off**: Code and screens use different words for one concept. AD-005's labels stay on screens.
- **Scope**: all of `src/` and `tests/`; proven by a naming test
- **Date**: 2026-11-05
- **Status**: active

## Handoff

- **Feature**: home page, `.specs/features/002-home-page`
- **Phase / Task**: Feature done and verified: `check_proofs.py --mutations` green at 43e2440 (80 pass, 3 mutations killed), `validate_verification.py` exit 0
- **Completed**: T1 to T28 (T15 ran before T14: `ILedgerStore.Months(Period)` needs `Period`)
- **In-progress** (file:line): none
- **Next step**: the user decides on pushing `feat/002-home-page` and opening a PR (nothing pushed yet)
- **Blockers**: none. Verifier: 6 findings, closed e8902f2 to e5386a7; screens: 7 differences, closed 5f7c028 to 90f4a2c (new criteria HOME-19 to HOME-23), round 2 matches; lessons L-006 to L-011 recorded as candidates
- **Uncommitted files**: none
- **Branch**: feat/002-home-page
