# Ledger Entry Specification

**Approved**: 2026-11-05 - user: "approved"

## Problem Statement

The home page runs on a mockup whose "item" is a rule that expands into occurrences: it has no entity that says what happened on a day. Gibeira needs a real LedgerEntry: one complete record of one day's money movement, holding its day, type, amount and everything else about it. This spec designs that entity with the tactical DDD building blocks that earn their place, puts it in the first finance module with in-memory storage, and retires the mockup as AD-007 planned.

## Goals

- [ ] A LedgerEntry holds all its information and refuses to exist in an invalid state, proven by domain tests with no page and no storage.
- [ ] A repetition with no end produces one complete LedgerEntry per day it falls on, for any date the page asks about, and a removed day never returns.
- [ ] `src/Host/Gibeira.Web/Mockup/` is gone, and the page shows the same ledger from the `Gibeira.Ledger` module.
- [ ] Every cartão LedgerEntry names a card, in the domain and in the add modal.
- [ ] Code is in English, and Portuguese appears only in text users see on screens.

## Out of Scope

| Feature | Reason |
| ------- | ------ |
| Persistence and a database | User: "We don't need to think of presistence right not, we can use in memory or mocked items on the backend" |
| Editing a LedgerEntry | User chose "Add and remove only": those methods arrive with the edit feature |
| Ending a repetition, changing it, or removing all its days at once | User chose "Add and remove only". As on today's page, a repetition cannot be stopped; each day can only be removed one at a time |
| Marking a LedgerEntry as paid, or separating past from projected | Not requested; spec 002 left it out too |
| A purchase date on cartão LedgerEntries, and card bills (faturas) | Not requested; the day stays the due date the user saved, as on today's page |
| Managing cards and tags | Not requested; both stay fixed lists, as in spec 002 |
| Accounts, transfers and other currencies | Not requested; Gibeira has one saldo in R$ |
| Any change to what the page does beyond the card rule | User chose "Module replaces mockup", where the page looks and behaves the same |

---

## Assumptions & Open Questions

Every ambiguity is resolved or recorded here - nothing is left silently unclear.

| Assumption / decision | Chosen default | Rationale | Confirmed? |
| --------------------- | -------------- | --------- | ---------- |
| What a LedgerEntry is | One complete record per day it falls on; a RecurringRule creates more on demand; parcelado creates all its parts at once (AD-008) | User chose "One per day, on demand" after saying "I want the transaction to have all information. The day, type, amount etc" | y |
| How far the spec goes | `Gibeira.Ledger` module with Domain, Application and in-memory Infrastructure; the page reads it; `Mockup/` is deleted | User chose "Module replaces mockup" | y |
| Behaviours | Add and remove one LedgerEntry | User chose "Add and remove only" | y |
| Cartão and card | Cartão requires a card; every other type has none; `sem cartão` leaves the modal (AD-009) | User chose "Cartão requires a card" | y |
| DDD building blocks | Aggregates (LedgerEntry, RecurringRule, Card), value objects, factory methods and repository interfaces. No domain events, because nothing subscribes to them; no base entity classes or mediator | User: "I want ddd, but not necessarly all of it. just what makes sense" (user delegated) | y |
| Entity name | `LedgerEntry` in code, not `Transaction` or `Entry`; screens keep `lançamento` (AD-010) | User: "Yeah, we can go with LedgerEntry". It is the closest match for `lançamento`, and a bare `Entry` reads like the screen's `entrada` | y |
| Code language | English for every identifier, in new and existing code; Portuguese only in text users see on screens (AD-011) | User: "do not use portuguese on the code, only for screens that users will face" | y |
| Module name | `Gibeira.Ledger` | The page already calls this area the ledger, and the module also holds rules, cards and the saldo | n |
| Card | Its own aggregate (name, due day, due date rule) that LedgerEntries reference by id; a fixed in-memory list | A card has an identity of its own; the LedgerEntry stays complete without copying it | n |
| Tags | A value object compared by name, in the order they were checked; a fixed in-memory list | Tags have no behaviour yet | n |
| Money | R$ only, with no currency field | Gibeira has one currency; a field nothing reads adds nothing | n |
| Description | Required and not blank in the domain, with no maximum length; the form keeps filling in the type's name when empty (ADD-22) | The drawer already cuts long text with `…` | n |
| Cartão day | The LedgerEntry does not check that its day matches the card's due day | The modal only offers due dates; checking would tie two aggregates together for a rule the screen already keeps | n |
| Parcelado too small | Refused when the amount gives less than R$ 0,01 per part, and the add button stays disabled | Today R$ 0,01 in 2x creates a R$ 0,00 part, which a LedgerEntry must not have | n |
| Modal opened on cartão | The modal opens with the card list open when the drawer's `+` starts it on cartão, because the add button is disabled until a card is chosen | Otherwise the user faces a disabled button with no sign of why | n |
| Removing a parcela | The other parts keep their amounts and numbers (`parcela 2/10`) | Same as today's page | n |
| Generating and reading | Reading the ledger or a day creates any missing LedgerEntries up to that date, and a daily rule far ahead creates them all | One local user steps at most 12 months per click; the mockup expands from the start in the same way | n |
| Tests | `tests/Gibeira.Ledger.Tests`; tests in `tests/Gibeira.Web.Tests/Mockup/` move there or go with the code they cover | Domain rules are tested with no page | n |

**Open questions:** none - the defaults marked `n` are for the user to correct before approval.

---

## The model

The shape that the criteria below hold. Design settles the code.

```text
LedgerEntry (aggregate root)                RecurringRule (aggregate root)
  Id           LedgerEntryId                  Id              RecurringRuleId
  Date         DateOnly                       FirstDate       DateOnly
  Kind         Income | Expense               Kind, Amount, Description, CardId?, Tags
               | DailySpending | Savings      Frequency       Monthly | Weekly | Daily
               | CreditCard                   GeneratedUntil  moves forward only
  Amount       Money                          GenerateUpTo(date) -> LedgerEntry[]
  Description
  CardId?      CreditCard only, and required
  Tags         Tag[]                        Card (aggregate root)
  Origin       Single                         Id, Name, DueDay
               | Repeat(ruleId, frequency)    DueDate(onOrAfter, monthsAhead)
               | Installment(planId, n, N)
```

Code is in English; the Portuguese words appear only on screens (AD-011). The criteria below use the screen's words.

| Screen | Code |
| ------ | ---- |
| `entrada` | `Income` |
| `saída` | `Expense` |
| `diário` | `DailySpending` |
| `economia` | `Savings` |
| `cartão` | `CreditCard` |
| `saldo` | `Balance` |
| `parcelado`, `parcela` | `Installments`, `Installment` |
| `vencimento` | `DueDate` |
| `lançamento` | `LedgerEntry` |

---

## User Stories

### P1: A LedgerEntry holds all its information ⭐ MVP

**User Story**: As the author, I want a LedgerEntry to hold its day, type, amount and everything else about it so that one record says what happened, with nothing to compute.

**Why P1**: It is what the user asked to design.

**Acceptance Criteria**:

1. The LedgerEntry SHALL expose its id, day, type, amount, description, tags and origin, and its card id when its type is cartão.  <!-- ENT-01 -->
2. The LedgerEntry SHALL be equal to another LedgerEntry only when their ids are equal, whatever their other values.  <!-- ENT-02 -->
3. The LedgerEntry SHALL offer no operation that changes it after it is created.  <!-- ENT-03 -->
4. IF the amount is zero or negative THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-04 -->
5. IF the amount is above 999.999.999,99 THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-05 -->
6. IF the amount has more than two decimal places THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-06 -->
7. IF the description is empty or only spaces THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-07 -->
8. IF the type is cartão and no card is given THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-08 -->
9. IF the type is not cartão and a card is given THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-09 -->
10. IF a tag is empty or only spaces THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-10 -->
11. IF the same tag is given twice THEN the system SHALL refuse to create the LedgerEntry.  <!-- ENT-11 -->
12. The LedgerEntry SHALL keep its tags in the order they were given.  <!-- ENT-12 -->
13. WHEN the type is entrada THEN the LedgerEntry SHALL add its amount to the saldo.  <!-- ENT-13 -->
14. WHEN the type is saída, diário, economia or cartão THEN the LedgerEntry SHALL subtract its amount from the saldo.  <!-- ENT-14 -->

**Independent Test**: Create LedgerEntries in domain tests, with no page and no storage: each invalid input is refused, and each valid one reads back complete.

---

### P1: Repetition with no end ⭐ MVP

**User Story**: As the author, I want an item that repeats to become one complete LedgerEntry per day so that every day holds real records and a removed day stays removed.

**Why P1**: `mensalmente`, `semanalmente` and `diariamente` have no end date (ADD-10, ADD-11), so the records cannot all exist up front.

**Acceptance Criteria**:

1. WHEN an item set to `mensalmente`, `semanalmente` or `diariamente` is added THEN the system SHALL store one RecurringRule for it.  <!-- REP-01 -->
2. WHEN the ledger or a day is read THEN the system SHALL create every missing LedgerEntry of every rule up to the last date read.  <!-- REP-02 -->
3. The system SHALL create a rule's first LedgerEntry on the rule's first day.  <!-- REP-03 -->
4. WHILE a rule is `mensalmente` the system SHALL create its LedgerEntries on the first day's day number of each month, on the month's last day when that number does not exist, with no drift (31/01, 28/02, 31/03).  <!-- REP-04 -->
5. WHILE a rule is `semanalmente` the system SHALL create its LedgerEntries every 7 days.  <!-- REP-05 -->
6. WHILE a rule is `diariamente` the system SHALL create its LedgerEntries every day.  <!-- REP-06 -->
7. WHEN a rule creates a LedgerEntry THEN the LedgerEntry SHALL hold the rule's type, amount, description, card and tags, and an origin naming the rule and its frequency.  <!-- REP-07 -->
8. WHEN the same date is read twice THEN the system SHALL create no LedgerEntry twice.  <!-- REP-08 -->
9. IF a LedgerEntry a rule created is removed THEN later reads SHALL not create it again.  <!-- REP-09 -->
10. IF a rule is given an amount, description, card or tags a LedgerEntry would refuse THEN the system SHALL refuse to create the rule and SHALL store nothing.  <!-- REP-10 -->
11. WHILE a date before a rule's first day is read the system SHALL create no LedgerEntry for that rule.  <!-- REP-11 -->

**Independent Test**: Add a monthly rule on day 31, read 14 months ahead and see one LedgerEntry per month on the right days; remove one, read again and see it stays gone.

---

### P1: Parcelado ⭐ MVP

**User Story**: As the author, I want `parcelado` to create all its LedgerEntries at once so that each part is a complete record with its own amount.

**Why P1**: The page offers `parcelado` today (ADD-09, ADD-12), and its count is known when it is added.

**Acceptance Criteria**:

1. WHEN an item set to `parcelado em <N>x` is added THEN the system SHALL store N LedgerEntries, one per month from the chosen day, on the month's last day when that day number does not exist.  <!-- INS-01 -->
2. The system SHALL give each part the amount divided by N cut to cents, and the first part the remainder, so the parts sum to the amount.  <!-- INS-02 -->
3. The system SHALL give each part an origin holding a plan id shared by its N parts, its number from 1 to N, and N.  <!-- INS-03 -->
4. IF N is below 2 or above 48 THEN the system SHALL refuse the item and SHALL store nothing.  <!-- INS-04 -->
5. IF the amount gives less than R$ 0,01 per part THEN the system SHALL refuse the item and SHALL store nothing.  <!-- INS-05 -->
6. WHILE the amount gives less than R$ 0,01 per part the system SHALL disable the add button of the modal.  <!-- INS-06 -->

**Independent Test**: Add R$ 100,00 in 3x and read three LedgerEntries of R$ 33,34, R$ 33,33 and R$ 33,33, numbered 1 to 3.

---

### P1: Adding and removing ⭐ MVP

**User Story**: As the author, I want to add an item and remove one day's LedgerEntry so that the ledger follows what I do, as it does today.

**Why P1**: These are the only two things the page does to data.

**Acceptance Criteria**:

1. WHEN an item set to `não repete` is added THEN the system SHALL store one LedgerEntry whose origin is single.  <!-- ADDT-01 -->
2. IF an item names a card that does not exist THEN the system SHALL refuse the item and SHALL store nothing.  <!-- ADDT-02 -->
3. WHEN a LedgerEntry is removed THEN the system SHALL delete that LedgerEntry and no other.  <!-- DEL-01 -->
4. WHEN a part of a parcelado is removed THEN the other parts SHALL keep their amounts and numbers.  <!-- DEL-02 -->
5. IF the LedgerEntry being removed no longer exists THEN the system SHALL change nothing and report no error.  <!-- DEL-03 -->
6. The system SHALL list a day's LedgerEntries in the order their items were added, a rule's LedgerEntries taking the place of the rule's add, whenever they were created.  <!-- ORD-01 -->

**Independent Test**: Add a monthly rule, then a single item on a later day the rule also falls on, and read that day: the rule's LedgerEntry comes first. Remove it and only the single item is left.

---

### P1: Card ⭐ MVP

**User Story**: As the author, I want a card to be its own entity with its due date rule so that a LedgerEntry can reference it by id.

**Why P1**: The card rule needs real cards, and the modal asks them for due dates (ADD-14, ADD-15).

**Acceptance Criteria**:

1. The Card SHALL expose its id, name and due day.  <!-- CARD-01 -->
2. IF the due day is below 1 or above 31 THEN the system SHALL refuse to create the Card.  <!-- CARD-02 -->
3. IF the name is empty or only spaces THEN the system SHALL refuse to create the Card.  <!-- CARD-03 -->
4. WHEN a Card is asked for a due date THEN it SHALL return its first due date on or after the given date, moved by the given number of months.  <!-- CARD-04 -->
5. WHILE a month is shorter than the due day the Card SHALL use the month's last day as the due date.  <!-- CARD-05 -->

**Independent Test**: Ask Itaú (day 10) for its due date from 29/10/2026 and from there one month ahead: 10/11/2026, then 10/12/2026.

---

### P1: The add modal requires a card ⭐ MVP

**User Story**: As the author, I want the modal to require a card for cartão so that it can never submit what the domain refuses.

**Why P1**: User chose "Cartão requires a card". This is the one change on the page; it replaces ADD-13 and ADD-17 of spec 002.

**Acceptance Criteria**:

1. WHEN the user chooses the type `cartão` THEN the system SHALL open the card dropdown: header `cartão` with a disabled add-card button, then `Itaú` and `Santander`, the current one checked, and no `sem cartão`.  <!-- MDL-01 -->
2. WHILE the type is `cartão` and no card is chosen the system SHALL disable the add button.  <!-- MDL-02 -->
3. WHEN the user uses `salvar` in the vencimento popover THEN the system SHALL enable the add button if the amount allows it.  <!-- MDL-03 -->
4. IF the user dismisses the card dropdown or the vencimento popover with no card chosen THEN the system SHALL keep the add button disabled.  <!-- MDL-04 -->
5. WHEN the user uses `+` in the drawer with the filter `cartão` THEN the system SHALL open the add modal with the card dropdown open.  <!-- MDL-05 -->

**Independent Test**: Open the modal, type an amount, choose `cartão`, press Esc and see the add button disabled; choose the type again, pick Itaú, save the due date and see the button enabled.

---

### P1: The module replaces the mockup ⭐ MVP

**User Story**: As the author, I want the page to read the new module so that the rules live in one place and the mockup is gone.

**Why P1**: AD-007 says the mockup is deleted when a finance module provides the data, and the user chose this scope.

**Acceptance Criteria**:

1. The system SHALL hold the domain, use cases and in-memory storage in `Gibeira.Ledger.Domain`, `Gibeira.Ledger.Application` and `Gibeira.Ledger.Infrastructure` under `src/Modules/Ledger/`.  <!-- MOD-01 -->
2. The system SHALL satisfy every rule in `docs/architecture.md` with the module in place.  <!-- MOD-02 -->
3. The repository SHALL have no `src/Host/Gibeira.Web/Mockup/` folder and no `tests/Gibeira.Web.Tests/Mockup/` folder.  <!-- MOD-03 -->
4. The system SHALL give the page its data only through `ILedgerStore`, backed by the module.  <!-- MOD-04 -->
5. WHEN the page loads THEN the system SHALL seed the module's storage with the items MOCK-01 requires, through the same add use case the page uses.  <!-- MOD-05 -->
6. WHEN the page reloads THEN the system SHALL discard every item added or removed since the last load.  <!-- MOD-06 -->
7. The system SHALL compute each day's sums by type and saldo from the stored LedgerEntries, with the results HOME-08, HOME-09, HOME-10 and HOME-13 require.  <!-- MOD-07 -->
8. The system SHALL keep passing every test of spec 002's criteria, except ADD-13 and ADD-17, which MDL-01 and MDL-02 replace.  <!-- MOD-08 -->
9. IF a `.cs`, `.razor` or `.css` file under `src/` or `tests/` uses a Portuguese word from this spec's naming table as an identifier THEN the naming test SHALL fail and name the file and line.  <!-- MOD-09 -->

**Independent Test**: Run the page: the ledger, drawer and modal work as before, with the card rule added. Search the repository for `Mockup` and find nothing under `src/`.

---

## Edge Cases

- WHEN `mensalmente` starts on day 31 THEN February SHALL get the LedgerEntry on its last day and March on day 31.  <!-- REP-04 -->
- IF the amount leaves a remainder across parts THEN the first part SHALL carry it.  <!-- INS-02 -->
- IF a rule's every LedgerEntry so far is removed THEN the rule SHALL keep creating later days.  <!-- REP-09 -->
- IF the add button is used twice before the modal closes THEN the system SHALL add the item once.  <!-- ADD-23, kept -->

**Implicit dimensions:**

| Dimension | Resolution |
| --------- | ---------- |
| Input validation and bounds | ENT-04 to ENT-11, INS-04, INS-05, CARD-02, CARD-03 |
| Failure and partial failure | REP-10, INS-04, INS-05 and ADDT-02 store nothing when refused |
| Idempotency, retry and duplicates | REP-08, REP-09, DEL-03, ADD-23 |
| State-transition integrity | ENT-03: a LedgerEntry has no states. REP-08: a rule's generated range only moves forward |
| Data lifecycle | MOD-06: storage lasts one load |
| Auth boundaries and rate limits | N/A because Gibeira has one local user and no login |
| Concurrency and ordering | ORD-01 for order; concurrent writes N/A because each circuit has its own storage and handles one event at a time |
| Observability | N/A because nothing here runs unattended and one local user sees every result on the page |
| External-dependency failure | N/A because the module calls no outside service |

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
| -------------- | ----- | ----- | ------ |
| ENT-01 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-02 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-03 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-04 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-05 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-06 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-07 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-08 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-09 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-10 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-11 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-12 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-13 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| ENT-14 | P1: A LedgerEntry holds all its information | Tasks | In Tasks |
| REP-01 | P1: Repetition with no end | Tasks | In Tasks |
| REP-02 | P1: Repetition with no end | Tasks | In Tasks |
| REP-03 | P1: Repetition with no end | Tasks | In Tasks |
| REP-04 | P1: Repetition with no end | Tasks | In Tasks |
| REP-05 | P1: Repetition with no end | Tasks | In Tasks |
| REP-06 | P1: Repetition with no end | Tasks | In Tasks |
| REP-07 | P1: Repetition with no end | Tasks | In Tasks |
| REP-08 | P1: Repetition with no end | Tasks | In Tasks |
| REP-09 | P1: Repetition with no end | Tasks | In Tasks |
| REP-10 | P1: Repetition with no end | Tasks | In Tasks |
| REP-11 | P1: Repetition with no end | Tasks | In Tasks |
| INS-01 | P1: Parcelado | Tasks | In Tasks |
| INS-02 | P1: Parcelado | Tasks | In Tasks |
| INS-03 | P1: Parcelado | Tasks | In Tasks |
| INS-04 | P1: Parcelado | Tasks | In Tasks |
| INS-05 | P1: Parcelado | Tasks | In Tasks |
| INS-06 | P1: Parcelado | Tasks | In Tasks |
| ADDT-01 | P1: Adding and removing | Tasks | In Tasks |
| ADDT-02 | P1: Adding and removing | Tasks | In Tasks |
| DEL-01 | P1: Adding and removing | Tasks | In Tasks |
| DEL-02 | P1: Adding and removing | Tasks | In Tasks |
| DEL-03 | P1: Adding and removing | Tasks | In Tasks |
| ORD-01 | P1: Adding and removing | Tasks | In Tasks |
| CARD-01 | P1: Card | Tasks | In Tasks |
| CARD-02 | P1: Card | Tasks | In Tasks |
| CARD-03 | P1: Card | Tasks | In Tasks |
| CARD-04 | P1: Card | Tasks | In Tasks |
| CARD-05 | P1: Card | Tasks | In Tasks |
| MDL-01 | P1: The add modal requires a card | Tasks | In Tasks |
| MDL-02 | P1: The add modal requires a card | Tasks | In Tasks |
| MDL-03 | P1: The add modal requires a card | Tasks | In Tasks |
| MDL-04 | P1: The add modal requires a card | Tasks | In Tasks |
| MDL-05 | P1: The add modal requires a card | Tasks | In Tasks |
| MOD-01 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-02 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-03 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-04 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-05 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-06 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-07 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-08 | P1: The module replaces the mockup | Tasks | In Tasks |
| MOD-09 | P1: The module replaces the mockup | Tasks | In Tasks |

**ID format:** `[CATEGORY]-[NUMBER]`

**Status values:** Pending → In Design → In Tasks → Implementing → Verified

**Coverage:** 56 total, 56 mapped to tasks (T1–T20) and checks C1–C58, 0 unmapped.

---

## Sources

- `.specs/features/002-home-page/spec.md` - the page behaviour this spec keeps (HOME, DAY, ADD, MOCK, SCR) and the two criteria it replaces (ADD-13, ADD-17)
- `.specs/STATE.md` AD-001 (module layout), AD-007 (the mockup is deleted when a module provides the data), AD-008 to AD-011 (this spec)
- `docs/architecture.md` - the rules the module must satisfy

---

## Success Criteria

- [ ] Every rule on a LedgerEntry is proven by a domain test that needs no page and no storage.
- [ ] Reading any date on the page shows the same sums and saldos as before, with no duplicate and no removed day returning.
- [ ] No file under `src/` mentions the mockup.
