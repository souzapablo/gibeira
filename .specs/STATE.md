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

## Handoff
