using Bunit;

using Gibeira.Web.Components.Home;
using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Home;

public sealed class MonthTableTests : BunitContext
{
    private static readonly DateOnly December = new(2026, 12, 1);

    private static readonly Kind[] Kinds = [Kind.Income, Kind.Expense, Kind.DailySpending, Kind.Savings, Kind.CreditCard];

    // December 2026: an entrada of 1.189,27 on the 10th, a saldo that turns negative on the 15th and is zero on the 20th.
    private static MonthLedger Ledger(DateOnly? today = null)
    {
        var days = Enumerable.Range(1, 31).Select(day =>
        {
            var date = new DateOnly(2026, 12, day);
            var sums = Kinds.ToDictionary(kind => kind, _ => 0m);
            if (day == 10)
            {
                sums[Kind.Income] = 1189.27m;
                sums[Kind.CreditCard] = 250m;
            }

            var balance = day switch { < 15 => 500m, 20 => 0m, _ => -477.67m };
            return new DayRow(date, sums, balance, date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday, date == today);
        }).ToList();

        var totals = new Dictionary<Kind, decimal>
        {
            [Kind.Income] = 15500m,
            [Kind.Expense] = 3210.5m,
            [Kind.DailySpending] = 0m,
            [Kind.Savings] = 800m,
            [Kind.CreditCard] = 1189.27m,
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

        var expense = row.QuerySelectorAll("td.money-cell")[1];
        Assert.Equal("R$ 0,00", expense.QuerySelector(".money")!.TextContent.Trim());
        Assert.Contains("var(--text-zero)", expense.QuerySelector(".money")!.GetAttribute("style"));
        Assert.Contains("var(--text-zero)", expense.QuerySelector(".kind-icon")!.GetAttribute("style"));

        var income = row.QuerySelectorAll("td.money-cell")[0];
        Assert.DoesNotContain("var(--text-zero)", income.QuerySelector(".money")!.GetAttribute("style") ?? "");
        Assert.Contains("var(--kind-income)", income.QuerySelector(".kind-icon")!.GetAttribute("style"));
    }

    // HOME-12 (AD-003): a saldo of zero or more gets the positive tint, a negative one the negative tint.
    [Fact]
    public void BalanceCellTintFollowsSign()
    {
        var table = RenderTable();

        var positive = Row(table, 1).QuerySelector("td.balance-cell")!;
        Assert.Contains("balance-cell--positive", positive.ClassList);
        Assert.Equal("R$ 500,00", positive.TextContent.Trim());

        var zero = Row(table, 20).QuerySelector("td.balance-cell")!;
        Assert.Contains("balance-cell--positive", zero.ClassList);
        Assert.DoesNotContain("balance-cell--negative", zero.ClassList);

        var negative = Row(table, 15).QuerySelector("td.balance-cell")!;
        Assert.Contains("balance-cell--negative", negative.ClassList);
        Assert.DoesNotContain("balance-cell--positive", negative.ClassList);
        Assert.Equal("R$ -477,67", negative.TextContent.Trim());
    }

    // HOME-13: the total row holds each kind's month sum with its icon, and an empty saldos cell.
    [Fact]
    public void TotalRowHoldsMonthSumsAndEmptyBalance()
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
    public void WeekendDayCellsAreTinted()
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

    // HOME-19: each kind's header starts with a dot in the kind's color; dia and saldos have none.
    [Fact]
    public void KindHeadersStartWithTheirDot()
    {
        var heads = RenderTable().FindAll("thead th");

        Assert.Equal(
            Kinds.Select(KindColor.Var),
            heads.Skip(1).Take(5).Select(th => th.QuerySelector(".head-content > .kind-dot:first-child")!.GetAttribute("style")!.Replace("background:", "").Trim()));
        Assert.Null(heads[0].QuerySelector(".kind-dot"));
        Assert.Null(heads[6].QuerySelector(".kind-dot"));
    }

    // HOME-20: each dia cell shows the pt-BR weekday after the day number; December 2026 starts on a terça.
    [Fact]
    public void DayCellShowsWeekdayAfterDayNumber()
    {
        var table = RenderTable();
        string[] week = ["ter", "qua", "qui", "sex", "sáb", "dom", "seg"];

        Assert.Equal(
            Enumerable.Range(0, 31).Select(i => week[i % 7]),
            Enumerable.Range(1, 31).Select(day => Row(table, day).QuerySelector(".day-number + .weekday")!.TextContent.Trim()));
    }

    // HOME-21: today's dia cell ends with the hoje badge; no other row has one.
    [Fact]
    public void TodayRowShowsTodayBadge()
    {
        var table = RenderTable(today: new DateOnly(2026, 12, 10));

        var badge = Assert.Single(table.FindAll(".today-badge"));
        Assert.Equal("hoje", badge.TextContent.Trim());
        Assert.Same(badge, Row(table, 10).QuerySelector(".weekday + .today-badge"));

        Assert.Empty(RenderTable(today: new DateOnly(2026, 11, 10)).FindAll(".today-badge"));
    }
}