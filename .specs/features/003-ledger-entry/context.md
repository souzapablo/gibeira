# Ledger Entry Context

**Gathered:** 2026-11-05
**Spec:** `.specs/features/003-ledger-entry/spec.md`
**Status:** Tasks approved

---

## Feature Boundary

Design the LedgerEntry entity with tactical DDD, put it in the first finance module with in-memory storage, and retire the home page's mockup. Persistence is left out.

---

## Implementation Decisions

### What a LedgerEntry is

- The LedgerEntry holds all its information. User: "I want the transaction to have all information. The day, type, amount etc"
- Asked how a salary that repeats monthly with no end date should work, the user chose "One per day, on demand (Recommended)": each month's salary is its own complete LedgerEntry, a small RecurringRule creates more as the user moves to later periods, and removing one day deletes that LedgerEntry. Parcelado creates its N LedgerEntries at once, with no rule.

### Name

- The entity started as "Transaction" in the user's words. Asked whether Transaction was the right term or a better match for "lançamento" existed, the user accepted the proposed `LedgerEntry` and asked whether it could be just `Entry`. Told that a bare `Entry` reads like `entrada`, the user said: "Yeah, we can go with LedgerEntry". Asked again once the kinds became English, the user still chose `LedgerEntry` over `Entry`: "yes".

### Code language

- User: "do not use portuguese on the code, only for screens that users will face". This covers new and existing code (AD-011).
- Asked whether the rename of existing code should stay in this feature, the user said "yes".

### How far the spec goes

- Asked how far the spec should go, the user chose "Module replaces mockup (Recommended)": a new Gibeira.Ledger module with Domain, Application and in-memory Infrastructure with seed data; the page reads it through ILedgerStore; Mockup/ is deleted; the page looks and behaves the same.

### Behaviours

- Asked which behaviours a LedgerEntry should have, the user chose "Add and remove only (Recommended)": only what the page does today; edit methods arrive with the edit feature.

### Card rule

- Asked whether a cartão LedgerEntry can have no card, the user chose "Cartão requires a card": every cartão LedgerEntry names a card, and `sem cartão` leaves the modal. This changes spec 002's ADD-13 and ADD-17. The agent's recommendation had been to keep the card optional.

### Agent's Discretion

- Which DDD building blocks to use. User: "I want ddd, but not necessarly all of it. just what makes sense"
- Storage. User: "We don't need to think of presistence right not, we can use in memory or mocked items on the backend"

### Declined / Undiscussed Gray Areas → Assumptions

The defaults marked `n` in the spec's Assumptions table, for the user to correct before approval.

---

## Specific References

No specific requirements - open to standard approaches.

---

## Deferred Ideas

- Editing a LedgerEntry, and changing or ending a repetition from a given day onward.
- Marking a LedgerEntry as paid. One LedgerEntry per day makes this cheap later.
- A purchase date on cartão LedgerEntries, and card bills (faturas).
