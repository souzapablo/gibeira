# Home page screens

**Verifier**: independent sub-agent (author != verifier)
**Round**: 1
**Pictures**: my own. Playwright Chromium against http://localhost:5196/ at HEAD dfa3ce0, with the window opened at each size and never resized afterwards, taken on 2026-10-30 (today = 30/out). Script: `shoot.cs`, plus `detail.cs` (today row, 2x zoom) and `probe.cs` (computed styles) in this folder.
**Source**: the design canvas boards `Main.dc.html` (ledger) and `Adicionar.dc.html` (add form). They do not render on their own, so I built the inventories from their markup and inline styles. Each row says which source it follows: **spec** where the spec states the content, and **canvas** for the look (palette, fonts, surfaces, radii, buttons, pills, today highlight, top bar). The departures under `## Differences from the source` are applied: no left panel or view tabs, and the spec's labels.

| Screen | Compared with | Verdict | Finding |
| --- | --- | --- | --- |
| `01-ledger--1290x902.png` | `Main.dc.html` + spec HOME-01..17 | diverges | 2 rows differ |
| `01-ledger--360x800.png` | `Main.dc.html` + spec HOME-01..17 | diverges | 3 rows differ |
| `01b-ledger-today--1290x902.png` | `Main.dc.html` + spec HOME-15/16 | diverges | 3 rows differ |
| `01b-ledger-today--360x800.png` | `Main.dc.html` + spec HOME-15/16 | diverges | 4 rows differ |
| `02-period-dropdown--1290x902.png` | `Main.dc.html` (look) + spec HOME-05 | diverges | 1 row differs |
| `02-period-dropdown--360x800.png` | `Main.dc.html` (look) + spec HOME-05, HOME-17 | diverges | 2 rows differ |
| `03-drawer--1290x902.png` | `Main.dc.html` (look) + spec DAY-01..04 | matches | |
| `03-drawer--360x800.png` | `Main.dc.html` (look) + spec DAY-01..04, SCR-02 | matches | |
| `04-drawer-filter--1290x902.png` | `Main.dc.html` (look) + spec DAY-06 | matches | |
| `04-drawer-filter--360x800.png` | `Main.dc.html` (look) + spec DAY-06 | matches | |
| `05-drawer-item-menu--1290x902.png` | `Main.dc.html` (look) + spec DAY-04, DAY-11 | matches | |
| `05-drawer-item-menu--360x800.png` | `Main.dc.html` (look) + spec DAY-04, DAY-11 | matches | |
| `06-add-modal--1290x902.png` | `Adicionar.dc.html` + spec ADD-01 | diverges | 1 row differs |
| `06-add-modal--360x800.png` | `Adicionar.dc.html` + spec ADD-01, SCR-01 | diverges | 1 row differs |
| `07-data-popover--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-05 | matches | |
| `07-data-popover--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-05 | matches | |
| `08-type-dropdown--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-03 | matches | |
| `08-type-dropdown--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-03 | matches | |
| `09-card-dropdown--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-04, ADD-13 | matches | |
| `09-card-dropdown--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-04, ADD-13 | matches | |
| `10-vencimento-popover--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-14 | matches | |
| `10-vencimento-popover--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-14 | matches | |
| `11-repetir-dropdown--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-07, ADD-16 | matches | |
| `11-repetir-dropdown--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-07, ADD-16 | matches | |
| `12-parcelas-popover--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-09 | matches | |
| `12-parcelas-popover--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-09 | matches | |
| `14-tags-dropdown--1290x902.png` | `Adicionar.dc.html` (look) + spec ADD-09, ADD-19 | matches | |
| `14-tags-dropdown--360x800.png` | `Adicionar.dc.html` (look) + spec ADD-09, ADD-19 | matches | |

`zoom-ledger-left--1290x902@2x.png` is a 2x crop I took to read the weekend tint. It is supporting evidence and gets no verdict of its own.

### `01-ledger--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| page ground #15130f, Instrument Sans body (canvas) | same | |
| top bar: logo (purse mark plus `gibeira` in Bricolage 800 22px) on the left and the primary pill `+ adicionar` on the right, with a 1px #2c2820 line below (canvas look, content per spec `TopBar` = logo and "adicionar") | same | the canvas date pill `qui, 29 de out` and the view tabs are absent. The tabs are an approved departure, and the spec's TopBar names only logo and adicionar |
| period bar centered: `<<`, `<`, pill `out/2026 - set/2027` with chevron, `>`, `>>` (spec HOME-01/02). Buttons are 44px round with border #2c2820 on #1d1a15 (canvas `.ib`/`.pill`) | same | note: the pill text is #f3ede2 where the canvas `.pill` uses #b9b0a1 |
| canvas `hoje` button, `todos os dias / só com movimento` segment and the `valores em R$ / saldo realizado / previsto` legend strip (canvas) | same | replaced by the spec's period bar. The past/projected legend is out of scope in the spec |
| month tables side by side as cards: #1d1a15, 1px #2c2820 border, 20px radius (canvas `.card`), with horizontal scroll inside the ledger (spec HOME-17) | same | the second month is cut at the right window edge, inside the ledger scroller. The page itself does not scroll (scrollWidth 1290) |
| month title `outubro de 2026` in Bricolage (spec HOME-07) | same | note: 20px/700, where the canvas month h1 is 26px/800 |
| header row `DIA ENTRADAS SAÍDAS DIÁRIOS ECONOMIAS CARTÃO SALDOS`: 12px/700 uppercase #958d7e with a #3a342a line below (canvas `.hd`, labels per spec) | same | |
| header kind marks: each kind column header starts with its colored dot (canvas `.hd` entradas #E8B04B, saídas #DF6A51, diários #D65DB1, economias #37A675, cartão #8C7CF0) | differs | plain uppercase labels with no colored dot |
| day rows: 46px tall with a #26221c line, tabular numbers, right-aligned money (canvas `.c`) | same | |
| dia cell: day number in Bricolage 17px/700, followed by the weekday abbreviation (`seg`, `ter`...) in 12px #958d7e (canvas `.day`; the spec is silent) | differs | the day number only, with no weekday abbreviation |
| money cell: kind badge (arrow in or out for entrada/saída, letter D/E/C) plus `R$ 1.800,00` (spec HOME-08) | same | |
| zero cell: badge and `R$ 0,00` in zero color #6f6758 (spec HOME-09, which replaces the canvas `—`) | same | |
| weekend: dia cell of Saturday and Sunday on #1a1813 (spec HOME-14 narrows the canvas whole-row tint to the dia cell) | same | rows 3, 4, 10 and 11 have the dia cell at rgb(26,24,19) |
| saldo cell: tinted coral when negative with text #ef8f79 (`R$ -282,08`), green tint when positive (spec AD-003, which replaces the canvas progress track) | same | |
| total row held at the bottom: label plus each kind's month sum with badge, saldos cell empty (spec HOME-13/16) | same | note: the label reads uppercase `TOTAL` 12px on #25211b, where the canvas has lowercase `total` on #15130f |

### `01-ledger--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| page ground and fonts (canvas) | same | |
| top bar: logo on the left, `+ adicionar` pill on the right, line below (canvas, spec TopBar) | same | |
| period bar: `<<` `<` pill `>` `>>` in one centered group, with the arrows flanking the pill (spec HOME-01; the canvas keeps prev, title and next together in one group that does not wrap) | differs | `<<` `<` and the pill on the first line, and `>` `>>` wrapped onto a second line centered under them, so `>` no longer sits beside the pill |
| ledger: first month card, scrolled horizontally inside the ledger, with no page horizontal scroll (spec HOME-17) | same | scrollWidth 360. The dia, entradas and saídas columns are visible |
| month title, header labels, header line (spec/canvas) | same | |
| header kind marks: colored dot before each kind column header (canvas) | differs | no dots |
| dia cell: day number plus weekday abbreviation (canvas) | differs | day number only |
| money and zero cells: badge plus `R$` amount, zero in #6f6758 (spec) | same | |
| weekend dia cells tinted (spec HOME-14) | same | |
| total row held at the bottom of the card (spec HOME-16) | same | |

### `01b-ledger-today--1290x902.png`

The first month is scrolled to its last rows, so today (30/out) is in view.

| Element | Result | Picture draws |
| --- | --- | --- |
| header row stays in view while the day rows scroll, and the total row stays at the bottom (spec HOME-16) | same | |
| today row: today ground #2a2316 across the row, with the day number in accent #E8B04B (canvas `.r.today`, spec HOME-15) | same | the saldo cell keeps its green tint (spec AD-003) |
| today badge: accent pill `hoje` (11px/800, #1a1408 on #E8B04B) after the day number (canvas `.hoje`, part of the today highlight) | differs | no `hoje` badge, only the accent number on the today ground |
| dia cell: weekday abbreviation after the number (canvas) | differs | day number only |
| header kind dots (canvas) | differs | no dots |
| weekend dia cells 24, 25 and 31 tinted (spec HOME-14) | same | |
| saldo cells: green tint and amounts (spec) | same | |

### `01b-ledger-today--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| period bar in one group with the arrows flanking the pill (spec/canvas) | differs | `>` `>>` wrapped onto a second line |
| header and total rows stay in view (spec HOME-16) | same | |
| today row: today ground plus accent day number (canvas, spec HOME-15) | same | |
| today badge `hoje` (canvas) | differs | no badge |
| dia cell weekday abbreviation (canvas) | differs | day number only |
| header kind dots (canvas) | differs | no dots |
| ledger scrolls inside, with no page horizontal scroll (spec HOME-17) | same | my scrollIntoView call also scrolled the ledger a few px sideways, so the card's left edge is just off view. That is the ledger's own scroller, not the page |

### `02-period-dropdown--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dropdown anchored under the period pill on the popover ground #2a261f, with a 16px-ish radius, the popover shadow and a 1px line (spec token table) | same | |
| header `PERÍODO` in the eyebrow style (12px/700 uppercase #958d7e, canvas `.eyebrow`) | same | |
| options: the 25 periods from `out/2025 - set/2026` to `out/2027 - set/2028`, in order (spec HOME-05) | same | the list scrolls inside the dropdown. The first 8 are visible |
| the shown period `out/2026 - set/2027` checked, with the check visible when the dropdown opens (spec HOME-05) | differs | the list opens scrolled to the top (scrollTop 0). The checked option is the 13th, at y=727, below the dropdown's bottom edge at y=544, so no option shows a check |

### `02-period-dropdown--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dropdown on the popover ground, with radius, shadow and header `PERÍODO` (spec/canvas) | same | |
| dropdown inside the window, with no page horizontal scroll down to 360px (spec HOME-17, render) | differs | the dropdown spans x=137 to 377 in a 360px window. Its right 17px, including the check column, is cut off, and the page gains horizontal scroll (scrollWidth 377) |
| options: 25 periods in order (spec HOME-05) | same | |
| the shown period checked and visible when the dropdown opens (spec HOME-05) | differs | the list opens at the top. The checked 13th option is scrolled out of view and no check shows |

### `03-drawer--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| drawer: a full-height panel on the right over the ledger, on the surface ground #1d1a15 with a #2c2820 edge (spec DAY-01, canvas surfaces) | same | |
| header: X and `<` on the left, title `10/out` in Bricolage in the middle, `>` and `+` on the right, all 44px round bordered icon buttons (spec DAY-01/03, canvas `.ib`) | same | |
| filter pill `cartão` with chevron below the header (spec DAY-01, canvas `.pill`) | same | |
| item row: purple `C` badge, `mercado do mês`, meta `10/10  mensalmente`, `R$ 950,00`, round `⋮` button (spec DAY-04) | same | |
| source cell (row 10, cartão) drawn on the selected ground | same | |

### `03-drawer--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| drawer at the full window width (spec SCR-02) | same | |
| header: X `<` `10/out` `>` + (spec DAY-01/03) | same | |
| filter pill `cartão` (spec DAY-01) | same | |
| item row: badge, description, `10/10 mensalmente`, `R$ 950,00`, ⋮ (spec DAY-04) | same | |

### `04-drawer-filter--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dropdown under the filter pill on the popover ground, with radius, shadow and the eyebrow header `FILTRO` (spec/canvas) | same | |
| options `todos`, `entradas`, `saídas`, `diários`, `economias`, `cartão` (spec DAY-06) | same | |
| current `cartão` checked: accent check on the selected ground (spec DAY-06) | same | |

### `04-drawer-filter--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dropdown inside the full-width drawer, with header `FILTRO` | same | |
| six options in order (spec DAY-06) | same | |
| `cartão` checked (spec DAY-06) | same | |

### `05-drawer-item-menu--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| filter pill `todos` with two items: `mercado do mês` (C, mensalmente, R$ 950,00) and `dia a dia` (D, diariamente, R$ 40,00), each with a ⋮ (spec DAY-04/07) | same | |
| menu under the second item's ⋮ on the popover ground, with radius and shadow (spec DAY-11) | same | |
| menu content: `excluir` (spec DAY-11) | same | note: the menu also has an eyebrow header `OPÇÕES` above `excluir`, which the spec does not mention |

### `05-drawer-item-menu--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| two items under filter `todos` (spec DAY-04/07) | same | |
| menu under ⋮, inside the window (spec DAY-11) | same | |
| `excluir` (spec DAY-11) | same | note: also the `OPÇÕES` header |

### `06-add-modal--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| backdrop dims the page. Dialog 640px wide, 48px from the top and horizontally centered, on #1d1a15 with a #2c2820 border, 24px radius and shadow `0 30px 80px rgba(0,0,0,.6)` (canvas dialog) | same | |
| title `adicionar` in Bricolage 800 ~28px on the left (spec ADD-01 text, canvas h1 look) | same | |
| close X on the right: a 44px round button with no border and no fill, glyph in muted #b9b0a1 (canvas `.close`) | differs | a bordered circle: 1px #2c2820 border on #1d1a15 fill, with the glyph in #f3ede2 (the Main board's `.ib` style, not the Adicionar close) |
| amount row: `R$` in muted display type plus a large Bricolage placeholder `0,00`, focused with a 2px accent outline (spec ADD-01, canvas `.money`) | same | note: 12px radius, where the canvas `.money` has 16px |
| rows in the spec's order: type (entrada badge, `entrada`, chevron), description (lines icon, placeholder `entrada`), data (calendar icon, `data`, value `30/10/26`, chevron), repetition (repeat icon, `não repete`, chevron), tags (tag icon, `tags`, chevron). Each row is 52px, with a 12px radius, a #3a342a border and #15130f fill (spec ADD-01 content, canvas `.in` look) | same | the canvas type grid, `o que é`/`quanto`/`quando` labels, tag chips and the gibeira preview strip are replaced by the spec's rows |
| footer: one pill button `adicionar entrada` on the right, accent colored and disabled while the amount is empty (spec ADD-01/04/20, canvas `.btn.primary`) | same | the canvas `cancelar` button is not in the spec. Note: 44px high where the canvas has 48px |

### `06-add-modal--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dialog at full width minus a 16px gutter each side (328px) on the dimmed backdrop (spec SCR-01, canvas dialog look) | same | |
| title `adicionar` (spec/canvas) | same | |
| close X: borderless and transparent, with a muted glyph (canvas `.close`) | differs | a bordered filled circle with a light glyph |
| amount row focused (spec ADD-01) | same | |
| five rows: type, description, data `30/10/26`, `não repete`, `tags`, with chevrons on the four pickers (spec ADD-01) | same | |
| `adicionar entrada` disabled pill on the right (spec ADD-01/20) | same | |

### `07-data-popover--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| popover anchored under the data row on the popover ground, with radius and shadow (spec ADD-05) | same | |
| header `DATA` in the eyebrow style (spec ADD-05) | same | |
| stepper: round `<`, `30/10/2026` in Bricolage, round `>` (spec ADD-05) | same | |
| full-width light pill `salvar` (spec ADD-05, Button light) | same | |

### `07-data-popover--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| popover inside the modal width, under the data row (spec ADD-05) | same | |
| `DATA`, `<` `30/10/2026` `>`, `salvar` (spec ADD-05) | same | |

### `08-type-dropdown--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dropdown under the type row on the popover ground, with header `TIPO` | same | |
| options with their kind badges: `entrada`, `saída`, `diário`, `economia`, `cartão` (spec ADD-03, kind colors per token table) | same | |
| current `entrada` checked on the selected ground (spec ADD-03) | same | |

### `08-type-dropdown--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| dropdown inside the window, with header `TIPO` | same | |
| five kind options with badges (spec ADD-03) | same | |
| `entrada` checked (spec ADD-03) | same | |

### `09-card-dropdown--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| type row now shows the purple `C` badge and `cartão`, and the button reads `adicionar cartão` in kind color #8C7CF0, disabled (spec ADD-04/20) | same | |
| dropdown header `CARTÃO` with a disabled `+ adicionar cartão` pill button (spec ADD-13) | same | |
| options `Itaú`, `Santander`, `sem cartão`, with `sem cartão` checked (spec ADD-13) | same | |

### `09-card-dropdown--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| type row `cartão`, button `adicionar cartão` in purple (spec ADD-04) | same | |
| header `CARTÃO` plus the disabled add-card button (spec ADD-13) | same | |
| `Itaú`, `Santander`, `sem cartão` checked, inside the window (spec ADD-13) | same | |

### `10-vencimento-popover--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| description placeholder now `cartão` (spec ADD-04) | same | |
| popover under the data row, with header `VENCIMENTO` (spec ADD-14) | same | |
| `<` `10/11/2026` `>`: Itaú's first due date on or after 30/10/2026 (spec ADD-14) | same | |
| light pill `salvar` (spec ADD-14) | same | |

### `10-vencimento-popover--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| header `VENCIMENTO`, `<` `10/11/2026` `>`, `salvar`, inside the window (spec ADD-14) | same | |

### `11-repetir-dropdown--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| data row relabeled `vencimento` with `10/11/26` (spec ADD-16) | same | |
| dropdown under the repetition row, with header `REPETIR` (spec ADD-07) | same | it floats past the modal's bottom edge over the backdrop and is fully visible |
| options with descriptions: `mensalmente` / "repete o mesmo valor mensalmente", `semanalmente` / "...semanalmente", `diariamente` / "...diariamente", `parcelado` / "divide o valor pelo número de parcelas", `não repete` (spec ADD-07) | same | |
| `não repete` checked (spec ADD-07) | same | |

### `11-repetir-dropdown--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| `vencimento 10/11/26` row (spec ADD-16) | same | |
| header `REPETIR`, five options with descriptions, `não repete` checked, inside the window (spec ADD-07) | same | |

### `12-parcelas-popover--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| popover under the repetition row, with header `PARCELAS` (spec ADD-09) | same | |
| `<` `2` `>`, starting at 2 (spec ADD-09) | same | |
| light pill `salvar` (spec ADD-09) | same | |

### `12-parcelas-popover--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| `PARCELAS`, `<` `2` `>`, `salvar`, inside the window (spec ADD-09) | same | |

### `14-tags-dropdown--1290x902.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| repetition row shows `parcelado em 2x` (spec ADD-09) | same | |
| tags row shows `tags` plus the checked names `casa, mercado` (spec ADD-19) | same | |
| dropdown with header `TAGS` and options `casa`, `mercado`, `transporte`, `lazer`, `saúde`, each with a check box. `casa` and `mercado` are checked (accent filled box) on the selected ground (spec ADD-19, multiple choice) | same | |

### `14-tags-dropdown--360x800.png`

| Element | Result | Picture draws |
| --- | --- | --- |
| `parcelado em 2x` and `tags  casa, mercado` rows (spec ADD-09/19) | same | |
| `TAGS` dropdown with five check-box options, two checked, inside the window (spec ADD-19) | same | |

## Fixes after round 1

| Differs | Fix | Closed |
| --- | --- | --- |
| Kind headers have no colored dot | HOME-19: a 9 px dot in the kind color before each kind header | 5f7c028 - C73 |
| Dia cells have no weekday | HOME-20: the pt-BR weekday in 12 px subtle text after the day number | 5f7c028 - C74 |
| Today's row has no `hoje` badge | HOME-21: the accent `hoje` pill after today's weekday | 5f7c028 - C75 |
| Period bar wraps `>` `>>` at 360 px | HOME-23: tighter gaps, a 12 px label and 40 px wide arrows (still 44 px high) below 432 px | 90f4a2c - C77 |
| Period list opens scrolled to the top, checked period hidden | HOME-22: a dropdown opens focused on its first checked option, which scrolls it into view | 90f4a2c - C76 |
| Period list cut off at 360 px and page scrolls sideways | HOME-23: the period list opens centered under the pill | 90f4a2c - C77 |
| Modal X drawn bordered on a surface | `IconButton Bare`: no border or ground, muted glyph; the modal's X uses it | 24874df |
