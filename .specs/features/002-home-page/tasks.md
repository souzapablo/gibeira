# Home Page Tasks

## Execution Protocol (MANDATORY -- do not skip)

Implement these tasks with the `tlc-spec-driven` skill: **activate it by name and follow its Execute flow and Critical Rules.** Do not search for skill files by filesystem path. The skill is the source of truth for the full flow (per-task cycle, sub-agent delegation, the walk, check_proofs, the Verifier and the screens sub-agent).

**If the skill cannot be activated, STOP and tell the user - do not proceed without it.**

---

**Design**: `.specs/features/002-home-page/design.md`
**Status**: In progress
**Approved**: 2026-10-30 - user: "Approve tasks.md"; execution: "Inline, task by task"
**Mutations**: 3
**Build**: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`

---

## Test Coverage Matrix

> Generated from the approved spec and design, the existing tests and `CONTRIBUTING.md` (format hook). Guidelines found: no testing guideline in `AGENTS.md`, `CONTRIBUTING.md` or `docs/`, so strong defaults apply. The mockup is the exception: per AD-007 it is tested only as deep as the criteria it feeds.

| Code Layer | Required Test Type | Coverage Expectation | Location Pattern | Run Command |
| ---------- | ------------------ | -------------------- | ---------------- | ----------- |
| Frontend helpers (`Ledger/Period`, `PtBr`, `AmountInput`, `AddItemForm`) | unit | All branches; 1:1 to the ACs they serve; every listed edge case | `tests/Gibeira.Web.Tests/Ledger/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Mockup (`Mockup/*`, temporary, AD-007) | unit | The ACs it feeds (HOME-10, ADD-10 to ADD-12, ADD-14, ADD-15, MOCK-01) and the spec's edge cases, nothing more | `tests/Gibeira.Web.Tests/Mockup/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| UI components (`Components/UI/*`) | component (bUnit) | Renders from parameters alone with no services; every parameter path and callback | `tests/Gibeira.Web.Tests/UI/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Home components and page (`Components/Home/*`, `Pages/Home.razor`) | component (bUnit) | Every HOME, DAY, ADD AC: happy path and listed edge cases, through user gestures | `tests/Gibeira.Web.Tests/Home/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Styles, tokens, fonts (`wwwroot/**`, `*.razor.css`) | unit (file scan) | UI-01, UI-02 with file and line, local font files | `tests/Gibeira.Web.Tests/Styles/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Layout and browser behaviour | e2e (Playwright on the Aspire-hosted app) | Every criterion only a browser shows: layout, sticky rows, scroll, widths, hosts, focus, input echo, reload | `tests/Gibeira.IntegrationTests/Browser/*.cs` | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj` |
| Host responses | integration (Aspire.Hosting.Testing) | The home page answers with the new page and no Bootstrap | `tests/Gibeira.IntegrationTests/WebHostTests.cs` | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj` |
| Contract records, DI registration, project files | none | build gate only | - | build gate only |

## Gate Check Commands

| Gate Level | When to Use | Command |
| ---------- | ----------- | ------- |
| Quick | Tasks with unit or bUnit tests only | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Full | Tasks with integration or Playwright tests | `dotnet test --solution Gibeira.slnx` |
| Build | Config-only tasks and phase ends | `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx` |

---

## Execution Plan

Phases run in order; tasks within a phase run in order.

### Phase 1: Test bed, tokens and template cleanup

```
T1 → T2 → T3 → T4
```

### Phase 2: UI primitives

```
T5 → T6 → T7 → T8
```

### Phase 3: UI containers

```
T9 → T10 → T11 → T12 → T13
```

### Phase 4: Frontend contract and helpers

```
T14 → T15 → T16 → T17
```

### Phase 5: Mockup (temporary, AD-007)

```
T18 → T19 → T20 → T21
```

### Phase 6: Home screen

```
T22 → T23 → T24 → T25 → T26 → T27 → T28
```

---

## Task Breakdown

### T1: Web test project and tokens file

**What**: Create `tests/Gibeira.Web.Tests` (xUnit v3 + bUnit 2.13.3, referencing `Gibeira.Web`, in `Gibeira.slnx` under `/tests/`, versions in `Directory.Packages.props`) and `tokens.css` with every value of the spec's token table as a custom property, each value once.
**Where**: `src/Host/Gibeira.Web/wwwroot/css/tokens.css`
**Depends on**: None
**Reuses**: `tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj` project shape, `xunit.v3.mtp-v2`
**Requirement**: UI-01

**Done when**:

- [x] `TokensDefineEachCanvasValueOnce` passes: it reads the spec's values from a list in the test and finds each exactly once as a custom property value
- [x] `ArchitectureTests` still pass (the new assembly ends in `Tests` and is skipped)
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`
- [x] Test count: Web.Tests 1 test

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add design tokens and web test project`

---

### T2: Self-hosted fonts

**What**: Add the Fontsource variable `woff2` files of Bricolage Grotesque and Instrument Sans with `OFL.txt` under `wwwroot/fonts/`, and their `@font-face` rules in `tokens.css`.
**Where**: `src/Host/Gibeira.Web/wwwroot/fonts/`
**Depends on**: T1
**Reuses**: Fontsource `@fontsource-variable/bricolage-grotesque` and `@fontsource-variable/instrument-sans` (OFL), fetched once and committed
**Requirement**: UI-04

**Done when**:

- [x] `FontFacesPointToLocalFiles` passes: every `@font-face` `src` is a relative URL to a file that exists, and no CSS file under `wwwroot` (outside `lib/`) has `@import` or a `url(` to another host
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`
- [x] Test count: Web.Tests 2 tests

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): serve Bricolage Grotesque and Instrument Sans locally`

---

### T3: Drop Bootstrap and the template chrome

**What**: Delete `wwwroot/lib/bootstrap`, `NavMenu` and the sidebar. Make `MainLayout` a bare shell with the error UI, rewrite `app.css` and `ReconnectModal.razor.css` onto tokens, link `tokens.css` and set `<html lang="pt-BR">` in `App.razor`. Add the color-literal scan.
**Where**: `src/Host/Gibeira.Web/wwwroot/app.css`
**Depends on**: T2
**Reuses**: The existing `#blazor-error-ui` markup
**Requirement**: UI-02, UI-05

**Done when**:

- [x] `NoColorLiteralOutsideTokens` passes on `Gibeira.Web`
- [x] `ColorLiteralIsReportedWithFileAndLine` passes: the scanner run over a temp folder holding `#fff`, `rgb(`, `rgba(` and `hsl(` in a `.razor` and a `.css` file reports each as `file:line`, and ignores `tokens.css` and `lib/`
- [x] `WebHomePageLoadsNoBootstrap` passes: `/` and every stylesheet it links contain no `bootstrap`
- [x] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [x] Test count: Web.Tests 4 tests; IntegrationTests 4 tests

**Tests**: integration
**Gate**: full

**Commit**: `refactor(web): replace Bootstrap and template chrome with tokens`

---

### T4: Playwright browser fixture

**What**: Add `Microsoft.Playwright` 1.64.1 to `Gibeira.IntegrationTests` with a Chromium fixture over the Aspire-hosted `web` endpoint. The fixture fails with the install command when the browser is missing, and `CONTRIBUTING.md` lists that command. Includes the first browser test.
**Where**: `tests/Gibeira.IntegrationTests/Browser/BrowserFixture.cs`
**Depends on**: T3
**Reuses**: `tests/Gibeira.IntegrationTests/WebHostFixture.cs`
**Requirement**: UI-04

**Done when**:

- [x] `HomePageLoadsNothingFromAnotherHost` passes: every request while `/` loads goes to a loopback host, and `document.fonts` reports both families loaded
- [x] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [x] Test count: IntegrationTests 5 tests

**Tests**: e2e
**Gate**: full

**Commit**: `test(web): add Playwright browser fixture`

---

### T5: Icon and KindIcon

**What**: `Ledger/Kind` enum, the inline-SVG `Icon` component, and `KindIcon` (arrow in, arrow out, D, E, C on the kind color, or the zero color when `Muted`).
**Where**: `src/Host/Gibeira.Web/Components/UI/KindIcon.razor`
**Depends on**: None (runs after Phase 1)
**Reuses**: Tokens from T1
**Requirement**: UI-03

**Done when**:

- [x] `KindIconShowsGlyphAndKindColor` and `IconIsHiddenFromScreenReadersWithoutLabel` pass
- [x] `EveryUiComponentRendersFromParametersAlone` exists as a theory with rows for `Icon` and `KindIcon`, rendered in a bUnit context with no services
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add Icon and KindIcon components`

---

### T6: pt-BR formats

**What**: `Ledger/PtBr`: money, month abbreviation, month title, day title, short, row and long dates, and kind names, from fixed arrays and an explicit `NumberFormatInfo`.
**Where**: `src/Host/Gibeira.Web/Ledger/PtBr.cs`
**Depends on**: T5
**Reuses**: None
**Requirement**: HOME-08, HOME-11, DAY-03

**Done when**:

- [x] `MoneyFormatsWithGroupsAndComma` (`R$ 1.189,27`, `R$ 0,00`, `R$ 15.500,00`) passes
- [x] `NegativeAmountFormatsWithMinusAfterSymbol` (`R$ -477,67`) passes
- [x] `DayTitleIsDayAndMonthAbbrev` (`10/nov`), `MonthTitleIsLowercaseFullName` (`dezembro de 2026`) and the date-format tests pass for every month
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add pt-BR formats`

---

### T7: Money

**What**: The `Money` component over `PtBr.Money`, in the zero color when `Muted`.
**Where**: `src/Host/Gibeira.Web/Components/UI/Money.razor`
**Depends on**: T6
**Reuses**: `PtBr`
**Requirement**: UI-03, HOME-09

**Done when**:

- [x] `MoneyRendersFormattedValue` and `MutedMoneyUsesZeroColor` pass; the `Money` row is added to `EveryUiComponentRendersFromParametersAlone`
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add Money component`

---

### T8: Button and IconButton

**What**: `Button` (Primary, Secondary, Light, Kind variants, pill, at least 44 px) and `IconButton` (required pt-BR `Label` as `aria-label`).
**Where**: `src/Host/Gibeira.Web/Components/UI/Button.razor`
**Depends on**: T7
**Reuses**: `Icon`
**Requirement**: UI-03, UI-06

**Done when**:

- [x] `ButtonVariantsSetTheirClass`, `DisabledButtonDoesNotFireClick` and `IconButtonHasAriaLabel` pass; rows added to the theory
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add Button and IconButton components`

---

### T9: Dropdown

**What**: `Dropdown<T>` with a header and optional action, options with optional description and icon, a check on the selected, single or multiple choice, `role="dialog"`, and close on Esc or an outside click.
**Where**: `src/Host/Gibeira.Web/Components/UI/Dropdown.razor`
**Depends on**: None (runs after Phase 2)
**Reuses**: `Icon`, `IconButton`
**Requirement**: UI-03, UI-06, ADD-25

**Done when**:

- [x] `DropdownChecksSelectedOption`, `MultipleDropdownTogglesWithoutClosing`, `DropdownClosesOnEscAndOutsideClickWithoutChoosing` and `DropdownHasDialogRole` pass; row added to the theory
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add Dropdown component`

---

### T10: StepperPopover

**What**: `StepperPopover<T>`: header, `<` formatted value `>`, `salvar`, `role="dialog"`, close on Esc or an outside click.
**Where**: `src/Host/Gibeira.Web/Components/UI/StepperPopover.razor`
**Depends on**: T9
**Reuses**: `IconButton`, `Button` (Light)
**Requirement**: UI-03, UI-06, ADD-25

**Done when**:

- [x] `StepperRaisesStepAndSave` and `StepperClosesOnEscWithoutSaving` pass; row added to the theory
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add StepperPopover component`

---

### T11: Modal

**What**: `Modal`: title, X, content, backdrop, `role="dialog"` with `aria-modal`, focus on open, close on X, Esc and a backdrop click; full width minus 16 px gutters at 432 px or narrower.
**Where**: `src/Host/Gibeira.Web/Components/UI/Modal.razor`
**Depends on**: T10
**Reuses**: `IconButton`
**Requirement**: UI-03, UI-06, ADD-24

**Done when**:

- [x] `ModalClosesOnXEscAndBackdrop` and `ModalIsAnAriaDialog` pass; row added to the theory
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add Modal component`

---

### T12: Drawer

**What**: `Drawer`: right panel with a header slot and content, `role="dialog"`, focus on open, close on Esc; full width at 432 px or narrower.
**Where**: `src/Host/Gibeira.Web/Components/UI/Drawer.razor`
**Depends on**: T11
**Reuses**: `IconButton`
**Requirement**: UI-03, UI-06, DAY-09

**Done when**:

- [x] `DrawerClosesOnEsc` and `DrawerIsADialog` pass; row added to the theory
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add Drawer component`

---

### T13: FieldRow and TopBar

**What**: `FieldRow` (icon, label, value, chevron, click) and `TopBar` (the `gibeira` logo and the "adicionar" button).
**Where**: `src/Host/Gibeira.Web/Components/UI/TopBar.razor`
**Depends on**: T12
**Reuses**: `Icon`, `Button`
**Requirement**: UI-03

**Done when**:

- [x] `FieldRowShowsLabelValueAndChevron` and `TopBarAddRaisesOnAdd` pass
- [x] `EveryUiComponentRendersFromParametersAlone` has all 11 rows of the spec's component table and passes
- [x] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add FieldRow and TopBar components`

---

### T14: Frontend contract

**What**: `Ledger/ILedgerStore` and the records `Repetition`, `Card`, `NewItem`, `DayRow`, `MonthLedger` and `DayEntry`, as in the design's Data Models.
**Where**: `src/Host/Gibeira.Web/Ledger/ILedgerStore.cs`
**Depends on**: None (runs after Phase 3)
**Reuses**: `Kind`
**Requirement**: MOCK-03

**Done when**:

- [ ] Types compile with the members of the design
- [ ] Gate check passes: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`

**Tests**: none
**Gate**: build

**Commit**: `feat(web): add the ledger store contract`

---

### T15: Period

**What**: `Ledger/Period`: start at a month, shift by months, the label, the 25 choices and the 12 months.
**Where**: `src/Host/Gibeira.Web/Ledger/Period.cs`
**Depends on**: T14
**Reuses**: `PtBr.MonthAbbrev`
**Requirement**: HOME-02, HOME-03, HOME-04, HOME-05

**Done when**:

- [ ] `PeriodLabelUsesPtBrAbbrevs` (`out/2026 - set/2027`), `ShiftMovesByMonthsAcrossYears` and `ChoicesSpanTwelveMonthsEachWay` (25, first `out/2025 - set/2026`, last `out/2027 - set/2028`) pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the twelve-month period`

---

### T16: AmountInput

**What**: `Ledger/AmountInput.Accept` and `Parse` for ADD-02.
**Where**: `src/Host/Gibeira.Web/Ledger/AmountInput.cs`
**Depends on**: T15
**Reuses**: None
**Requirement**: ADD-02

**Done when**:

- [ ] `AmountAcceptsDigitsAndOneCommaWithTwoDecimals` passes over accepted (`1`, `12,`, `12,3`, `12,34`, `999999999,99`) and rejected (`a`, `1,2,`, `12,345`, `1000000000`, `-1`, `1.0`) inputs
- [ ] `ParseReadsCommaDecimals` passes (`1189,27` → 1189.27, empty → 0)
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the amount input filter`

---

### T17: AddItemForm

**What**: `Ledger/AddItemForm`, the add modal's state and transitions from the design.
**Where**: `src/Host/Gibeira.Web/Ledger/AddItemForm.cs`
**Depends on**: T16
**Reuses**: `AmountInput`, `PtBr`
**Requirement**: ADD-04, ADD-08, ADD-16, ADD-17, ADD-18, ADD-19, ADD-20, ADD-22, ADD-23

**Done when**:

- [ ] `ChoosingTypeSetsPlaceholderAndButtonLabel`, `RepetitionLabelFollowsChoice` (including `parcelado em 3x`), `DueDateRelabelsDataRowAsVencimento`, `SemCartaoKeepsDataLabel`, `LeavingCartaoClearsCard`, `TagsLabelJoinsCheckedNames`, `AddButtonDisabledWhileAmountZeroOrEmpty`, `EmptyDescriptionSavesTypeName` and `SubmitReturnsItemOnce` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the add-item form state`

---

### T18: Mockup Schedule

**What**: `Mockup/Schedule.Expand` and the mockup's `LedgerItem` and `Occurrence` records (temporary, AD-007).
**Where**: `src/Host/Gibeira.Web/Mockup/Schedule.cs`
**Depends on**: None (runs after Phase 4)
**Reuses**: `NewItem`, `Repetition`
**Requirement**: ADD-10, ADD-11, ADD-12

**Done when**:

- [ ] `MonthlyRepeatsOnSameDayClampedToMonthEnd` (31/01 → 28/02 or 29/02 → 31/03), `WeeklyAndDailyRepeatWithNoEnd` and `InstallmentsSplitWithRemainderOnFirst` (100,00 / 3 → 33,34 + 33,33 + 33,33, labels `parcela 1/3` to `parcela 3/3`) pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the mockup repetition schedule`

---

### T19: Mockup LedgerCalculator

**What**: `Mockup/LedgerCalculator` folding occurrences into `MonthLedger`s with the running saldo (temporary, AD-007).
**Where**: `src/Host/Gibeira.Web/Mockup/LedgerCalculator.cs`
**Depends on**: T18
**Reuses**: `Schedule`
**Requirement**: HOME-10, HOME-13

**Done when**:

- [ ] `SaldoCarriesPreviousDayAcrossMonths` and `MonthTotalsSumEachKind` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the mockup saldo calculator`

---

### T20: Mockup CardDueDates

**What**: `Mockup/CardDueDates`: a card's first due date on or after a date, stepped by months (temporary, AD-007).
**Where**: `src/Host/Gibeira.Web/Mockup/CardDueDates.cs`
**Depends on**: T19
**Reuses**: `Card`
**Requirement**: ADD-14, ADD-15

**Done when**:

- [ ] `FirstDueDateOnOrAfter` (Itaú on 29/10/2026 → 10/11/2026; on 10/11/2026 → 10/11/2026) and `DueDateStepsByMonth` (→ 10/12/2026, ← 10/10/2026) pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the mockup card due dates`

---

### T21: Mockup store and seed

**What**: `Mockup/InMemoryLedgerStore` implementing `ILedgerStore`, `Mockup/MockSeed`, and the scoped registration plus `TimeProvider.System` in `Program.cs` (temporary, AD-007).
**Where**: `src/Host/Gibeira.Web/Mockup/InMemoryLedgerStore.cs`
**Depends on**: T20
**Reuses**: `Schedule`, `LedgerCalculator`, `CardDueDates`
**Requirement**: MOCK-01, DAY-04, DAY-12

**Done when**:

- [ ] `SeedHasEveryKindThisMonthAndANegativeSaldo` passes for several `today` values, including 2026-10-29 and a 31st
- [ ] `RemoveDropsOnlyThatOccurrence` and `DayListsEntriesInAddedOrder` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): add the mockup ledger store and seed`

---

### T22: PeriodBar

**What**: `Home/PeriodBar`: `<<`, `<`, the period pill with its `Dropdown`, `>`, `>>`.
**Where**: `src/Host/Gibeira.Web/Components/Home/PeriodBar.razor`
**Depends on**: None (runs after Phase 5)
**Reuses**: `IconButton`, `Dropdown`, `Period`
**Requirement**: HOME-03, HOME-04, HOME-05, HOME-06

**Done when**:

- [ ] `PeriodArrowsMoveOneMonth`, `PeriodDoubleArrowsMoveTwelveMonths`, `PeriodPillListsTwentyFivePeriodsWithCurrentChecked` and `ChoosingPeriodShowsItAndClosesDropdown` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add the period bar`

---

### T23: MonthTable

**What**: `Home/MonthTable`: title, header row, day rows, total row, weekend and today classes, saldo tint, and `OnCell(date, kind?)`.
**Where**: `src/Host/Gibeira.Web/Components/Home/MonthTable.razor`
**Depends on**: T22
**Reuses**: `KindIcon`, `Money`, `PtBr`
**Requirement**: HOME-07, HOME-08, HOME-09, HOME-12, HOME-13, HOME-14, HOME-15

**Done when**:

- [ ] `MonthTableHasPtBrTitleColumnsAndOneRowPerDay`, `MoneyCellShowsKindIconAndDaySum`, `ZeroCellIsMuted`, `SaldoCellTintFollowsSign`, `TotalRowHoldsMonthSumsAndEmptySaldo`, `WeekendDiaCellsAreTinted` and `TodayRowIsMarked` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add the month table`

---

### T24: Home page ledger

**What**: `Pages/Home.razor` composing `TopBar`, `PeriodBar` and twelve `MonthTable`s from `ILedgerStore` and `TimeProvider`, with the page layout CSS. Rewrites `WebHomePageReturnsGibeira` to assert the period label.
**Where**: `src/Host/Gibeira.Web/Components/Pages/Home.razor`
**Depends on**: T23
**Reuses**: `TopBar`, `PeriodBar`, `MonthTable`
**Requirement**: HOME-01, HOME-02, HOME-07, HOME-16, HOME-17, MOCK-03

**Done when**:

- [ ] bUnit: `PeriodStartsAtCurrentMonthWithPtBrLabel`, `MonthTablesFollowPeriodInOrder` and `HomeReadsDataOnlyThroughILedgerStore` (a stub store's values appear) pass
- [ ] Playwright: `HomeShowsTopBarPeriodBarAndMonthTablesInOrder`, `HeaderAndTotalRowsStayInViewWhileDaysScroll` and `LedgerScrollsSidewaysWithoutPageScrollAt360` pass
- [ ] `WebHomePageReturnsGibeira` passes with the period label
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`

**Tests**: e2e
**Gate**: full

**Commit**: `feat(web): show the twelve-month ledger on the home page`

---

### T25: DayDrawer

**What**: `Home/DayDrawer` over `Drawer`: title, `<` `>`, `+`, X, filter `Dropdown`, entry list with ⋮ and `excluir`, and `nenhum lançamento`.
**Where**: `src/Host/Gibeira.Web/Components/Home/DayDrawer.razor`
**Depends on**: T24
**Reuses**: `Drawer`, `Dropdown`, `IconButton`, `KindIcon`, `Money`, `PtBr`
**Requirement**: DAY-03, DAY-04, DAY-05, DAY-06, DAY-07, DAY-08, DAY-09, DAY-11

**Done when**:

- [ ] `DrawerTitleIsDayAndMonthAbbrev`, `DrawerListsEntriesInAddedOrderWithLabels`, `DrawerArrowsStepDayKeepingFilter`, `DrawerFilterListsKindsWithCurrentChecked`, `DrawerFilterShowsOnlyThatKind`, `EmptyDayShowsNenhumLancamento`, `DrawerClosesOnXAndEsc` and `ItemMenuOffersExcluir` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add the day drawer`

---

### T26: Drawer on the home page

**What**: Wire `DayDrawer` into `Home.razor`: a money cell opens it on the cell's kind, a dia cell on `todos`, and `excluir` removes the occurrence.
**Where**: `src/Host/Gibeira.Web/Components/Pages/Home.razor` (modify)
**Depends on**: T25
**Reuses**: `DayDrawer`, `ILedgerStore.Remove`
**Requirement**: DAY-01, DAY-02, DAY-04, DAY-12, SCR-02

**Done when**:

- [ ] bUnit: `MoneyCellOpensDrawerFilteredByKind`, `DiaCellOpensDrawerWithTodos` and `ExcluirRemovesOccurrenceAndUpdatesSaldos` pass
- [ ] Playwright: `DrawerFillsWidthBelow432` and `LongDescriptionIsCutWithEllipsis` pass
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`

**Tests**: e2e
**Gate**: full

**Commit**: `feat(web): open the day drawer from the ledger`

---

### T27: AddItemModal

**What**: `Home/AddItemModal` over `Modal` and `AddItemForm`: the six rows, the type, card, repetir and tags dropdowns, and the data, vencimento and parcelas popovers.
**Where**: `src/Host/Gibeira.Web/Components/Home/AddItemModal.razor`
**Depends on**: T26
**Reuses**: `Modal`, `FieldRow`, `Dropdown`, `StepperPopover`, `Button`, `AddItemForm`, `ILedgerStore.DueDate`
**Requirement**: ADD-01, ADD-03, ADD-04, ADD-05, ADD-06, ADD-07, ADD-08, ADD-09, ADD-13, ADD-14, ADD-15, ADD-16, ADD-17, ADD-19, ADD-20, ADD-24, ADD-25

**Done when**:

- [ ] `AddModalOpensWithRowsAndFocusedAmount`, `TypeRowListsKindsWithCurrentChecked`, `ChoosingTypeUpdatesRowPlaceholderAndButton`, `DataPopoverStepsOneDay`, `DataSalvarSetsDateAndCloses`, `RepetirListsOptionsWithDescriptions`, `ChoosingRepetitionShowsItOnRow`, `ParceladoOpensCountPopoverFrom2To48`, `CartaoOpensCardDropdown`, `ChoosingCardOpensVencimentoAtFirstDueDate`, `VencimentoArrowsStepOneMonth`, `VencimentoSalvarSetsDateAndRelabelsRow`, `SemCartaoOpensNoVencimento`, `TagsDropdownChecksSeveral`, `AddButtonIsDisabledForZeroAmount`, `ModalClosesOnXEscAndBackdropWithoutAdding` and `PopoverClosesOnEscAndOutsideKeepingValue` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**Tests**: component
**Gate**: quick

**Commit**: `feat(web): add the add-item modal`

---

### T28: Modal on the home page

**What**: Wire `AddItemModal` into `Home.razor` from the top bar and the drawer's `+`; adding writes to the store and closes the modal.
**Where**: `src/Host/Gibeira.Web/Components/Pages/Home.razor` (modify)
**Depends on**: T27
**Reuses**: `AddItemModal`, `ILedgerStore.Add`
**Requirement**: HOME-18, DAY-10, ADD-02, ADD-21, ADD-23, MOCK-02, SCR-01, UI-06

**Done when**:

- [ ] bUnit: `TopBarAddOpensModalWithEntradaAndToday`, `DrawerPlusOpensModalWithDayAndKind`, `AddingItemUpdatesTablesAndOpenDrawer`, `DoubleSubmitAddsOnce` and `IconOnlyControlsHaveAriaLabelsAndDialogsHaveRole` pass
- [ ] Playwright: `AmountFieldDropsRejectedCharacters`, `ReloadDiscardsChanges` and `ModalFillsWidthMinusGuttersBelow432` pass
- [ ] Gate check passes: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`

**Tests**: e2e
**Gate**: build

**Commit**: `feat(web): add items from the home page`

---

## Checks

**C1** - `tokens.css` defines every value of the spec's token table as a custom property, each value once (UI-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*TokensDefineEachCanvasValueOnce"`

**C2** - A color literal in a `.razor` or `.css` file outside `tokens.css` and `lib/` fails the scan and is reported as `file:line` (UI-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ColorLiteralIsReportedWithFileAndLine"`

**C3** - `Gibeira.Web` has no color literal outside `tokens.css` and `lib/` (UI-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*NoColorLiteralOutsideTokens"`

**C4** - All 11 components render from parameters alone, in a bUnit context with no services (UI-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*EveryUiComponentRendersFromParametersAlone"`

**C5** - While `/` loads, every request goes to a loopback host and both font families load (UI-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*HomePageLoadsNothingFromAnotherHost"`

**C6** - Every `@font-face` points to a file under `wwwroot/fonts/` (UI-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*FontFacesPointToLocalFiles"`

**C7** - `/` and its stylesheets load no Bootstrap (UI-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebHomePageLoadsNoBootstrap"`

**C8** - With the drawer, the modal and a popover open, every icon-only control has a pt-BR `aria-label` and every modal, drawer and popover has `role="dialog"` (UI-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*IconOnlyControlsHaveAriaLabelsAndDialogsHaveRole"`

**C9** - `/` shows the top bar, then the period bar, then the month tables side by side (HOME-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*HomeShowsTopBarPeriodBarAndMonthTablesInOrder"`

**C10** - On 2026-10-29 the period reads `out/2026 - set/2027` (HOME-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*PeriodStartsAtCurrentMonthWithPtBrLabel"`

**C11** - `<` and `>` move the period one month (HOME-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*PeriodArrowsMoveOneMonth"`

**C12** - `<<` and `>>` move the period 12 months (HOME-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*PeriodDoubleArrowsMoveTwelveMonths"`

**C13** - The period pill lists 25 periods with the shown one checked (HOME-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*PeriodPillListsTwentyFivePeriodsWithCurrentChecked"`

**C14** - Choosing a period shows it and closes the dropdown (HOME-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ChoosingPeriodShowsItAndClosesDropdown"`

**C15** - Each month table is titled `dezembro de 2026` style, with the columns dia to saldos and one row per calendar day (HOME-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*MonthTableHasPtBrTitleColumnsAndOneRowPerDay"`

**C16** - The page shows 12 month tables in the period's month order (HOME-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*MonthTablesFollowPeriodInOrder"`

**C17** - A money cell shows its kind icon and the day's sum as `R$ 1.189,27` (HOME-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*MoneyCellShowsKindIconAndDaySum"`

**C18** - A zero cell shows its icon and `R$ 0,00` in the zero color (HOME-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ZeroCellIsMuted"`

**C19** - Each saldo is the previous day's plus entradas minus the other kinds, from the opening balance and across months (HOME-10, AC 10)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*SaldoCarriesPreviousDayAcrossMonths"`

**C20** - A negative amount reads `R$ -477,67` (HOME-11, AC 11)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*NegativeAmountFormatsWithMinusAfterSymbol"`

**C21** - A saldo of zero or more gets the positive tint, a negative one the negative tint (HOME-12, AC 12)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*SaldoCellTintFollowsSign"`

**C22** - The total row holds each kind's month sum with its icon and an empty saldos cell (HOME-13, AC 13)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*TotalRowHoldsMonthSumsAndEmptySaldo"`

**C23** - Saturday and Sunday dia cells get the weekend ground (HOME-14, AC 14)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*WeekendDiaCellsAreTinted"`

**C24** - Today's row gets the today ground and an accent day number (HOME-15, AC 15)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*TodayRowIsMarked"`

**C25** - After scrolling the day rows, the header and total rows are still in view (HOME-16, AC 16)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*HeaderAndTotalRowsStayInViewWhileDaysScroll"`

**C26** - At 360 px the ledger scrolls sideways and the page has no horizontal scroll (HOME-17, AC 17)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*LedgerScrollsSidewaysWithoutPageScrollAt360"`

**C27** - "adicionar" in the top bar opens the modal with entrada and today's date (HOME-18, AC 18)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*TopBarAddOpensModalWithEntradaAndToday"`

**C28** - A money cell opens the drawer on its day, filtered by its kind (DAY-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*MoneyCellOpensDrawerFilteredByKind"`

**C29** - A dia cell opens the drawer with `todos` (DAY-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DiaCellOpensDrawerWithTodos"`

**C30** - The drawer title reads `10/nov` (DAY-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerTitleIsDayAndMonthAbbrev"`

**C31** - The drawer lists the day's matching entries in added order with icon, description, amount, `dd/mm`, ⋮ and the repetition label (DAY-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerListsEntriesInAddedOrderWithLabels"`

**C32** - A description too long for the row ends in `…` (DAY-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*LongDescriptionIsCutWithEllipsis"`

**C33** - `<` and `>` in the drawer step one day and keep the filter (DAY-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerArrowsStepDayKeepingFilter"`

**C34** - The filter pill lists `todos` to `cartão` with the current one checked (DAY-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerFilterListsKindsWithCurrentChecked"`

**C35** - A filter shows only its kind; `todos` shows all (DAY-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerFilterShowsOnlyThatKind"`

**C36** - A day with no matching entry shows `nenhum lançamento` (DAY-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*EmptyDayShowsNenhumLancamento"`

**C37** - X and Esc close the drawer (DAY-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerClosesOnXAndEsc"`

**C38** - `+` in the drawer opens the modal with the drawer's day and the filter's kind, or entrada for `todos` (DAY-10, AC 10)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerPlusOpensModalWithDayAndKind"`

**C39** - ⋮ opens a menu with `excluir` (DAY-11, AC 11)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ItemMenuOffersExcluir"`

**C40** - `excluir` removes that occurrence, and the drawer, cell, totals and later saldos update (DAY-12, AC 12)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ExcluirRemovesOccurrenceAndUpdatesSaldos"`

**C41** - The modal opens with title, X, the six rows, `adicionar entrada` and the amount focused (ADD-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*AddModalOpensWithRowsAndFocusedAmount"`

**C42** - The amount accepts digits and one comma with two decimals up to `999.999.999,99` and rejects anything else (ADD-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*AmountAcceptsDigitsAndOneCommaWithTwoDecimals"`

**C43** - Typing a letter or a third decimal leaves the field's text unchanged in the browser (ADD-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*AmountFieldDropsRejectedCharacters"`

**C44** - The type row lists the five kinds with the current one checked (ADD-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*TypeRowListsKindsWithCurrentChecked"`

**C45** - Choosing a type updates the row, the placeholder, and the button's label and color (ADD-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ChoosingTypeUpdatesRowPlaceholderAndButton"`

**C46** - The data popover shows `dd/mm/yyyy` and `<` `>` step one day (ADD-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DataPopoverStepsOneDay"`

**C47** - `salvar` sets the date, shows `dd/mm/yy` on the row and closes the popover (ADD-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DataSalvarSetsDateAndCloses"`

**C48** - The repetition row opens `repetir` with the four options and their descriptions, plus `não repete` (ADD-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*RepetirListsOptionsWithDescriptions"`

**C49** - Choosing mensalmente, semanalmente, diariamente or não repete shows it on the row (ADD-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ChoosingRepetitionShowsItOnRow"`

**C50** - `parcelado` opens `parcelas` from 2 to 48, starting at 2, and `salvar` shows `parcelado em <N>x` (ADD-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ParceladoOpensCountPopoverFrom2To48"`

**C51** - A monthly item repeats on the same day, on the last day when the month is shorter (ADD-10, AC 10)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*MonthlyRepeatsOnSameDayClampedToMonthEnd"`

**C52** - Weekly and daily items repeat every 7 days and every day with no end (ADD-11, AC 11)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*WeeklyAndDailyRepeatWithNoEnd"`

**C53** - `parcelado em 3x` of R$ 100,00 gives 33,34 + 33,33 + 33,33 in three months (ADD-12, AC 12)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*InstallmentsSplitWithRemainderOnFirst"`

**C54** - Choosing `cartão` opens the card dropdown with the disabled add-card button, Itaú, Santander and sem cartão (ADD-13, AC 13)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*CartaoOpensCardDropdown"`

**C55** - Choosing Itaú on 29/10/2026 opens `vencimento` at 10/11/2026 (ADD-14, AC 14)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ChoosingCardOpensVencimentoAtFirstDueDate"`

**C56** - `>` in `vencimento` shows 10/12/2026 and `<` goes back one month (ADD-15, AC 15)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*VencimentoArrowsStepOneMonth"`

**C57** - `salvar` in `vencimento` sets the date and labels the row `vencimento` (ADD-16, AC 16)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*VencimentoSalvarSetsDateAndRelabelsRow"`

**C58** - `sem cartão` keeps the row labeled `data` and opens no popover (ADD-17, AC 17)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*SemCartaoOpensNoVencimento"`

**C59** - Leaving `cartão` clears the card and relabels the row `data` (ADD-18, AC 18)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*LeavingCartaoClearsCard"`

**C60** - The tags dropdown checks several, and the row shows them joined by `, `, or `tags` when none (ADD-19, AC 19)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*TagsDropdownChecksSeveral"`

**C61** - The add button is disabled while the amount is zero or empty (ADD-20, AC 20)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*AddButtonIsDisabledForZeroAmount"`

**C62** - Adding closes the modal and updates the tables, totals, saldos and the open drawer (ADD-21, AC 21)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*AddingItemUpdatesTablesAndOpenDrawer"`

**C63** - An empty description is saved as the type's name (ADD-22, AC 22)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*EmptyDescriptionSavesTypeName"`

**C64** - Two clicks on the add button add one item (ADD-23, AC 23)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DoubleSubmitAddsOnce"`

**C65** - X, Esc and a backdrop click close the modal and add nothing (ADD-24, AC 24)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*ModalClosesOnXEscAndBackdropWithoutAdding"`

**C66** - Esc or an outside click closes a dropdown or popover and keeps the previous value (ADD-25, AC 25)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*PopoverClosesOnEscAndOutsideKeepingValue"`

**C67** - The seed gives every kind a non-zero day in the current month and a negative saldo within 12 months (MOCK-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*SeedHasEveryKindThisMonthAndANegativeSaldo"`

**C68** - An item added before a reload is gone after it (MOCK-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*ReloadDiscardsChanges"`

**C69** - The page shows what a stub `ILedgerStore` returns (MOCK-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*HomeReadsDataOnlyThroughILedgerStore"`

**C70** - At 400 px the modal is the window width minus 16 px each side (SCR-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*ModalFillsWidthMinusGuttersBelow432"`

**C71** - At 400 px the drawer is the full window width (SCR-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*DrawerFillsWidthBelow432"`

**C72** - The page in the canvas look: tokens, fonts, pill buttons, warm grounds, at 1290×902 and 360 px with the drawer, modal, dropdowns and popovers open (Success Criteria)
Proof: walk - open `/` at both widths, open a cartão cell, the add modal, each dropdown and popover, and compare against the canvas boards `Main.dc.html` and `Adicionar.dc.html`

---

## Handoff

The feature's live state, written as it happens - a compaction gives no warning.

**Settled mid-build:** none
**Dispatched:** none
**Walked:** none

---

## Phase Execution Map

```
Phase 1 → Phase 2 → Phase 3 → Phase 4 → Phase 5 → Phase 6

Phase 1:  T1 → T2 → T3 → T4
Phase 2:  T5 → T6 → T7 → T8
Phase 3:  T9 → T10 → T11 → T12 → T13
Phase 4:  T14 → T15 → T16 → T17
Phase 5:  T18 → T19 → T20 → T21
Phase 6:  T22 → T23 → T24 → T25 → T26 → T27 → T28
```

Execution is strictly sequential. 28 tasks pack into four batches of about seven (Phases 1–2, 3–4, 5, 6), so Execute starts with the offer to run them as batch sub-agents.
