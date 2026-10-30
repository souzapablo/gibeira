# Home Page Specification

**Approved**: 2026-10-30 - user: "approved"

## Problem Statement

Gibeira has no screen yet: `/` shows the Blazor template heading. The author wants a home page made of a multi-month day-by-day ledger, a day drawer and an "adicionar" modal, drawn in Gibeira's own look from the design canvas. No backend exists, so the page runs on mocked data. The tokens and components built for it must be reusable by the pages that come next.

## Goals

- [ ] `/` shows the 12-month ledger, the day drawer and the add modal described below, in the canvas look.
- [ ] Every color, font, radius, shadow and spacing value lives in one tokens file; a test fails on any color literal elsewhere.
- [ ] Each component in the component table renders on its own in a test, with no dependency on the home page or the mock data.
- [ ] Adding or deleting an item updates the tables, saldos and drawer in the same session, from an in-memory mock store.

## Out of Scope

| Feature | Reason |
| ------- | ------ |
| Backend, database, persistence across reloads | User: "frontend only with mocked data" |
| Editing an item | User chose "Add updates in memory": "Editing comes in a later feature" |
| Managing cards (the add-card icon) and managing tags | Not in the request; cards and tags are fixed mock lists |
| The canvas left panel (saldo hoje, month summary, "vem por aí") and view tabs (totais, horizonte, tags) | User chose the ledger layout plus the canvas top bar only (AD-004) |
| Distinguishing past from projected values | The chosen layout does not draw it |
| Light theme | The canvas is dark only |
| Restyling `Error`, `NotFound` and the reconnect modal beyond dropping Bootstrap | Not part of the home page |
| Languages other than pt-BR | The wording is pt-BR |

---

## Assumptions & Open Questions

| Assumption / decision | Chosen default | Rationale | Confirmed? |
| --------------------- | -------------- | --------- | ---------- |
| Visual style | Canvas palette; saldo cell tinted green/coral | User chose "Canvas palette, tinted saldo (Recommended)" (AD-003) | y |
| Wording | Every label exactly as written in this spec (dia, entradas, saídas, diários, economias, cartão, saldos, adicionar, repetir, vencimento, ...) | User chose these labels over the canvas vocabulary (cofrinho, bolsinho, colocar na gibeira) | y |
| Layout | Period bar, month tables, drawer and modal, plus the canvas top bar and today highlight | User chose the ledger layout plus the canvas top bar (AD-004) | y |
| Mock interactivity | Add writes to an in-memory store; ⋮ offers only "excluir"; both reset on reload | User chose "Add updates in memory (Recommended)" | y |
| Where tokens and components live | `src/Host/Gibeira.Web`: tokens in `wwwroot/css/tokens.css`, components in `Components/UI/` | Only one web app consumes them; a Razor class library can be split out when a second consumer appears | n |
| Mock store location | `Gibeira.Web`, behind an `ILedgerStore` interface, one instance per browser tab (circuit) | A finance module replaces it later without touching the page | n |
| "Today" | The machine's local date | The period and the today highlight follow the real calendar | n |
| Period dropdown | Lists the 25 periods starting 12 months before to 12 months after the current month | Centers the list on the current period, one year each way | n |
| Cell click | A money cell opens the drawer for that day filtered by the cell's kind; a dia cell opens it with "todos" | A money cell belongs to one kind, so the drawer opens on that kind | n |
| Cartão items are booked on the card's due date | The vencimento chosen in the popover is the item's date; the saldo drops on that day | A card bill leaves the account on its due date | n |
| Mock cards | Itaú, due day 10; Santander, due day 6 | Two due days exercise the vencimento popover | n |
| Data popover | Same stepper popover as vencimento: header "data", `<` date `>` stepping one day, "salvar" | One reusable component for both date pickers | n |
| Parcelado | Opens a stepper popover "parcelas" (2 to 48); the amount splits into N monthly items, the cent remainder on the first; drawer label `parcela 2/10` | `parcelado` "divide o valor pelo número de parcelas", which needs a count | n |
| Recurrence end | mensalmente, semanalmente and diariamente repeat with no end date | Keeps the modal at six rows; an end date can come later | n |
| "excluir" on a recurring item | Removes only the occurrence on the drawer's day, with no confirmation | Mock data resets on reload; a confirmation and "all occurrences" arrive with real persistence | n |
| Empty drawer text | `nenhum lançamento` | Short pt-BR text in the app's wording | n |
| Fonts | Bricolage Grotesque and Instrument Sans served from `wwwroot`, no Google Fonts request | The app is self-hosted and local; both fonts are OFL | n |
| Icons | Inline SVG `Icon` component; the kind badges show an arrow in for entrada, an arrow out for saída and the letters D, E, C, in canvas colors | No icon font dependency | n |
| Bootstrap | Removed from the app | The canvas tokens replace it | n |
| Supported widths | 1290×902 down to 360 px wide | The canvas pages work at phone width | n |
| Existing integration test `WebHomePageReturnsGibeira` | Rewritten to assert the new home page (period label) instead of `<h1>Gibeira</h1>` | The template heading goes away | n |
| Test tools | bUnit for components and page behavior; Playwright for the running pictures | Settled in Design | n |

**Open questions:** none - the defaults marked `n` are for the user to correct before approval.

---

## Tokens and components

The canvas values below bind. Token names are the builder's call; each value has exactly one token.

| Group | Values |
| ----- | ------ |
| Grounds | page #15130f, backdrop #0c0b09, surface #1d1a15, raised #25211b, popover #2a261f, weekend #1a1813, today #2a2316, selected #241f15 |
| Lines | default #2c2820, strong #3a342a, hover #4a4336, row #26221c |
| Text | primary #f3ede2, muted #b9b0a1, subtle #958d7e, zero #6f6758, on-accent #1a1408, negative #ef8f79 |
| Accent | #E8B04B, hover #f3c76f |
| Kinds | entrada #E8B04B, saída #DF6A51, diário #D65DB1, economia #37A675, cartão #8C7CF0 |
| Fonts | display Bricolage Grotesque 500–800, body Instrument Sans 400–700 |
| Radii | pill 999px, modal 24px, card 20px, panel 16px, option 14px, input 12px |
| Shadows | modal `0 30px 80px rgba(0,0,0,.6)`, popover `0 12px 32px rgba(0,0,0,.45)` |
| Targets | controls at least 44 px high |

| Component | Used here for |
| --------- | ------------- |
| `Icon` | every glyph |
| `KindIcon` | the colored badge of entrada, saída, diário, economia, cartão |
| `Money` | `R$ 1.189,27` and `R$ -477,67` formatting |
| `Button` | primary, secondary, light ("salvar") and kind-colored ("adicionar cartão") variants |
| `IconButton` | `<`, `>`, `<<`, `>>`, X, +, ⋮ |
| `Dropdown` | header with optional action, options with optional description, check on the selected, single or multiple choice |
| `StepperPopover` | header, `<` value `>`, "salvar" |
| `Modal` | the "adicionar" dialog |
| `Drawer` | the day panel |
| `FieldRow` | icon, label, value, chevron rows of the modal |
| `TopBar` | logo and "adicionar" |

---

## User Stories

### P1: Tokens and reusable components ⭐ MVP

**User Story**: As the author, I want one tokens file and a set of standalone components so that the next pages reuse the look without copying styles.

**Why P1**: The request asks for them; every other story is built from them.

**Acceptance Criteria**:

1. The system SHALL define every value of the token table as a CSS custom property in `src/Host/Gibeira.Web/wwwroot/css/tokens.css`, each value exactly once.  <!-- UI-01 -->
2. IF a `.razor` or `.css` file of `Gibeira.Web`, other than `tokens.css` and files under `wwwroot/lib`, contains a color literal (`#rgb`, `#rrggbb`, `#rrggbbaa`, `rgb(`, `rgba(`, `hsl(`) THEN the token test SHALL fail and name the file and line.  <!-- UI-02 -->
3. The system SHALL render each component of the component table from its parameters alone, without the home page, the mock store or any injected service.  <!-- UI-03 -->
4. The system SHALL serve Bricolage Grotesque and Instrument Sans from `wwwroot`, and the rendered page SHALL reference no font, script or stylesheet on another host.  <!-- UI-04 -->
5. The system SHALL not load Bootstrap.  <!-- UI-05 -->
6. The system SHALL give every icon-only control an `aria-label` in pt-BR and every modal, drawer and popover `role="dialog"`.  <!-- UI-06 -->

**Independent Test**: A test renders each component alone; the token test scans the web project for color literals.

---

### P1: Twelve-month ledger ⭐ MVP

**User Story**: As the author, I want to see each day's entradas, saídas, diários, economias, cartão and saldo across twelve months so that I see when the money runs out.

**Why P1**: It is the home page.

**Acceptance Criteria**:

1. WHEN a user opens `/` THEN the system SHALL show the top bar, the period bar (`<<`, `<`, the period pill with a chevron, `>`, `>>`, centered) and the month tables side by side, in the canvas look of AD-003 and with the top bar of AD-004.  <!-- HOME-01 -->
2. WHEN a user opens `/` THEN the system SHALL show the period of 12 months starting at the current month, labeled `<mmm>/<yyyy> - <mmm>/<yyyy>` with pt-BR month abbreviations (`out/2026 - set/2027` when today is 2026-10-29).  <!-- HOME-02 -->
3. WHEN the user uses `<` or `>` in the period bar THEN the system SHALL move the period one month earlier or later.  <!-- HOME-03 -->
4. WHEN the user uses `<<` or `>>` in the period bar THEN the system SHALL move the period 12 months earlier or later.  <!-- HOME-04 -->
5. WHEN the user uses the period pill THEN the system SHALL open a dropdown of the 25 periods starting 12 months before to 12 months after the current month, with the shown period checked.  <!-- HOME-05 -->
6. WHEN the user chooses a period in that dropdown THEN the system SHALL show that period and close the dropdown.  <!-- HOME-06 -->
7. The system SHALL show one table per month of the period, in month order, titled `<mês> de <yyyy>` in lowercase pt-BR (`dezembro de 2026`), with the columns dia, entradas, saídas, diários, economias, cartão, saldos and one row per calendar day.  <!-- HOME-07 -->
8. The system SHALL show each money cell as its kind icon and the day's sum for that kind formatted `R$ 1.189,27`.  <!-- HOME-08 -->
9. IF a day's sum for a kind is zero THEN the system SHALL show its icon and `R$ 0,00` in the zero text color.  <!-- HOME-09 -->
10. The system SHALL compute each day's saldo as the previous day's saldo plus entradas minus saídas, diários, economias and cartão, starting from the mock opening balance and carried across months.  <!-- HOME-10 -->
11. The system SHALL format a negative amount as `R$ -477,67`.  <!-- HOME-11 -->
12. WHILE a day's saldo is zero or more the system SHALL tint its saldo cell with the positive tint, and WHILE it is below zero with the negative tint (AD-003).  <!-- HOME-12 -->
13. The system SHALL end each table with a total row holding each kind column's month sum with its icon, and an empty saldos cell.  <!-- HOME-13 -->
14. The system SHALL tint the dia cell of Saturdays and Sundays with the weekend ground.  <!-- HOME-14 -->
15. The system SHALL mark today's row with the today ground and today's day number in the accent color (AD-004).  <!-- HOME-15 -->
16. WHILE the day rows are taller than the window the system SHALL keep each table's header row and total row in view while the day rows scroll.  <!-- HOME-16 -->
17. WHILE the month tables are wider than the window the system SHALL scroll them horizontally inside the ledger area, with no horizontal scroll on the page, down to 360 px wide.  <!-- HOME-17 -->
18. WHEN the user uses "adicionar" in the top bar THEN the system SHALL open the add modal with type entrada and today's date.  <!-- HOME-18 -->

**Independent Test**: Open `/`, read today's period label, step it with `<`, `>`, `<<`, `>>`, and check one saldo against the mock data by hand.

---

### P1: Day drawer ⭐ MVP

**User Story**: As the author, I want to open a day and see the items behind a number so that I know what makes up each cell.

**Why P1**: The ledger shows sums only; the drawer is how the user sees and removes items.

**Acceptance Criteria**:

1. WHEN the user uses a money cell THEN the system SHALL open the drawer for that day, filtered by that cell's kind: a panel on the right over the ledger, with X, `<` title `>` and + in its header and the filter pill below it.  <!-- DAY-01 -->
2. WHEN the user uses a dia cell THEN the system SHALL open the drawer for that day with the filter `todos`.  <!-- DAY-02 -->
3. The system SHALL title the drawer `<d>/<mmm>` (`10/nov`).  <!-- DAY-03 -->
4. The system SHALL list each item of the day that matches the filter, in the order it was added, with its kind icon, description (cut with `…` when it does not fit), amount, date `<dd>/<mm>`, ⋮, and its recurrence label when it repeats: `mensalmente`, `semanalmente`, `diariamente` or `parcela <n>/<N>`.  <!-- DAY-04 -->
5. WHEN the user uses `<` or `>` in the drawer THEN the system SHALL show the previous or next day with the same filter.  <!-- DAY-05 -->
6. WHEN the user uses the filter pill THEN the system SHALL open a dropdown with `todos`, `entradas`, `saídas`, `diários`, `economias`, `cartão`, the current one checked.  <!-- DAY-06 -->
7. WHEN the user chooses a filter THEN the system SHALL list only that kind's items, or all items for `todos`.  <!-- DAY-07 -->
8. IF no item of the day matches the filter THEN the system SHALL show `nenhum lançamento`.  <!-- DAY-08 -->
9. WHEN the user uses X or presses Esc in the drawer THEN the system SHALL close the drawer.  <!-- DAY-09 -->
10. WHEN the user uses + in the drawer THEN the system SHALL open the add modal with the drawer's day as date and the filter's kind as type, or entrada when the filter is `todos`.  <!-- DAY-10 -->
11. WHEN the user uses ⋮ on an item THEN the system SHALL open a menu with `excluir`.  <!-- DAY-11 -->
12. WHEN the user chooses `excluir` THEN the system SHALL remove that item's occurrence on that day from the mock store, and the drawer, the cell, the totals and every later saldo SHALL update.  <!-- DAY-12 -->

**Independent Test**: Click a cartão cell, see its items, switch the filter, step days, delete one item and watch the saldo column change.

---

### P1: Add modal ⭐ MVP

**User Story**: As the author, I want to add an entrada, saída, diário, economia or cartão item, once or repeating, so that the ledger projects it.

**Why P1**: Without it the mock data never changes.

**Acceptance Criteria**:

1. WHEN the add modal opens THEN the system SHALL show title `adicionar`, X, the rows R$ amount (placeholder `0,00`), type (kind icon and name), description (placeholder the type's name), data (date `dd/mm/yy`), repetition (`não repete`) and tags (`tags`), each with its icon and the last four with a chevron, and the button `adicionar entrada`, with the amount field focused.  <!-- ADD-01 -->
2. The system SHALL accept in the amount field digits and one comma with at most two digits after it, up to `999.999.999,99`, and ignore any other character typed.  <!-- ADD-02 -->
3. WHEN the user uses the type row THEN the system SHALL open a dropdown with `entrada`, `saída`, `diário`, `economia`, `cartão`, the current one checked.  <!-- ADD-03 -->
4. WHEN the user chooses a type THEN the system SHALL show its icon and name on the row, use its name as the description placeholder, and label the button `adicionar <type>` in that kind's color.  <!-- ADD-04 -->
5. WHEN the user uses the data row THEN the system SHALL open a popover `data` showing the date `dd/mm/yyyy` between `<` and `>`, each stepping one day, and `salvar`.  <!-- ADD-05 -->
6. WHEN the user uses `salvar` in the data popover THEN the system SHALL set the date, show it on the row as `dd/mm/yy` and close the popover.  <!-- ADD-06 -->
7. WHEN the user uses the repetition row THEN the system SHALL open a dropdown with header `repetir`, `mensalmente` ("repete o mesmo valor mensalmente"), `semanalmente` ("repete o mesmo valor semanalmente"), `diariamente` ("repete o mesmo valor diariamente"), `parcelado` ("divide o valor pelo número de parcelas") and `não repete`, the current one checked.  <!-- ADD-07 -->
8. WHEN the user chooses `mensalmente`, `semanalmente`, `diariamente` or `não repete` THEN the system SHALL show it on the row.  <!-- ADD-08 -->
9. WHEN the user chooses `parcelado` THEN the system SHALL open a popover `parcelas` with a count from 2 to 48 between `<` and `>` (starting at 2) and `salvar`, which shows `parcelado em <N>x` on the row.  <!-- ADD-09 -->
10. WHEN an item set to `mensalmente` is added THEN the system SHALL repeat it on the same day of every later month, on the month's last day when that day does not exist.  <!-- ADD-10 -->
11. WHEN an item set to `semanalmente` or `diariamente` is added THEN the system SHALL repeat it every 7 days or every day with no end.  <!-- ADD-11 -->
12. WHEN an item set to `parcelado em <N>x` is added THEN the system SHALL create N monthly occurrences of the amount divided by N, the cent remainder on the first.  <!-- ADD-12 -->
13. WHEN the user chooses the type `cartão` THEN the system SHALL open the card dropdown: header `cartão` with a disabled add-card button, `Itaú`, `Santander`, `sem cartão`, the current one checked.  <!-- ADD-13 -->
14. WHEN the user chooses a card THEN the system SHALL open the popover `vencimento` showing that card's first due date on or after the form's date as `dd/mm/yyyy` between `<` and `>`, and `salvar`.  <!-- ADD-14 -->
15. WHEN the user uses `<` or `>` in the vencimento popover THEN the system SHALL show the due date one month earlier or later (`10/11/2026` to `10/12/2026` for Itaú).  <!-- ADD-15 -->
16. WHEN the user uses `salvar` in the vencimento popover THEN the system SHALL set the item's date to that due date, label the data row `vencimento` with that date, and close the popover.  <!-- ADD-16 -->
17. WHEN the user chooses `sem cartão` THEN the system SHALL keep the data row labeled `data` and open no vencimento popover.  <!-- ADD-17 -->
18. WHEN the user changes the type from `cartão` to another type THEN the system SHALL clear the card and label the data row `data` again.  <!-- ADD-18 -->
19. WHEN the user uses the tags row THEN the system SHALL open a dropdown of the mock tags where several can be checked, and show the checked names joined by `, ` on the row, or `tags` when none.  <!-- ADD-19 -->
20. WHILE the amount is zero or empty the system SHALL disable the add button.  <!-- ADD-20 -->
21. WHEN the user uses the add button THEN the system SHALL add the item to the mock store, close the modal, and update the tables, totals, saldos and an open drawer.  <!-- ADD-21 -->
22. IF the description is empty when the item is added THEN the system SHALL save the type's name as the description.  <!-- ADD-22 -->
23. IF the user uses the add button twice before the modal closes THEN the system SHALL add the item once.  <!-- ADD-23 -->
24. WHEN the user uses X, presses Esc or clicks the backdrop THEN the system SHALL close the modal and add nothing.  <!-- ADD-24 -->
25. WHILE a dropdown or popover is open, WHEN the user presses Esc or clicks outside it the system SHALL close it and keep the previous value.  <!-- ADD-25 -->

**Independent Test**: Add a monthly saída of R$ 100,00 and see it in every later month's saídas and saldos; add a cartão item on Itaú and see it on the due date.

---

### P1: Mock data ⭐ MVP

**User Story**: As the author, I want realistic mock data so that the page looks like real use before a backend exists.

**Why P1**: Every other story reads it.

**Acceptance Criteria**:

1. WHEN the page loads THEN the system SHALL seed the mock store with the same items on every load: an opening balance, a monthly entrada, a daily diário, monthly saídas, a monthly economia and cartão items on both cards, so that every kind has a non-zero day in the current month and at least one saldo is negative within the first 12 months.  <!-- MOCK-01 -->
2. WHEN the page reloads THEN the system SHALL discard every item added or deleted since the last load.  <!-- MOCK-02 -->
3. The system SHALL give the page its data only through the `ILedgerStore` interface.  <!-- MOCK-03 -->

**Independent Test**: Reload twice and compare the first table; add an item, reload, and see it gone.

---

### P1: Screens at each width ⭐ MVP

**User Story**: As the author, I want the page to hold at desktop and phone widths so that it works on any screen I own.

**Why P1**: The canvas look is fluid; a broken narrow layout is a broken page.

**Acceptance Criteria**:

1. WHILE the window is narrower than 432 px the system SHALL show the modal at full width minus a 16 px gutter on each side.  <!-- SCR-01 -->
2. WHILE the window is narrower than 432 px the system SHALL show the drawer at the full window width.  <!-- SCR-02 -->

**Independent Test**: Open `/` at 360 px, open the drawer and the modal, and check nothing overflows the window.

---

## Edge Cases

- IF a period contains a day whose items are all deleted THEN the system SHALL show `R$ 0,00` for those cells and recompute later saldos.  <!-- covered by DAY-12, HOME-09 -->
- WHEN `mensalmente` starts on day 31 THEN February SHALL get the item on its last day.  <!-- ADD-10 -->
- IF the parcel count would leave a remainder THEN the first parcel SHALL carry it so the parcels sum to the amount.  <!-- ADD-12 -->

**Implicit dimensions:** input bounds (ADD-02, ADD-09, ADD-20); duplicate submit (ADD-23); data lifecycle (MOCK-02); state transitions (ADD-18, ADD-25). Failure states, external dependencies, auth, concurrency and observability: N/A because the page reads and writes an in-memory store in its own circuit, calls no service and has one local user.

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
| -------------- | ----- | ----- | ------ |
| UI-01 | P1: Tokens and reusable components | Execute | Implemented (T1) |
| UI-02 | P1: Tokens and reusable components | Execute | Implemented (T3) |
| UI-03 | P1: Tokens and reusable components | Design | Pending |
| UI-04 | P1: Tokens and reusable components | Execute | Implemented (T2, T4) |
| UI-05 | P1: Tokens and reusable components | Execute | Implemented (T3) |
| UI-06 | P1: Tokens and reusable components | Design | Pending |
| HOME-01 | P1: Twelve-month ledger | Design | Pending |
| HOME-02 | P1: Twelve-month ledger | Design | Pending |
| HOME-03 | P1: Twelve-month ledger | Design | Pending |
| HOME-04 | P1: Twelve-month ledger | Design | Pending |
| HOME-05 | P1: Twelve-month ledger | Design | Pending |
| HOME-06 | P1: Twelve-month ledger | Design | Pending |
| HOME-07 | P1: Twelve-month ledger | Design | Pending |
| HOME-08 | P1: Twelve-month ledger | Design | Pending |
| HOME-09 | P1: Twelve-month ledger | Design | Pending |
| HOME-10 | P1: Twelve-month ledger | Execute | Implemented (T19) |
| HOME-11 | P1: Twelve-month ledger | Execute | Implemented (T6) |
| HOME-12 | P1: Twelve-month ledger | Design | Pending |
| HOME-13 | P1: Twelve-month ledger | Design | Pending |
| HOME-14 | P1: Twelve-month ledger | Design | Pending |
| HOME-15 | P1: Twelve-month ledger | Design | Pending |
| HOME-16 | P1: Twelve-month ledger | Design | Pending |
| HOME-17 | P1: Twelve-month ledger | Design | Pending |
| HOME-18 | P1: Twelve-month ledger | Design | Pending |
| DAY-01 | P1: Day drawer | Design | Pending |
| DAY-02 | P1: Day drawer | Design | Pending |
| DAY-03 | P1: Day drawer | Design | Pending |
| DAY-04 | P1: Day drawer | Design | Pending |
| DAY-05 | P1: Day drawer | Design | Pending |
| DAY-06 | P1: Day drawer | Design | Pending |
| DAY-07 | P1: Day drawer | Design | Pending |
| DAY-08 | P1: Day drawer | Design | Pending |
| DAY-09 | P1: Day drawer | Design | Pending |
| DAY-10 | P1: Day drawer | Design | Pending |
| DAY-11 | P1: Day drawer | Design | Pending |
| DAY-12 | P1: Day drawer | Design | Pending |
| ADD-01 | P1: Add modal | Design | Pending |
| ADD-02 | P1: Add modal | Design | Pending |
| ADD-03 | P1: Add modal | Design | Pending |
| ADD-04 | P1: Add modal | Design | Pending |
| ADD-05 | P1: Add modal | Design | Pending |
| ADD-06 | P1: Add modal | Design | Pending |
| ADD-07 | P1: Add modal | Design | Pending |
| ADD-08 | P1: Add modal | Design | Pending |
| ADD-09 | P1: Add modal | Design | Pending |
| ADD-10 | P1: Add modal | Execute | Implemented (T18) |
| ADD-11 | P1: Add modal | Execute | Implemented (T18) |
| ADD-12 | P1: Add modal | Execute | Implemented (T18) |
| ADD-13 | P1: Add modal | Design | Pending |
| ADD-14 | P1: Add modal | Design | Pending |
| ADD-15 | P1: Add modal | Design | Pending |
| ADD-16 | P1: Add modal | Design | Pending |
| ADD-17 | P1: Add modal | Design | Pending |
| ADD-18 | P1: Add modal | Design | Pending |
| ADD-19 | P1: Add modal | Design | Pending |
| ADD-20 | P1: Add modal | Design | Pending |
| ADD-21 | P1: Add modal | Design | Pending |
| ADD-22 | P1: Add modal | Execute | Implemented (T17) |
| ADD-23 | P1: Add modal | Design | Pending |
| ADD-24 | P1: Add modal | Design | Pending |
| ADD-25 | P1: Add modal | Design | Pending |
| MOCK-01 | P1: Mock data | Design | Pending |
| MOCK-02 | P1: Mock data | Design | Pending |
| MOCK-03 | P1: Mock data | Design | Pending |
| SCR-01 | P1: Screens at each width | Design | Pending |
| SCR-02 | P1: Screens at each width | Design | Pending |

**Coverage:** 66 total, 0 mapped to tasks, 66 unmapped ⚠️

---

## Differences from the source

| Source shows | Product shows | Confirmed |
| ------------ | ------------- | --------- |
| Canvas left panel (saldo hoje, month summary, "vem por aí") and view tabs (totais, horizonte, tags) | Top bar, period bar and month tables only | y - AD-004 |
| Canvas vocabulary (cofrinho, bolsinho, colocar na gibeira) | The labels written in this spec (entradas, saídas, diários, economias, cartão, saldos, adicionar, ...) | y - AD-005 |

## Sources

- [Gibeira — saldos design canvas](https://claude.ai/artifact/WEboiPZKbDSccEKkXZhG8m) - binding for the look (token table above) - user: "using the style from this"

---

## Success Criteria

- [ ] The running page at 1290×902 and at 360 px wide, with the drawer, the modal and each dropdown and popover open, is judged by the screens sub-agent to match the requirements and the canvas look.
- [ ] `dotnet build` has 0 warnings and `dotnet test` passes, including the token and component tests.
