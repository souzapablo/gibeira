# Home Page Context

**Gathered:** 2026-10-29
**Spec:** `.specs/features/002-home-page/spec.md`
**Status:** Ready for design once the spec is approved

---

## Feature Boundary

The `/` page of `Gibeira.Web`: top bar, 12-month ledger, day drawer and add modal on in-memory mock data, built from a tokens file and reusable components.

---

## Implementation Decisions

### Palette

- User chose "Canvas palette, tinted saldo (Recommended)": canvas kind colors; the saldo cell keeps positive/negative meaning as a muted tint of #37A675 / #DF6A51 (AD-003).

### Wording

- User chose the labels written in the spec over the canvas vocabulary (cofrinho, bolsinho, colocar na gibeira).

### Layout

- User chose the ledger layout plus the canvas top bar: the period bar and month tables, plus the canvas top bar (logo, "adicionar") and today's row highlight; no left panel, no view tabs (AD-004).

### Mock interactivity

- User chose "Add updates in memory (Recommended)": "adicionar" writes to an in-memory store and the tables, saldos and drawer update; ⋮ offers "excluir"; everything resets on reload; editing comes in a later feature.

### Mockup is temporary

- User: "any logic other than the frontend is temporary to display the frontend mockup". Everything that is not frontend lives in `Gibeira.Web/Mockup/` behind `ILedgerStore` and is deleted when a finance module provides the data (AD-007).
- Asked where the ledger rules should live, the user chose "Plain C# in Gibeira.Web (Recommended)".

### Agent's Discretion

None delegated.

### Declined / Undiscussed Gray Areas → Assumptions

The defaults marked `n` in the spec's Assumptions table (period dropdown, cell click, cartão on due date, data and parcelas popovers, recurrence with no end, excluir on one occurrence without confirmation, empty drawer text, fonts, icons, widths, test tools) await the user's correction at spec approval.

---

## Specific References

- Design canvas https://claude.ai/artifact/WEboiPZKbDSccEKkXZhG8m, board `Main.dc.html` ("saldos") and `Adicionar.dc.html`: tokens, fonts, surfaces, buttons, today highlight, top bar.

---

## Deferred Ideas

- Editing an item from ⋮.
- Managing cards (the add-card icon) and tags.
- The canvas pages totais, horizonte and tags.
