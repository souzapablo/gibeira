using Bunit;

using Gibeira.Web.Components.Home;
using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Home;

public sealed class MonthTableTests : BunitContext
{
    private static readonly DateOnly December = new(2026, 12, 1);

    private static readonly Kind[] Kinds = [Kind.Entrada, Kind.Saida, Kind.Diario, Kind.Economia, Kind.Cartao];

    // December 2026: an entrada of 1.189,27 on the 10th, a saldo that turns negative on the 15th and is zero on the 20th.
    private static MonthLedger Ledger(DateOnly? today = null)
    {
        var days = Enumerable.Range(1, 31).Select(day =>
        {
            var date = new DateOnly(2026, 12, day);
            var sums = Kinds.ToDictionary(kind => kind, _ => 0m);
            if (day == 10)
            {
                sums[Kind.Entrada] = 1189.27m;
                sums[Kind.Cartao] = 250m;
            }

            var saldo = day switch { < 15 => 500m, 20 => 0m, _ => -477.67m };
            return new DayRow(date, sums, saldo, date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday, date == today);
        }).ToList();

        var totals = new Dictionary<Kind, decimal>
        {
            [Kind.Entrada] = 15500m,
            [Kind.Saida] = 3210.5m,
            [Kind.Diario] = 0m,
            [Kind.Economia] = 800m,
            [Kind.Cartao] = 1189.27m,
        };
        return new MonthLedger(December, days, totals);
    }

    private IRenderedComponent<MonthTable> RenderTable(DateOnly? today = null) =>
        Render<MonthTable>(parameters => parameters.Add(p => p.Ledger, Ledger(today)));

    private static AngleSharp.Dom.IElement Row(IRenderedComponent<MonthTable> table, int day) => table.FindAll("tbody tr")[day - 1];

    // HOME-07: titled in lowercase pt-BR, the columns dia to saldos, one row per calendar day.
    [Fact]
    public void MonthTableHasPtBrTitleColumnsAndOneRowPerDay()
    {
        var table = RenderTable();

        Assert.Equal("dezembro de 2026", table.Find(".month-title").TextContent.Trim());
        Assert.Equal(
            ["dia", "entradas", "saídas", "diários", "economias", "cartão", "saldos"],
            table.FindAll("thead th").Select(th => th.TextContent.Trim()));

        var rows = table.FindAll("tbody tr");
        Assert.Equal(31, rows.Count);
        Assert.Equal(Enumerable.Range(1, 31).Select(d => d.ToString(System.Globalization.CultureInfo.InvariantCulture)),
            rows.Select(r => r.QuerySelector(".day-number")!.TextContent.Trim()));
        Assert.All(rows, r => Assert.Equal(7, r.Children.Length));
    }

    // HOME-08: a money cell shows its kind icon and the day's sum for that kind as R$ 1.189,27.
    [Fact]
    public void MoneyCellShowsKindIconAndDaySum()
    {
        var table = RenderTable();

        var cells = Row(table, 10).QuerySelectorAll("td.money-cell");
        Assert.Equal(5, cells.Length);
        Assert.Equal("R$ 1.189,27", cells[0].QuerySelector(".money")!.TextContent.Trim());
        Assert.Equal("R$ 250,00", cells[4].QuerySelector(".money")!.TextContent.Trim());

        // Five icons per day row in render order: day 10 holds the tenth five.
        var icons = table.FindComponents<KindIcon>().Skip(9 * 5).Take(5).ToList();
        Assert.Equal(Kinds, icons.Select(i => i.Instance.Kind));
        Assert.False(icons[0].Instance.Muted);
        Assert.False(icons[4].Instance.Muted);
    }

    // HOME-09: a zero sum shows its icon and R$ 0,00 in the zero color.
    [Fact]
    public void ZeroCellIsMuted()
    {
        var table = RenderTable();
        var row = Row(table, 10);

        var saida = row.QuerySelectorAll("td.money-cell")[1];
        Assert.Equal("R$ 0,00", saida.QuerySelector(".money")!.TextContent.Trim());
        Assert.Contains("var(--text-zero)", saida.QuerySelector(".money")!.GetAttribute("style"));
        Assert.Contains("var(--text-zero)", saida.QuerySelector(".kind-icon")!.GetAttribute("style"));

        var entrada = row.QuerySelectorAll("td.money-cell")[0];
        Assert.DoesNotContain("var(--text-zero)", entrada.QuerySelector(".money")!.GetAttribute("style") ?? "");
        Assert.Contains("var(--kind-entrada)", entrada.QuerySelector(".kind-icon")!.GetAttribute("style"));
    }

    // HOME-12 (AD-003): a saldo of zero or more gets the positive tint, a negative one the negative tint.
    [Fact]
    public void SaldoCellTintFollowsSign()
    {
        var table = RenderTable();

        var positive = Row(table, 1).QuerySelector("td.saldo-cell")!;
        Assert.Contains("saldo-cell--positive", positive.ClassList);
        Assert.Equal("R$ 500,00", positive.TextContent.Trim());

        var zero = Row(table, 20).QuerySelector("td.saldo-cell")!;
        Assert.Contains("saldo-cell--positive", zero.ClassList);
        Assert.DoesNotContain("saldo-cell--negative", zero.ClassList);

        var negative = Row(table, 15).QuerySelector("td.saldo-cell")!;
        Assert.Contains("saldo-cell--negative", negative.ClassList);
        Assert.DoesNotContain("saldo-cell--positive", negative.ClassList);
        Assert.Equal("R$ -477,67", negative.TextContent.Trim());
    }

    // HOME-13: the total row holds each kind's month sum with its icon, and an empty saldos cell.
    [Fact]
    public void TotalRowHoldsMonthSumsAndEmptySaldo()
    {
        var table = RenderTable();

        var total = table.Find("tfoot tr");
        Assert.Equal(7, total.Children.Length);
        var cells = total.QuerySelectorAll("td.money-cell");
        Assert.Equal(
            ["R$ 15.500,00", "R$ 3.210,50", "R$ 0,00", "R$ 800,00", "R$ 1.189,27"],
            cells.Select(c => c.QuerySelector(".money")!.TextContent.Trim()));
        Assert.All(cells, c => Assert.NotNull(c.QuerySelector(".kind-icon")));
        // The total row renders after the 31 day rows: its icons are the last five.
        Assert.Equal(Kinds, table.FindComponents<KindIcon>().Skip(31 * 5).Select(i => i.Instance.Kind));
        Assert.Equal("", total.Children[6].TextContent.Trim());
        Assert.Empty(total.Children[6].Children);
    }

    // HOME-14: Saturday and Sunday dia cells get the weekend ground. 5 and 6 December 2026 are a Saturday and a Sunday.
    [Fact]
    public void WeekendDiaCellsAreTinted()
    {
        var table = RenderTable();

        var weekend = table.FindAll("tbody tr")
            .Where(r => r.Children[0].ClassList.Contains("day-cell--weekend"))
            .Select(r => int.Parse(r.QuerySelector(".day-number")!.TextContent, System.Globalization.CultureInfo.InvariantCulture));
        Assert.Equal([5, 6, 12, 13, 19, 20, 26, 27], weekend);
    }

    // HOME-15 (AD-004): today's row gets the today ground and an accent day number.
    [Fact]
    public void TodayRowIsMarked()
    {
        var table = RenderTable(today: new DateOnly(2026, 12, 10));

        var today = Assert.Single(table.FindAll("tbody tr.day-row--today"));
        Assert.Equal("10", today.QuerySelector(".day-number")!.TextContent.Trim());

        var none = RenderTable(today: new DateOnly(2026, 11, 10));
        Assert.Empty(none.FindAll("tbody tr.day-row--today"));
    }
}