using Gibeira.Web.Ledger;
using Gibeira.Web.Mockup;

namespace Gibeira.Web.Tests.Mockup;

public sealed class ScheduleTests
{
    private static LedgerItem Item(decimal amount, DateOnly date, Repetition repetition) =>
        new(Guid.NewGuid(), 1, new NewItem(Kind.Saida, amount, "aluguel", date, repetition, null, []));

    // ADD-10: the same day every month, the month's last day when that day does not exist.
    [Theory]
    [InlineData(2026, 28)]
    [InlineData(2028, 29)]
    public void MonthlyRepeatsOnSameDayClampedToMonthEnd(int year, int lastDayOfFebruary)
    {
        var item = Item(50m, new DateOnly(year, 1, 31), new Repetition.Monthly());

        var occurrences = Schedule.Expand(item, new DateOnly(year, 1, 1), new DateOnly(year, 4, 30)).ToList();

        Assert.Equal(
            [new DateOnly(year, 1, 31), new DateOnly(year, 2, lastDayOfFebruary), new DateOnly(year, 3, 31), new DateOnly(year, 4, 30)],
            occurrences.Select(o => o.Date));
        Assert.All(occurrences, o => Assert.Equal(50m, o.Amount));
        Assert.All(occurrences, o => Assert.Equal("mensalmente", o.RepetitionLabel));
    }

    // ADD-11: every 7 days and every day, with no end.
    [Fact]
    public void WeeklyAndDailyRepeatWithNoEnd()
    {
        var start = new DateOnly(2026, 10, 29);
        var end = new DateOnly(2036, 10, 29);

        var weekly = Schedule.Expand(Item(10m, start, new Repetition.Weekly()), start, end).ToList();
        var daily = Schedule.Expand(Item(10m, start, new Repetition.Daily()), start, end).ToList();

        Assert.Equal(start, weekly[0].Date);
        Assert.All(weekly.Zip(weekly.Skip(1)), pair => Assert.Equal(pair.First.Date.AddDays(7), pair.Second.Date));
        Assert.True(weekly[^1].Date > end.AddDays(-7));
        Assert.All(weekly, o => Assert.Equal("semanalmente", o.RepetitionLabel));

        Assert.Equal(end.DayNumber - start.DayNumber + 1, daily.Count);
        Assert.All(daily.Zip(daily.Skip(1)), pair => Assert.Equal(pair.First.Date.AddDays(1), pair.Second.Date));
        Assert.Equal(end, daily[^1].Date);
        Assert.All(daily, o => Assert.Equal("diariamente", o.RepetitionLabel));
    }

    // ADD-11: a window that starts years later still falls on the item's weekday.
    [Fact]
    public void WeeklyWindowStartingLaterKeepsTheWeekday()
    {
        var start = new DateOnly(2026, 10, 29);

        var weekly = Schedule.Expand(Item(10m, start, new Repetition.Weekly()), new DateOnly(2030, 1, 1), new DateOnly(2030, 1, 31)).ToList();

        Assert.Equal([new DateOnly(2030, 1, 3), new DateOnly(2030, 1, 10), new DateOnly(2030, 1, 17), new DateOnly(2030, 1, 24), new DateOnly(2030, 1, 31)],
            weekly.Select(o => o.Date));
    }

    // ADD-12: 100,00 in 3x is 33,34 + 33,33 + 33,33 in three months, labelled parcela 1/3 to 3/3.
    [Fact]
    public void InstallmentsSplitWithRemainderOnFirst()
    {
        var start = new DateOnly(2026, 10, 29);

        var parts = Schedule.Expand(Item(100m, start, new Repetition.Installments(3)), start, start.AddYears(5)).ToList();

        Assert.Equal([33.34m, 33.33m, 33.33m], parts.Select(o => o.Amount));
        Assert.Equal([start, new DateOnly(2026, 11, 29), new DateOnly(2026, 12, 29)], parts.Select(o => o.Date));
        Assert.Equal(["parcela 1/3", "parcela 2/3", "parcela 3/3"], parts.Select(o => o.RepetitionLabel));
    }

    // ADD-12 edge case: the parcels always sum to the amount.
    [Theory]
    [InlineData("100", 4, "25")]
    [InlineData("10", 3, "3.34")]
    [InlineData("0.05", 2, "0.03")]
    [InlineData("999999999.99", 48, "20833333.48")]
    public void InstallmentsSumToTheAmount(string amount, int count, string first)
    {
        var start = new DateOnly(2026, 1, 31);
        var value = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

        var parts = Schedule.Expand(Item(value, start, new Repetition.Installments(count)), start, start.AddYears(5)).ToList();

        Assert.Equal(count, parts.Count);
        Assert.Equal(value, parts.Sum(o => o.Amount));
        Assert.Equal(decimal.Parse(first, System.Globalization.CultureInfo.InvariantCulture), parts[0].Amount);
        Assert.All(parts.Skip(1), o => Assert.Equal(parts[1].Amount, o.Amount));
    }

    // DAY-04: an item that does not repeat falls on its day only and has no label.
    [Fact]
    public void NoRepetitionFallsOnItsDayOnly()
    {
        var day = new DateOnly(2026, 10, 29);

        var occurrence = Assert.Single(Schedule.Expand(Item(10m, day, new Repetition.None()), day.AddYears(-1), day.AddYears(1)));

        Assert.Equal(day, occurrence.Date);
        Assert.Null(occurrence.RepetitionLabel);
    }
}