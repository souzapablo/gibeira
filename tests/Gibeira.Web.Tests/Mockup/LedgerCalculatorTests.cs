using Gibeira.Web.Ledger;
using Gibeira.Web.Mockup;

namespace Gibeira.Web.Tests.Mockup;

public sealed class LedgerCalculatorTests
{
    private static readonly DateOnly Today = new(2026, 10, 29);
    private static readonly Period October = Period.Starting(Today);

    private static Occurrence On(int year, int month, int day, Kind kind, decimal amount) =>
        new(new LedgerItem(Guid.NewGuid(), 1, new NewItem(kind, amount, "item", new DateOnly(year, month, day), new Repetition.None(), null, [])),
            new DateOnly(year, month, day), amount, null);

    private static DayRow Row(IReadOnlyList<MonthLedger> months, int year, int month, int day) =>
        months.Single(m => m.Month == new DateOnly(year, month, 1)).Days.Single(d => d.Date == new DateOnly(year, month, day));

    // HOME-10: saldo(d) = saldo(d-1) + entradas - saídas - diários - economias - cartão, from before the period and across months.
    [Fact]
    public void BalanceCarriesPreviousDayAcrossMonths()
    {
        Occurrence[] occurrences =
        [
            On(2026, 9, 15, Kind.Income, 1000m),
            On(2026, 10, 1, Kind.Expense, 200m),
            On(2026, 10, 31, Kind.DailySpending, 30m),
            On(2026, 11, 1, Kind.Savings, 100m),
            On(2026, 11, 1, Kind.CreditCard, 50m),
            On(2026, 11, 2, Kind.Income, 10m),
            On(2027, 1, 5, Kind.Expense, 1200m),
        ];

        var months = LedgerCalculator.Months(occurrences, October, Today);

        Assert.Equal(800m, Row(months, 2026, 10, 1).Balance);
        Assert.Equal(800m, Row(months, 2026, 10, 2).Balance);
        Assert.Equal(770m, Row(months, 2026, 10, 31).Balance);
        Assert.Equal(620m, Row(months, 2026, 11, 1).Balance);
        Assert.Equal(630m, Row(months, 2026, 11, 2).Balance);
        Assert.Equal(630m, Row(months, 2027, 1, 4).Balance);
        Assert.Equal(-570m, Row(months, 2027, 1, 5).Balance);
        Assert.Equal(-570m, Row(months, 2027, 9, 30).Balance);
    }

    // HOME-08, HOME-13: each cell is the day's sum for its kind, zero when none; the total row sums the month.
    [Fact]
    public void MonthTotalsSumEachKind()
    {
        Occurrence[] occurrences =
        [
            On(2026, 10, 3, Kind.Expense, 120.50m),
            On(2026, 10, 3, Kind.Expense, 79.50m),
            On(2026, 10, 20, Kind.Expense, 10m),
            On(2026, 10, 5, Kind.Income, 1189.27m),
            On(2026, 11, 3, Kind.Expense, 999m),
        ];

        var months = LedgerCalculator.Months(occurrences, October, Today);
        var october = months[0];

        Assert.Equal(200m, Row(months, 2026, 10, 3).Sums[Kind.Expense]);
        Assert.Equal(0m, Row(months, 2026, 10, 3).Sums[Kind.Income]);
        Assert.All(october.Days, d => Assert.Equal(Enum.GetValues<Kind>().Length, d.Sums.Count));
        Assert.Equal(1189.27m, october.Totals[Kind.Income]);
        Assert.Equal(210m, october.Totals[Kind.Expense]);
        Assert.Equal(0m, october.Totals[Kind.DailySpending]);
        Assert.Equal(0m, october.Totals[Kind.Savings]);
        Assert.Equal(0m, october.Totals[Kind.CreditCard]);
        Assert.Equal(999m, months[1].Totals[Kind.Expense]);
    }

    // HOME-07: twelve months in the period's order, one row per day.
    [Fact]
    public void MonthsHaveOneRowPerDayInOrder()
    {
        var months = LedgerCalculator.Months([], October, Today);

        Assert.Equal(October.Months, months.Select(m => m.Month));
        Assert.All(months, m => Assert.Equal(
            Enumerable.Range(1, DateTime.DaysInMonth(m.Month.Year, m.Month.Month)).Select(d => new DateOnly(m.Month.Year, m.Month.Month, d)),
            m.Days.Select(d => d.Date)));
    }

    // HOME-14, HOME-15: Saturdays and Sundays are weekend rows; only today's row is today.
    [Fact]
    public void DayRowsMarkWeekendsAndToday()
    {
        var months = LedgerCalculator.Months([], October, Today);
        var october = months[0].Days;

        Assert.Equal([3, 4, 10, 11, 17, 18, 24, 25, 31], october.Where(d => d.IsWeekend).Select(d => d.Date.Day));
        Assert.Equal(Today, Assert.Single(months.SelectMany(m => m.Days), d => d.IsToday).Date);
    }
}