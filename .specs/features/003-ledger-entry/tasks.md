# Ledger Entry Tasks

## Execution Protocol (MANDATORY -- do not skip)

Implement these tasks with the `tlc-spec-driven` skill: **activate it by name and follow its Execute flow and Critical Rules.** Do not search for skill files by filesystem path. The skill is the source of truth for the full flow (per-task cycle, sub-agent delegation, the walk, check_proofs, the Verifier and the screens sub-agent).

**If the skill cannot be activated, STOP and tell the user - do not proceed without it.**

---

**Design**: `.specs/features/003-ledger-entry/design.md`
**Status**: In Progress
**Approved**: 2026-11-06 - user: "start phase 1"; execution: inline, phase by phase
**Mutations**: 3
**Build**: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`

---

## Test Coverage Matrix

> Generated from the approved spec and design, the existing tests and `CONTRIBUTING.md` (format hook). Guidelines found: no testing guideline in `AGENTS.md`, `CONTRIBUTING.md` or `docs/`, so strong defaults apply. `docs/architecture.md` rules are enforced by `tests/Gibeira.ArchitectureTests`, which already references `src/Modules/**/*.csproj`.

| Code Layer | Required Test Type | Coverage Expectation | Location Pattern | Run Command |
| ---------- | ------------------ | -------------------- | ---------------- | ----------- |
| Ledger domain (`Gibeira.Ledger.Domain`: value objects, aggregates, factories) | unit | All branches; 1:1 to ENT, REP, INS, CARD ACs; every listed edge case; no page and no storage | `tests/Gibeira.Ledger.Tests/Domain/*.cs` | `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj` |
| Ledger use cases and projection (`Gibeira.Ledger.Application`) | unit, over the in-memory repositories | Every REP, INS, ADDT, DEL, ORD AC the use case owns: happy path, each refusal storing nothing, idempotent reads | `tests/Gibeira.Ledger.Tests/Application/*.cs` | `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj` |
| Ledger storage and seed (`Gibeira.Ledger.Infrastructure`) | unit | Key paths: add, order counter, remove (present and gone), date queries; the seed meets MOCK-01 | `tests/Gibeira.Ledger.Tests/Infrastructure/*.cs` | `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj` |
| Web adapter and helpers (`Ledger/ModuleLedgerStore`, `LedgerModule`, `AddItemForm`, `AmountInput`) | unit | Every mapping and label the page reads; DI scope per circuit; MDL-02, INS-06 | `tests/Gibeira.Web.Tests/Ledger/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Home components and page (`Components/Home/*`, `Pages/Home.razor`) | component (bUnit), on the real module | Every MDL AC through user gestures; spec 002's tests keep passing (MOD-08) | `tests/Gibeira.Web.Tests/Home/*.cs` | `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` |
| Repository rules (layout, deleted folders, naming language) | unit (file scan) | MOD-01, MOD-03, MOD-09 with file and line, each with a deliberate violation | `tests/Gibeira.RepositoryTests/*.cs` | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj` |
| Architecture rules | unit (ArchUnitNET) | Every `*HoldsOnSolution` test passes with the module in place (MOD-02); no new rule | `tests/Gibeira.ArchitectureTests/*.cs` | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj` |
| Layout and browser behaviour | e2e (Playwright on the Aspire-hosted app) | Spec 002's browser tests keep passing, renamed only; reload still discards changes (MOD-06) | `tests/Gibeira.IntegrationTests/Browser/*.cs` | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj` |
| Ports, request records, DI wiring, project files | none | build gate only | - | build gate only |

## Gate Check Commands

| Gate Level | When to Use | Command |
| ---------- | ----------- | ------- |
| Quick | Tasks whose tests live in one unit project | `dotnet test --project <that task's test project>` (Ledger.Tests, Web.Tests or RepositoryTests, as the task names) |
| Full | Tasks that change the page, its store or the solution layout | `dotnet test --solution Gibeira.slnx` |
| Build | Config-only tasks and phase ends | `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx` |

---

## Execution Plan

Phases run in order; tasks within a phase run in order.

### Phase 1: English identifiers (AD-011)

The rename comes first, so every later commit is written in English and the naming test guards it.

```
T1 → T2
```

### Phase 2: Ledger domain

```
T3 → T4 → T5 → T6 → T7 → T8 → T9
```

### Phase 3: Use cases and in-memory storage

```
T10 → T11 → T12 → T13 → T14 → T15 → T16
```

### Phase 4: The page reads the module

```
T17 → T18 → T19 → T20
```

---

## Task Breakdown

### T1: Naming language scan

**What**: Add a scan that reports, as `file:line`, every `.cs`, `.razor` or `.css` file under `src/` and `tests/` using a word of the spec's naming table as an identifier: C# identifiers outside string literals and comments, CSS class names and custom properties, and `class` attribute values in `.razor`. Screen text and string literals are not identifiers. Files come from `RepositoryRoot.ListFilesAsync`, so `bin/` and `obj/` are never read. This task adds only the scanner and its deliberate-violation test; the solution-wide test arrives with the rename in T2.
**Where**: `tests/Gibeira.RepositoryTests/NamingLanguageTests.cs`
**Depends on**: None
**Reuses**: `tests/Gibeira.Web.Tests/Styles/ColorLiterals.cs` scan-and-report shape, `RepositoryRoot.ListFilesAsync`
**Requirement**: MOD-09

**Done when**:

- [x] `PortugueseIdentifierIsReportedWithFileAndLine` passes: sample `.cs`, `.razor` and `.css` text holding `Kind.Cartao`, `class="saldo-cell"` and `--kind-entrada` is reported with each file and line, while `"saldo"` in a string literal, a comment and markup text `entrada` are not
- [x] Gate check passes: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj`
- [x] Test count: RepositoryTests 36 → 37 (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `test(repo): report Portuguese identifiers with file and line`

---

### T2: Rename Portuguese identifiers to English

**What**: Rename every Portuguese identifier across `Gibeira.Web`, `Gibeira.Web.Tests` and `Gibeira.IntegrationTests` with no behaviour change: `Kind.Entrada` to `Kind.Income` (and `Saida`→`Expense`, `Diario`→`DailySpending`, `Economia`→`Savings`, `Cartao`→`CreditCard`), `DayRow.Saldo` to `Balance`, `.saldo-cell` to `.balance-cell`, `--kind-entrada` to `--kind-income` and the other kind tokens, `--saldo-positive`/`--saldo-negative` to `--balance-*`, the modal's `"mensal"` style keys to `"monthly"`, and test names such as `CartaoOpensCardDropdown` to `CreditCardOpensCardDropdown`. Strings shown on screens and the seed's sample data stay pt-BR. Add the solution-wide naming test.
**Where**: `src/Host/Gibeira.Web/Ledger/Kind.cs`
**Depends on**: T1
**Reuses**: The naming table in the spec; `TokenTests` and the Playwright tests catch a missed class or token
**Requirement**: MOD-09

**Done when**:

- [x] `CodeHasNoPortugueseIdentifiers` passes over `src/` and `tests/`
- [x] Every Web.Tests, IntegrationTests and ArchitectureTests test that passed before still passes, under its English name, with its assertions unchanged
- [x] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [x] Test count: solution 324 → 325, the new one `CodeHasNoPortugueseIdentifiers`; no test attribute removed (no silent deletions)

**Tests**: unit
**Gate**: full

**Commit**: `refactor(web): rename Portuguese identifiers to English`

---

### T3: Ledger module and its test project

**What**: Create `Gibeira.Ledger.Domain`, `.Application` and `.Infrastructure` with `dotnet new gibeira-module -n Ledger`, `tests/Gibeira.Ledger.Tests` (xUnit v3, referencing the three projects, `Exe`, like `Gibeira.Web.Tests`), all four in `Gibeira.slnx`, and `LedgerRuleException` in Domain. Add the layout test.
**Where**: `tests/Gibeira.RepositoryTests/StructureTests.cs`
**Depends on**: None (runs after Phase 1)
**Reuses**: `templates/module`, `tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj` project shape
**Requirement**: MOD-01, MOD-02

**Done when**:

- [x] `LedgerModuleHasThreeLayerProjects` passes: the three projects exist under `src/Modules/Ledger/` and are in `Gibeira.slnx`
- [x] Every `*HoldsOnSolution` architecture test passes with the module loaded, and `EveryProjectTargetsNet10WithNullableAndWarningsAsErrors` still passes
- [x] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [x] Test count: RepositoryTests grows by 1 (no silent deletions)

**Tests**: unit
**Gate**: full

**Commit**: `feat(ledger): add the Ledger module and its test project`

---

### T4: Money value object

**What**: `Money` in Domain: above zero, at most `999_999_999.99`, at most two decimals; `Of`, `Max`, `CanSplit(value, parts)` and `SplitInto(parts)`, the first part carrying the remainder.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Domain/Money.cs`
**Depends on**: T3
**Reuses**: The split in `src/Host/Gibeira.Web/Mockup/Schedule.cs` and the `InstallmentsSplitWithRemainderOnFirst`, `InstallmentsSumToTheAmount` cases
**Requirement**: ENT-04, ENT-05, ENT-06, INS-02, INS-05

**Done when**:

- [ ] `MoneyRefusesZeroAndNegative`, `MoneyRefusesAboveMax`, `MoneyAcceptsMax`, `MoneyRefusesThreeDecimals` pass
- [ ] `SplitGivesFirstPartTheRemainder` (100,00 in 3 → 33,34, 33,33, 33,33) and `SplitPartsSumToTheAmount` pass
- [ ] `CanSplitNeedsOneCentPerPart` passes: 0,02 in 2 is allowed, 0,01 in 2 is not
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 7 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the Money value object`

---

### T5: Tag and EntryDetails

**What**: `Tag` (compared by name, not blank) and `EntryDetails.Create(kind, amount, description, cardId, tags)`: every rule an entry shares with the rule that creates it, so a LedgerEntry can only be built from valid details. `EntryKind` and the typed ids (`CardId` and the others, `New()` over `Guid.CreateVersion7()`) arrive here.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Domain/EntryDetails.cs`
**Depends on**: T4
**Reuses**: `Money` from T4
**Requirement**: ENT-07, ENT-08, ENT-09, ENT-10, ENT-11, ENT-12

**Done when**:

- [ ] `BlankDescriptionIsRefused` (empty and spaces), `CreditCardWithoutCardIsRefused`, `CardOnOtherKindIsRefused` (each of the four other kinds), `BlankTagIsRefused`, `RepeatedTagIsRefused` pass, each throwing `LedgerRuleException`
- [ ] `TagsKeepTheirGivenOrder` passes
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 13 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add Tag and EntryDetails`

---

### T6: LedgerEntry aggregate

**What**: `LedgerEntry` with `EntryOrigin` (`Single`, `Repeat`, `Installment`), equal by id only, with no setter or mutating method, `BalanceEffect` signed by kind, and `Single(date, details, addOrder)`.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Domain/LedgerEntry.cs`
**Depends on**: T5
**Reuses**: The nested sealed records of `Repetition` in `src/Host/Gibeira.Web/Ledger/ILedgerStore.cs`
**Requirement**: ENT-01, ENT-02, ENT-03, ENT-13, ENT-14

**Done when**:

- [ ] `EntryReadsBackComplete` passes: id, day, kind, amount, description, card id on cartão, tags and a single origin
- [ ] `EntriesAreEqualOnlyByIdentity` passes: two entries with equal values and different ids differ; the same entry equals itself
- [ ] `EntryHasNoMutatingMember` passes: by reflection, no public setter, no public non-readonly field and no public method other than queries
- [ ] `IncomeAddsToBalance` and `OtherKindsSubtractFromBalance` (four kinds) pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 18 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the LedgerEntry aggregate`

---

### T7: Card aggregate

**What**: `Card.Create(name, dueDay)` refusing a blank name and a day outside 1 to 31, and `DueDate(onOrAfter, monthsAhead)` clamped to the month's last day.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Domain/Card.cs`
**Depends on**: T6
**Reuses**: `src/Host/Gibeira.Web/Mockup/CardDueDates.cs` and its three test cases
**Requirement**: CARD-01, CARD-02, CARD-03, CARD-04, CARD-05

**Done when**:

- [ ] `CardReadsBackIdNameAndDueDay`, `DueDayOutsideMonthIsRefused` (0 and 32), `BlankCardNameIsRefused` pass
- [ ] `FirstDueDateOnOrAfter` (Itaú day 10 from 29/10/2026 → 10/11/2026), `DueDateStepsByMonth` (→ 10/12/2026) and `DueDayClampsToMonthLength` (day 31 in February) pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 24 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the Card aggregate`

---

### T8: RecurringRule aggregate

**What**: `RecurringRule.Create(firstDate, details, frequency, addOrder)` and `GenerateUpTo(date)`, which returns the entries from its private next index up to the date and moves the index forward only. Monthly steps with `AddMonths` from the first date, so day 31 clamps without drift.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Domain/RecurringRule.cs`
**Depends on**: T7
**Reuses**: The repetition loops in `src/Host/Gibeira.Web/Mockup/Schedule.cs` and `MonthlyRepeatsOnSameDayClampedToMonthEnd`, `WeeklyAndDailyRepeatWithNoEnd`
**Requirement**: REP-03, REP-04, REP-05, REP-06, REP-07, REP-08, REP-11

**Done when**:

- [ ] `FirstEntryFallsOnFirstDay`, `MonthlyFromDay31ClampsWithoutDrift` (31/01, 28/02, 31/03), `WeeklyStepsSevenDays`, `DailyStepsOneDay` pass
- [ ] `GeneratedEntryCopiesRuleDetailsAndOrigin` passes: kind, amount, description, card, tags, add order and `Repeat(ruleId, frequency)`
- [ ] `GeneratingTheSameDateTwiceCreatesNothingNew` and `GeneratingBeforeFirstDayCreatesNothing` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 31 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the RecurringRule aggregate`

---

### T9: InstallmentPlan factory

**What**: `InstallmentPlan.Create(firstDate, details, parts, addOrder)`: N entries one month apart, clamped like the monthly rule, sharing one plan id, numbered 1 to N, amounts from `Money.SplitInto`; refuses N outside 2 to 48 and an amount below R$ 0,01 per part.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Domain/InstallmentPlan.cs`
**Depends on**: T8
**Reuses**: `Money.SplitInto` and `CanSplit` from T4
**Requirement**: INS-01, INS-02, INS-03, INS-04, INS-05

**Done when**:

- [ ] `PlanCreatesOneEntryPerMonthClamped` (from day 31) and `PlanOfOneHundredInThreeSplitsWithRemainderOnFirst` pass
- [ ] `PartsShareOnePlanIdAndNumberOneToN` passes
- [ ] `PartCountOutsideTwoToFortyEightIsRefused` (1 and 49) and `AmountBelowOneCentPerPartIsRefused` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 36 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the InstallmentPlan factory`

---

### T10: Application ports and request records

**What**: `ILedgerEntryRepository`, `IRecurringRuleRepository`, `ILedgerCatalog`, `ItemRepetition` and `NewLedgerItem`, as in the design.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Application/Ports.cs`
**Depends on**: None (runs after Phase 2)
**Reuses**: The design's Application listing
**Requirement**: MOD-01

**Done when**:

- [ ] The ports and records compile with no Infrastructure or ASP.NET reference
- [ ] Gate check passes: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`

**Tests**: none
**Gate**: build

**Commit**: `feat(ledger): add the Ledger application ports`

---

### T11: In-memory storage and catalog

**What**: `InMemoryLedgerEntryRepository` (a list and an add-order counter; `Remove` ignores a missing id), `InMemoryRecurringRuleRepository` and `InMemoryLedgerCatalog` (Itaú day 10, Santander day 6, the five tags).
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Infrastructure/InMemoryLedgerEntryRepository.cs`
**Depends on**: T10
**Reuses**: `MockSeed.Cards` and `MockSeed.Tags` values
**Requirement**: DEL-01, DEL-03, ADDT-02

**Done when**:

- [ ] `NextAddOrderIncreases`, `OnReturnsOnlyThatDay`, `UpToReturnsEveryEntryUpToTheDate` pass
- [ ] `RemoveDeletesOnlyThatEntry` and `RemovingAMissingEntryChangesNothing` pass
- [ ] `CatalogFindsItsCardsAndNoOther` passes
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 42 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add in-memory ledger storage`

---

### T12: AddItem use case

**What**: `AddItem.Execute(NewLedgerItem)` checking, in order, `EntryDetails.Create`, the card exists, the repetition's own rules, and only then storing one single entry, one rule, or N installment entries, all with the next add order.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Application/AddItem.cs`
**Depends on**: T11
**Reuses**: The domain factories from T6 to T9; the in-memory storage from T11 in tests
**Requirement**: ADDT-01, ADDT-02, REP-01, REP-10, INS-01, INS-04, INS-05

**Done when**:

- [ ] `AddingNoRepeatStoresOneSingleEntry`, `AddingRecurringStoresOneRule` (each frequency), `AddingInstallmentsStoresNEntries` pass
- [ ] `UnknownCardStoresNothing`, `InvalidRuleStoresNothing`, `PartCountOutOfRangeStoresNothing`, `TooSmallInstallmentStoresNothing` pass: each throws and both repositories stay empty
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 49 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the AddItem use case`

---

### T13: RemoveEntry use case

**What**: `RemoveEntry.Execute(LedgerEntryId)`.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Application/RemoveEntry.cs`
**Depends on**: T12
**Reuses**: `AddItem` to build the ledger in tests
**Requirement**: DEL-01, DEL-02, DEL-03

**Done when**:

- [ ] `RemovingAnEntryDeletesOnlyIt`, `RemovingAPartKeepsOtherPartsAmountsAndNumbers` (`parcela 2/10` stays 2 of 10), `RemovingTwiceReportsNoError` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 52 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the RemoveEntry use case`

---

### T14: LedgerProjection

**What**: `LedgerProjection.Months(entries, monthStarts)` with `DaySummary` and `MonthSummary`: one row per day, sums by kind with zeros, the balance carried across months from every earlier entry, month totals.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Application/LedgerProjection.cs`
**Depends on**: T13
**Reuses**: `src/Host/Gibeira.Web/Mockup/LedgerCalculator.cs` and its tests' cases
**Requirement**: MOD-07, ENT-13, ENT-14

**Done when**:

- [ ] `BalanceCarriesPreviousDayAcrossMonths`, `DaySumsHoldEveryKindWithZeros`, `MonthTotalsSumEachKind`, `MonthsHaveOneRowPerDayInOrder` pass
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 56 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the ledger projection`

---

### T15: ReadLedger use case

**What**: `ReadLedger.Months(monthStarts)` and `Day(day, filter)`: first every rule generates up to the last date read and its new entries are stored, then the projection or the day's entries in add order. A comment says the read writes.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Application/ReadLedger.cs`
**Depends on**: T14
**Reuses**: `LedgerProjection` from T14
**Requirement**: REP-02, REP-08, REP-09, REP-11, ORD-01

**Done when**:

- [ ] `ReadingCreatesMissingEntriesUpToLastDate` passes: a monthly rule on day 31 read 14 months ahead holds one entry per month on the right days
- [ ] `ReadingTheSameDateTwiceCreatesNoDuplicate` and `RemovedRuleEntryNeverReturns` pass
- [ ] `RuleKeepsCreatingAfterEveryEntryIsRemoved` and `ReadingBeforeFirstDayCreatesNothing` pass
- [ ] `DayListsEntriesInAddOrderWithRulesInTheirAddPlace` passes: a monthly rule, then a single item on a later day the rule falls on; the rule's entry comes first
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 62 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): add the ReadLedger use case`

---

### T16: Ledger seed

**What**: `LedgerSeed.Run(AddItem, ILedgerCatalog, DateOnly today)` adding `MockSeed`'s items through `AddItem`, the cartão items naming Itaú and Santander by id.
**Where**: `src/Modules/Ledger/Gibeira.Ledger.Infrastructure/LedgerSeed.cs`
**Depends on**: T15
**Reuses**: `src/Host/Gibeira.Web/Mockup/MockSeed.cs`, and `SeedHasEveryKindThisMonthAndANegativeSaldo`, `SeedBooksCartaoOnBothCardsDueDays` cases
**Requirement**: MOD-05

**Done when**:

- [ ] `SeedHasEveryKindThisMonthAndANegativeBalance` and `SeedBooksCreditCardOnBothCardsDueDays` pass, reading through `ReadLedger`
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj`
- [ ] Test count: Ledger.Tests 64 tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(ledger): seed the ledger through AddItem`

---

### T17: The page reads the module

**What**: Swap the store in one step, because the contract change and the mockup cannot compile side by side. `ILedgerStore` changes as in the design (`Card.Id`, `NewItem.CardId`, `DayEntry.EntryId`, `Remove(Guid entryId)`); `ModuleLedgerStore` implements it over `AddItem`, `ReadLedger`, `RemoveEntry` and the catalog, mapping `Kind`↔`EntryKind`, adding `IsWeekend` and `IsToday` from `TimeProvider`, and building the labels `mensalmente`, `semanalmente`, `diariamente`, `parcela 2/10`; `AddLedgerModule()` registers storage, use cases and store as scoped and runs the seed; `Program.cs` calls it; `Home.razor` and `DayDrawer.razor` use `EntryId`; `Mockup/` and `tests/Gibeira.Web.Tests/Mockup/` are deleted; page tests build the real store through `AddLedgerModule()` with a test helper.
**Where**: `src/Host/Gibeira.Web/Ledger/ModuleLedgerStore.cs`
**Depends on**: None (runs after Phase 3)
**Reuses**: `InMemoryLedgerStoreTests` assertions, moved to `tests/Gibeira.Web.Tests/Ledger/ModuleLedgerStoreTests.cs`; `FixedTime`
**Requirement**: MOD-03, MOD-04, MOD-05, MOD-06, MOD-07, MOD-08

**Done when**:

- [ ] `MockupFoldersAreGone` (RepositoryTests) passes
- [ ] `PageStoreIsTheModuleStore` passes: a scope from `AddLedgerModule()` resolves `ILedgerStore` as `ModuleLedgerStore`
- [ ] `NewScopeStartsFromTheSeed` passes: an item added in one scope is absent from the next
- [ ] `StoreBalanceCarriesPreviousDayAcrossMonths`, `RemoveDropsOnlyThatEntry`, `DayListsEntriesInAddedOrder` and `DayEntriesCarryRepetitionLabels` pass
- [ ] Every other Web.Tests and IntegrationTests test passes unchanged, including `ReloadDiscardsChanges`
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [ ] Test count: the Mockup tests' cases are gone from Web.Tests and present in Ledger.Tests or `ModuleLedgerStoreTests` (no silent deletions)

**Tests**: unit
**Gate**: full

**Commit**: `feat(web): read the ledger from the Ledger module and delete the mockup`

---

### T18: Add form card and split rules

**What**: `AddItemForm.CanSubmit` is false on cartão with no card (MDL-02) and while `Money.CanSplit` is false for parcelado (INS-06); `Submit` passes `Card?.Id`; `AmountInput.Max` reads `Money.Max`.
**Where**: `src/Host/Gibeira.Web/Ledger/AddItemForm.cs`
**Depends on**: T17
**Reuses**: `Money.Max`, `Money.CanSplit` from T4
**Requirement**: MDL-02, INS-06

**Done when**:

- [ ] `CreditCardWithoutCardCannotSubmit` and `CreditCardWithCardCanSubmit` pass
- [ ] `InstallmentBelowOneCentPerPartCannotSubmit` passes: R$ 0,01 in 2x cannot, R$ 0,02 in 2x can
- [ ] `AmountMaxIsMoneyMax` passes
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`
- [ ] Test count: Web.Tests grows by 4 (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): require a card for cartão and a cent per part`

---

### T19: Card dropdown without sem cartão

**What**: The modal's card list drops `sem cartão`; choosing `cartão` opens it; `salvar` in the vencimento popover enables the add button when the amount allows; dismissing the list or the popover with no card keeps it disabled. Spec 002's `CreditCardOpensCardDropdown` and `SemCartaoOpensNoVencimento` (renamed in T2) are replaced, not kept.
**Where**: `src/Host/Gibeira.Web/Components/Home/AddItemModal.razor`
**Depends on**: T18
**Reuses**: The existing card dropdown and vencimento popover
**Requirement**: MDL-01, MDL-02, MDL-03, MDL-04

**Done when**:

- [ ] `CreditCardOpensCardDropdownWithoutNoCard` passes: header `cartão`, disabled add-card button, `Itaú`, `Santander`, the current one checked, no `sem cartão`
- [ ] `CreditCardWithNoCardDisablesAdd` and `SavingDueDateEnablesAdd` pass
- [ ] `DismissingWithNoCardKeepsAddDisabled` passes for Esc on the card list and on the vencimento popover
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`
- [ ] Test count: Web.Tests grows by 4 and loses the two replaced tests (no silent deletions)

**Tests**: unit
**Gate**: quick

**Commit**: `feat(web): drop sem cartão from the card dropdown`

---

### T20: Drawer `+` on cartão opens the card list

**What**: When the modal starts on cartão, it opens with the card list open.
**Where**: `src/Host/Gibeira.Web/Components/Home/AddItemModal.razor`
**Depends on**: T19
**Reuses**: `DrawerPlusOpensModalWithDayAndKind` setup
**Requirement**: MDL-05

**Done when**:

- [ ] `DrawerPlusOnCreditCardOpensCardList` passes, through the page: drawer filtered `cartão`, `+`, the card list is open
- [ ] `DrawerPlusOpensModalWithDayAndKind` and `TopBarAddOpensModalWithIncomeAndToday` still pass
- [ ] Gate check passes: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`
- [ ] Test count: Web.Tests grows by 1 (no silent deletions)

**Tests**: unit
**Gate**: build

**Commit**: `feat(web): open the card list when the modal starts on cartão`

---

## Checks

**C1** - A LedgerEntry reads back its id, day, type, amount, description, card id on cartão, tags and origin (ENT-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*EntryReadsBackComplete"`

**C2** - Two LedgerEntries with equal values and different ids are not equal; an entry equals itself (ENT-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*EntriesAreEqualOnlyByIdentity"`

**C3** - LedgerEntry has no public setter, mutable field or mutating method (ENT-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*EntryHasNoMutatingMember"`

**C4** - An amount of 0 or -1 is refused (ENT-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*MoneyRefusesZeroAndNegative"`

**C5** - 999.999.999,99 is accepted and 1.000.000.000,00 is refused (ENT-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*MoneyRefusesAboveMax"`

**C6** - An amount of 10,005 is refused (ENT-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*MoneyRefusesThreeDecimals"`

**C7** - An empty or all-spaces description is refused (ENT-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*BlankDescriptionIsRefused"`

**C8** - Cartão with no card is refused (ENT-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*CreditCardWithoutCardIsRefused"`

**C9** - Entrada, saída, diário and economia with a card are each refused (ENT-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*CardOnOtherKindIsRefused"`

**C10** - An empty or all-spaces tag is refused (ENT-10, AC 10)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*BlankTagIsRefused"`

**C11** - The same tag given twice is refused (ENT-11, AC 11)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*RepeatedTagIsRefused"`

**C12** - Tags `lazer`, `casa`, `mercado` read back in that order (ENT-12, AC 12)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*TagsKeepTheirGivenOrder"`

**C13** - An entrada of R$ 100,00 has a balance effect of +100,00 (ENT-13, AC 13)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*IncomeAddsToBalance"`

**C14** - Saída, diário, economia and cartão of R$ 100,00 each have a balance effect of -100,00 (ENT-14, AC 14)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*OtherKindsSubtractFromBalance"`

**C15** - Adding a `mensalmente`, `semanalmente` or `diariamente` item stores one rule and no entry (REP-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*AddingRecurringStoresOneRule"`

**C16** - Reading 14 months ahead of a monthly rule creates one entry per month up to the last date read (REP-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*ReadingCreatesMissingEntriesUpToLastDate"`

**C17** - A rule's first entry falls on its first day (REP-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*FirstEntryFallsOnFirstDay"`

**C18** - A monthly rule from 31/01 falls on 31/01, 28/02, 31/03 (REP-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*MonthlyFromDay31ClampsWithoutDrift"`

**C19** - A weekly rule's entries are 7 days apart (REP-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*WeeklyStepsSevenDays"`

**C20** - A daily rule's entries are 1 day apart (REP-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*DailyStepsOneDay"`

**C21** - A generated entry holds the rule's kind, amount, description, card and tags and a `Repeat(ruleId, frequency)` origin (REP-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*GeneratedEntryCopiesRuleDetailsAndOrigin"`

**C22** - Reading the same date twice leaves one entry per day (REP-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*ReadingTheSameDateTwiceCreatesNoDuplicate"`

**C23** - A removed rule entry is absent after later reads (REP-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*RemovedRuleEntryNeverReturns"`

**C24** - A rule whose every entry so far is removed still creates later days (REP-09, edge case)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*RuleKeepsCreatingAfterEveryEntryIsRemoved"`

**C25** - A rule with a blank description stores no rule and no entry (REP-10, AC 10)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*InvalidRuleStoresNothing"`

**C26** - Reading a date before a rule's first day creates no entry for it (REP-11, AC 11)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*ReadingBeforeFirstDayCreatesNothing"`

**C27** - `parcelado em 3x` from 31/01 stores 3 entries on 31/01, 28/02, 31/03 (INS-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*PlanCreatesOneEntryPerMonthClamped"`

**C28** - R$ 100,00 in 3x gives R$ 33,34, R$ 33,33, R$ 33,33 (INS-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*PlanOfOneHundredInThreeSplitsWithRemainderOnFirst"`

**C29** - The parts share one plan id and are numbered 1 to N of N (INS-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*PartsShareOnePlanIdAndNumberOneToN"`

**C30** - 1 and 49 parts are refused and store nothing (INS-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*PartCountOutOfRangeStoresNothing"`

**C31** - R$ 0,01 in 2x is refused and stores nothing (INS-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*TooSmallInstallmentStoresNothing"`

**C32** - The form cannot submit R$ 0,01 in 2x and can submit R$ 0,02 in 2x (INS-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*InstallmentBelowOneCentPerPartCannotSubmit"`

**C33** - Adding a `não repete` item stores one entry with a single origin (ADDT-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*AddingNoRepeatStoresOneSingleEntry"`

**C34** - An item naming a card that does not exist stores nothing (ADDT-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*UnknownCardStoresNothing"`

**C35** - Removing an entry deletes it and no other (DEL-01, AC 3)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*RemovingAnEntryDeletesOnlyIt"`

**C36** - Removing `parcela 2/10` leaves the other nine with their amounts and numbers (DEL-02, AC 4)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*RemovingAPartKeepsOtherPartsAmountsAndNumbers"`

**C37** - Removing an entry already gone changes nothing and throws nothing (DEL-03, AC 5)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*RemovingTwiceReportsNoError"`

**C38** - A monthly rule added before a single item lists first on a later day both fall on (ORD-01, AC 6)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*DayListsEntriesInAddOrderWithRulesInTheirAddPlace"`

**C39** - A Card reads back its id, name and due day (CARD-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*CardReadsBackIdNameAndDueDay"`

**C40** - Due days 0 and 32 are refused (CARD-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*DueDayOutsideMonthIsRefused"`

**C41** - A blank card name is refused (CARD-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*BlankCardNameIsRefused"`

**C42** - Itaú (day 10) from 29/10/2026 gives 10/11/2026, and one month ahead 10/12/2026 (CARD-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*DueDateStepsByMonth"`

**C43** - A card due on day 31 is due on February's last day (CARD-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*DueDayClampsToMonthLength"`

**C44** - Choosing `cartão` opens the list: header `cartão`, disabled add-card button, `Itaú`, `Santander`, current checked, no `sem cartão` (MDL-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*CreditCardOpensCardDropdownWithoutNoCard"`

**C45** - On `cartão` with no card the add button is disabled (MDL-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*CreditCardWithNoCardDisablesAdd"`

**C46** - `salvar` in the vencimento popover enables the add button (MDL-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*SavingDueDateEnablesAdd"`

**C47** - Esc on the card list or the vencimento popover with no card keeps the add button disabled (MDL-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DismissingWithNoCardKeepsAddDisabled"`

**C48** - The drawer's `+` with the filter `cartão` opens the modal with the card list open (MDL-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*DrawerPlusOnCreditCardOpensCardList"`

**C49** - `Gibeira.Ledger.Domain`, `.Application` and `.Infrastructure` exist under `src/Modules/Ledger/` and are in the solution (MOD-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*LedgerModuleHasThreeLayerProjects"`

**C50** - Every `docs/architecture.md` rule holds with the module loaded (MOD-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*HoldsOnSolution"`

**C51** - No `src/Host/Gibeira.Web/Mockup/` and no `tests/Gibeira.Web.Tests/Mockup/` file exists (MOD-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*MockupFoldersAreGone"`

**C52** - The page's `ILedgerStore` resolves to `ModuleLedgerStore` (MOD-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*PageStoreIsTheModuleStore"`

**C53** - The seed, added through `AddItem`, has every kind non-zero this month and a negative balance within 12 months (MOD-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.Ledger.Tests/Gibeira.Ledger.Tests.csproj --filter-method "*SeedHasEveryKindThisMonthAndANegativeBalance"`

**C54** - An item added before a reload is gone after it (MOD-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*ReloadDiscardsChanges"`

**C55** - The page's balance carries the previous day across months from the stored entries (MOD-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj --filter-method "*StoreBalanceCarriesPreviousDayAcrossMonths"`

**C56** - Spec 002's page tests pass on the module, except the replaced ADD-13 and ADD-17 (MOD-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.Web.Tests/Gibeira.Web.Tests.csproj`

**C57** - A Portuguese identifier in a `.cs`, `.razor` or `.css` file is reported with file and line (MOD-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*PortugueseIdentifierIsReportedWithFileAndLine"`

**C58** - `src/` and `tests/` hold no Portuguese identifier (MOD-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*CodeHasNoPortugueseIdentifiers"`

---

## Handoff

The feature's live state, written as it happens - a compaction gives no warning.

**Settled mid-build:** none
**T2 notes:** AD-011 covers every identifier, so T2 also renamed Portuguese names outside the naming table (`Excluir`, `Nenhum`, `Salvar`, `Hoje`, `Todos`, `Padaria`, `Geladeira`, the modal keys `parcelado`→`installments`, `nenhuma`→`none`). Spec 002's `tasks.md` proof filters now name the renamed tests. Running the full solution once had all 17 integration tests fail on Aspire's orchestrator startup check (timeout) while RepositoryTests ran; run alone they pass
**T3 notes:** the T3 gate ended with MTP exit code 8 only because `Gibeira.Ledger.Tests` held no test yet (326 passed, 0 failed); T4 adds its first tests. The template was installed with `dotnet new install ./templates/module` as `CONTRIBUTING.md` says
**Dispatched:** none
**Walked:** none

---

## Phase Execution Map

```
Phase 1 → Phase 2 → Phase 3 → Phase 4

Phase 1:  T1 → T2
Phase 2:  T3 → T4 → T5 → T6 → T7 → T8 → T9
Phase 3:  T10 → T11 → T12 → T13 → T14 → T15 → T16
Phase 4:  T17 → T18 → T19 → T20
```

Execution is strictly sequential. 20 tasks pack into three batches of about seven (Phases 1–2, 3, 4), so Execute starts with the offer to run them as batch sub-agents.
