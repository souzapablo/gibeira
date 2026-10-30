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

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, int day) =>
        home.FindAll(".month")[month].QuerySelectorAll("tbody tr")[day - 1];

    private static AngleSharp.Dom.IElement MoneyCell(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, int day, Kind kind) =>
        Row(home, month, day).QuerySelectorAll("td.money-cell")[(int)kind].QuerySelector("button")!;

    private static string TotalCell(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, Kind kind) =>
        home.FindAll(".month")[month].QuerySelectorAll("tfoot td.money-cell")[(int)kind].QuerySelector(".money")!.TextContent.Trim();

    // R$ -1.234,56 → -1234.56
    private static decimal Saldo(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home, int month, int day) =>
        decimal.Parse(
            Row(home, month, day).QuerySelector("td.saldo-cell")!.TextContent.Trim()["R$ ".Length..].Replace(".", "", StringComparison.Ordinal).Replace(',', '.'),
            System.Globalization.CultureInfo.InvariantCulture);

    private static string[] Descriptions(IRenderedComponent<Gibeira.Web.Components.Pages.Home> home) =>
        home.FindAll(".entry-description").Select(d => d.TextContent.Trim()).ToArray();

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