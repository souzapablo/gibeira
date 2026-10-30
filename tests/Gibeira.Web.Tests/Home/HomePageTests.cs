using Bunit;

using Gibeira.Web.Components.Pages;
using Gibeira.Web.Ledger;
using Gibeira.Web.Mockup;

using Microsoft.Extensions.DependencyInjection;

namespace Gibeira.Web.Tests.Home;

public sealed class HomePageTests : BunitContext
{
    private static readonly DateOnly Today = new(2026, 10, 29);

    private IRenderedComponent<Gibeira.Web.Components.Pages.Home> RenderHome(ILedgerStore? store = null)
    {
        var time = new FixedTime(Today);
        Services.AddSingleton<TimeProvider>(time);
        Services.AddSingleton(store ?? new InMemoryLedgerStore(time));
        return Render<Gibeira.Web.Components.Pages.Home>();
    }

    // HOME-02: on 2026-10-29 the page opens on out/2026 - set/2027.
    [Fact]
    public void PeriodStartsAtCurrentMonthWithPtBrLabel()
    {
        var home = RenderHome();

        Assert.Equal("out/2026 - set/2027", home.Find(".period-pill").TextContent.Trim());
    }

    // HOME-07: twelve month tables in the period's month order, following the period bar.
    [Fact]
    public void MonthTablesFollowPeriodInOrder()
    {
        var home = RenderHome();

        Assert.Equal(
            ["outubro de 2026", "novembro de 2026", "dezembro de 2026", "janeiro de 2027", "fevereiro de 2027", "março de 2027",
             "abril de 2027", "maio de 2027", "junho de 2027", "julho de 2027", "agosto de 2027", "setembro de 2027"],
            home.FindAll(".month-title").Select(t => t.TextContent.Trim()));

        home.Find("button[aria-label='mês seguinte']").Click();

        var titles = home.FindAll(".month-title").Select(t => t.TextContent.Trim()).ToList();
        Assert.Equal(12, titles.Count);
        Assert.Equal("novembro de 2026", titles[0]);
        Assert.Equal("outubro de 2027", titles[11]);
        Assert.Equal("nov/2026 - out/2027", home.Find(".period-pill").TextContent.Trim());
    }

    // MOCK-03: the page shows what the store returns, whichever store it is.
    [Fact]
    public void HomeReadsDataOnlyThroughILedgerStore()
    {
        var store = new StubStore();
        var home = RenderHome(store);

        Assert.Equal(Period.Starting(Today), Assert.Single(store.Asked));
        var october = home.FindAll(".month")[0];
        var third = october.QuerySelectorAll("tbody tr")[2];
        Assert.Equal("R$ 4.321,00", third.QuerySelectorAll("td.money-cell")[3].QuerySelector(".money")!.TextContent.Trim());
        Assert.Equal("R$ -12,34", third.QuerySelector("td.saldo-cell")!.TextContent.Trim());
        Assert.Equal("R$ 98.765,43", october.QuerySelectorAll("tfoot td.money-cell")[0].QuerySelector(".money")!.TextContent.Trim());
    }

    // DAY-01: a money cell opens the drawer on its day, filtered by its kind.
    [Fact]
    public void MoneyCellOpensDrawerFilteredByKind()
    {
        var home = RenderHome();
        Assert.Empty(home.FindAll(".drawer"));

        MoneyCell(home, 0, 10, Kind.Cartao).Click();

        Assert.Equal("10/out", home.Find(".drawer-title").TextContent.Trim());
        Assert.Equal("cartão", home.Find(".filter-pill").TextContent.Trim());
        Assert.Equal(["mercado do mês"], Descriptions(home));

        MoneyCell(home, 0, 6, Kind.Economia).Click();

        Assert.Single(home.FindAll(".drawer"));
        Assert.Equal("6/out", home.Find(".drawer-title").TextContent.Trim());
        Assert.Equal("economias", home.Find(".filter-pill").TextContent.Trim());
        Assert.Equal(["reserva"], Descriptions(home));
    }

    // DAY-02: a dia cell opens the drawer with todos.
    [Fact]
    public void DiaCellOpensDrawerWithTodos()
    {
        var home = RenderHome();
        MoneyCell(home, 0, 10, Kind.Cartao).Click();

        Row(home, 0, 6).QuerySelector(".day-cell button")!.Click();

        Assert.Equal("6/out", home.Find(".drawer-title").TextContent.Trim());
        Assert.Equal("todos", home.Find(".filter-pill").TextContent.Trim());
        Assert.Equal(["reserva", "assinaturas", "dia a dia"], Descriptions(home));
    }

    // DAY-12: excluir removes that occurrence only; the drawer, the cell, the totals and every later saldo update.
    [Fact]
    public void ExcluirRemovesOccurrenceAndUpdatesSaldos()
    {
        var home = RenderHome();
        var saldoTenth = Saldo(home, 0, 10);
        var saldoEleventh = Saldo(home, 0, 11);
        var saldoNextMonth = Saldo(home, 1, 1);
        var saldoNinth = Saldo(home, 0, 9);
        Assert.Equal("R$ 1.427,67", TotalCell(home, 0, Kind.Cartao));
        MoneyCell(home, 0, 10, Kind.Cartao).Click();

        home.Find(".entry button[aria-label='opções']").Click();
        home.Find(".entry .dropdown-option").Click();

        Assert.Equal("nenhum lançamento", home.Find(".entries-empty").TextContent.Trim());
        Assert.Equal("R$ 0,00", MoneyCell(home, 0, 10, Kind.Cartao).QuerySelector(".money")!.TextContent.Trim());
        Assert.Equal("R$ 477,67", TotalCell(home, 0, Kind.Cartao));
        Assert.Equal(saldoTenth + 950m, Saldo(home, 0, 10));
        Assert.Equal(saldoEleventh + 950m, Saldo(home, 0, 11));
        Assert.Equal(saldoNextMonth + 950m, Saldo(home, 1, 1));
        Assert.Equal(saldoNinth, Saldo(home, 0, 9));
        Assert.Equal("R$ 950,00", MoneyCell(home, 1, 10, Kind.Cartao).QuerySelector(".money")!.TextContent.Trim());
    }

    // HOME-18: adicionar in the top bar opens the modal on entrada and today.
    [Fact]
    public void TopBarAddOpensModalWithEntradaAndToday()
    {
        var home = RenderHome();
        Assert.Empty(home.FindAll(".modal"));

        home.Find(".top-bar .button").Click();

        Assert.Equal("adicionar", home.Find(".modal .modal-title").TextContent.Trim());
        Assert.Equal("entrada", ModalRowLabel(home, 1));
        Assert.Equal("29/10/26", home.FindAll(".modal .field-row")[3].QuerySelector(".field-row-value")!.TextContent.Trim());
        Assert.Equal("adicionar entrada", AddButton(home).TextContent.Trim());
    }

    // DAY-10: + in the drawer opens the modal on the drawer's day and the filter's kind, or entrada for todos.
    [Fact]
    public void DrawerPlusOpensModalWithDayAndKind()
    {
        var home = RenderHome();
        MoneyCell(home, 0, 10, Kind.Cartao).Click();

        home.Find(".drawer-header button[aria-label='adicionar']").Click();

        Assert.Equal("cartão", ModalRowLabel(home, 1));
        Assert.Equal("10/10/26", home.FindAll(".modal .field-row")[3].QuerySelector(".field-row-value")!.TextContent.Trim());
        Assert.Equal("adicionar cartão", AddButton(home).TextContent.Trim());

        home.Find(".modal-header button[aria-label='fechar']").Click();
        Row(home, 0, 6).QuerySelector(".day-cell button")!.Click();
        home.Find(".drawer-header button[aria-label='adicionar']").Click();

        Assert.Equal("entrada", ModalRowLabel(home, 1));
        Assert.Equal("06/10/26", home.FindAll(".modal .field-row")[3].QuerySelector(".field-row-value")!.TextContent.Trim());
    }

    // ADD-21: adding closes the modal and updates the tables, totals, saldos and the open drawer.
    [Fact]
    public void AddingItemUpdatesTablesAndOpenDrawer()
    {
        var home = RenderHome();
        var saidaTwelfth = MoneyValue(MoneyCell(home, 0, 12, Kind.Saida));
        var saidaTotal = TotalCell(home, 0, Kind.Saida);
        var saldoEleventh = Saldo(home, 0, 11);
        var saldoTwelfth = Saldo(home, 0, 12);
        var saldoNextMonth = Saldo(home, 1, 1);
        Row(home, 0, 12).QuerySelector(".day-cell button")!.Click();
        Assert.Equal(["dia a dia"], Descriptions(home));

        home.Find(".drawer-header button[aria-label='adicionar']").Click();
        home.Find(".modal input.amount-input").Input("100");
        home.Find(".modal input.description-input").Input("farmácia");
        home.FindAll(".modal .field-row")[1].Click();
        home.FindAll(".modal .dropdown-option").Single(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim() == "saída").Click();
        AddButton(home).Click();

        Assert.Empty(home.FindAll(".modal"));
        Assert.Equal(["dia a dia", "farmácia"], Descriptions(home));
        Assert.Equal(saidaTwelfth + 100m, MoneyValue(MoneyCell(home, 0, 12, Kind.Saida)));
        Assert.Equal(ParseMoney(saidaTotal) + 100m, ParseMoney(TotalCell(home, 0, Kind.Saida)));
        Assert.Equal(saldoEleventh, Saldo(home, 0, 11));
        Assert.Equal(saldoTwelfth - 100m, Saldo(home, 0, 12));
        Assert.Equal(saldoNextMonth - 100m, Saldo(home, 1, 1));
    }

    // ADD-22 through the page: an item added with no description is listed in the drawer under its type's name.
    [Fact]
    public void HomeEmptyDescriptionSavesTypeName()
    {
        var home = RenderHome();
        Row(home, 0, 12).QuerySelector(".day-cell button")!.Click();

        home.Find(".drawer-header button[aria-label='adicionar']").Click();
        home.Find(".modal input.amount-input").Input("100");
        home.FindAll(".modal .field-row")[1].Click();
        home.FindAll(".modal .dropdown-option").Single(o => o.QuerySelector(".dropdown-label")!.TextContent.Trim() == "saída").Click();
        AddButton(home).Click();

        Assert.Equal(["dia a dia", "saída"], Descriptions(home));
    }

    // ADD-23: a second click on the add button while the first is being handled adds nothing.
    [Fact]
    public void DoubleSubmitAddsOnce()
    {
        var store = new ReentrantStore(new InMemoryLedgerStore(new FixedTime(Today)));
        var home = RenderHome(store);
        var entradaTotal = ParseMoney(TotalCell(home, 0, Kind.Entrada));
        home.Find(".top-bar .button").Click();
        home.Find(".modal input.amount-input").Input("250");
        var button = AddButton(home);
        store.BeforeAdd = () => button.Click();

        button.Click();

        Assert.Equal(1, store.Adds);
        Assert.Empty(home.FindAll(".modal"));
        Assert.Equal(entradaTotal + 250m, ParseMoney(TotalCell(home, 0, Kind.Entrada)));
    }

    // UI-06: with the drawer, the modal and a popover open, every icon-only control has a pt-BR label and every
    // drawer, modal, dropdown and popover is a dialog.
    [Fact]
    public void IconOnlyControlsHaveAriaLabelsAndDialogsHaveRole()
    {
        string[] ptBrLabels =
            ["12 meses antes", "mês anterior", "mês seguinte", "12 meses depois", "fechar", "dia anterior", "dia seguinte", "adicionar", "opções", "anterior", "próximo"];
        var home = RenderHome();
        MoneyCell(home, 0, 10, Kind.Cartao).Click();
        home.Find(".drawer-header button[aria-label='adicionar']").Click();
        home.FindAll(".modal .field-row")[3].Click();

        Assert.Equal("dialog", home.Find(".drawer").GetAttribute("role"));
        Assert.Equal("dialog", home.Find(".modal").GetAttribute("role"));
        Assert.Equal("dialog", home.Find(".stepper").GetAttribute("role"));
        var iconOnly = home.FindAll("button").Where(b => b.TextContent.Trim().Length == 0).ToList();
        Assert.NotEmpty(iconOnly);
        Assert.All(iconOnly, b => Assert.Contains(b.GetAttribute("aria-label"), ptBrLabels));
        Assert.Contains(iconOnly, b => b.Closest(".stepper") is not null);
        Assert.Contains(iconOnly, b => b.Closest(".drawer") is not null);
        Assert.Contains(iconOnly, b => b.Closest(".modal") is not null);

        home.Find(".stepper").KeyDown(key: "Escape");
        home.FindAll(".modal .field-row")[4].Click();
        Assert.Equal("dialog", home.Find(".modal .dropdown").GetAttribute("role"));
        home.Find(".modal").KeyDown(key: "Escape");
        home.Find(".period-pill").Click();
        Assert.Equal("dialog", home.Find(".period-bar .dropdown").GetAttribute("role"));
        home.Find(".period-bar .dropdown").KeyDown(key: "Escape");
        home.Find(".filter-pill").Click();
        Assert.Equal("dialog", home.Find(".drawer .dropdown").GetAttribute("role"));
    }

    private static string ModalRowLabel(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int row) =>
        home.FindAll(".modal .field-row")[row].QuerySelector(".field-row-label")!.TextContent.Trim();

    private static AngleSharp.Dom.IElement AddButton(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home) =>
        home.Find(".modal .add-button-slot .button");

    private static decimal MoneyValue(AngleSharp.Dom.IElement cell) => ParseMoney(cell.QuerySelector(".money")!.TextContent.Trim());

    // R$ -1.234,56 → -1234.56
    private static decimal ParseMoney(string text) =>
        decimal.Parse(text["R$ ".Length..].Replace(".", "", StringComparison.Ordinal).Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture);

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, int day) =>
        home.FindAll(".month")[month].QuerySelectorAll("tbody tr")[day - 1];

    private static AngleSharp.Dom.IElement MoneyCell(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, int day, Kind kind) =>
        Row(home, month, day).QuerySelectorAll("td.money-cell")[(int)kind].QuerySelector("button")!;

    private static string TotalCell(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, Kind kind) =>
        home.FindAll(".month")[month].QuerySelectorAll("tfoot td.money-cell")[(int)kind].QuerySelector(".money")!.TextContent.Trim();

    private static decimal Saldo(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, int day) =>
        ParseMoney(Row(home, month, day).QuerySelector("td.saldo-cell")!.TextContent.Trim());

    private static string[] Descriptions(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home) =>
        home.FindAll(".entry-description").Select(d => d.TextContent.Trim()).ToArray();

    // The mock store, with a hook that runs inside the first Add, before the page can close the modal.
    private sealed class ReentrantStore(ILedgerStore inner) : ILedgerStore
    {
        public Action? BeforeAdd { get; set; }

        public int Adds { get; private set; }

        public IReadOnlyList<Card> Cards => inner.Cards;

        public IReadOnlyList<string> Tags => inner.Tags;

        public IReadOnlyList<MonthLedger> Months(Period period) => inner.Months(period);

        public IReadOnlyList<DayEntry> Day(DateOnly day, Kind? filter) => inner.Day(day, filter);

        public DateOnly DueDate(Card card, DateOnly onOrAfter, int monthsAhead) => inner.DueDate(card, onOrAfter, monthsAhead);

        public void Add(NewItem item)
        {
            Adds++;
            var hook = BeforeAdd;
            BeforeAdd = null;
            hook?.Invoke();
            inner.Add(item);
        }

        public void Remove(Guid itemId, DateOnly day) => inner.Remove(itemId, day);
    }

    // Every day of every month is zero, but the 3rd: an economia of 4.321,00 and a saldo of -12,34; entradas total 98.765,43.
    private sealed class StubStore : ILedgerStore
    {
        private static readonly Kind[] Kinds = Enum.GetValues<Kind>();

        public List<Period> Asked { get; } = [];

        public IReadOnlyList<Card> Cards => [];

        public IReadOnlyList<string> Tags => [];

        public IReadOnlyList<MonthLedger> Months(Period period)
        {
            Asked.Add(period);
            return period.Months.Select(month =>
            {
                var days = Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month)).Select(day =>
                {
                    var sums = Kinds.ToDictionary(k => k, k => day == 3 && k == Kind.Economia ? 4321m : 0m);
                    return new DayRow(new DateOnly(month.Year, month.Month, day), sums, day == 3 ? -12.34m : 0m, false, false);
                }).ToList();
                var totals = Kinds.ToDictionary(k => k, k => k == Kind.Entrada ? 98765.43m : 0m);
                return new MonthLedger(month, days, totals);
            }).ToList();
        }

        public IReadOnlyList<DayEntry> Day(DateOnly day, Kind? filter) => [];

        public DateOnly DueDate(Card card, DateOnly onOrAfter, int monthsAhead) => onOrAfter;

        public void Add(NewItem item)
        {
        }

        public void Remove(Guid itemId, DateOnly day)
        {
        }
    }
}