using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Ledger;

public sealed class PeriodTests
{
    private static readonly DateOnly Today = new(2026, 10, 29);

    // HOME-02: out/2026 - set/2027 when today is 2026-10-29.
    [Theory]
    [InlineData(2026, 10, 29, "out/2026 - set/2027")]
    [InlineData(2026, 1, 1, "jan/2026 - dez/2026")]
    [InlineData(2026, 2, 15, "fev/2026 - jan/2027")]
    public void PeriodLabelUsesPtBrAbbrevs(int year, int month, int day, string expected) =>
        Assert.Equal(expected, Period.Starting(new DateOnly(year, month, day)).Label);

    // HOME-03 and HOME-04: one month and twelve months each way.
    [Theory]
    [InlineData(1, "nov/2026 - out/2027")]
    [InlineData(-1, "set/2026 - ago/2027")]
    [InlineData(3, "jan/2027 - dez/2027")]
    [InlineData(-10, "dez/2025 - nov/2026")]
    [InlineData(12, "out/2027 - set/2028")]
    [InlineData(-12, "out/2025 - set/2026")]
    public void ShiftMovesByMonthsAcrossYears(int months, string expected) =>
        Assert.Equal(expected, Period.Starting(Today).Shift(months).Label);

    [Fact]
    public void ShiftBackAndForthIsTheSamePeriod() =>
        Assert.Equal(Period.Starting(Today), Period.Starting(Today).Shift(5).Shift(-5));

    // HOME-05: the 25 periods from 12 months before to 12 months after the current month.
    [Fact]
    public void ChoicesSpanTwelveMonthsEachWay()
    {
        var choices = Period.Choices(Today);

        Assert.Equal(25, choices.Count);
        Assert.Equal("out/2025 - set/2026", choices[0].Label);
        Assert.Equal(Period.Starting(Today), choices[12]);
        Assert.Equal("out/2027 - set/2028", choices[24].Label);
        for (var i = 1; i < choices.Count; i++)
        {
            Assert.Equal(choices[i - 1].Shift(1), choices[i]);
        }
    }

    // HOME-07: twelve month tables in the period's order.
    [Fact]
    public void MonthsAreTheTwelveFirstDaysFromTheStart()
    {
        var months = Period.Starting(Today).Months.ToList();

        Assert.Equal(12, months.Count);
        Assert.Equal(new DateOnly(2026, 10, 1), months[0]);
        Assert.Equal(new DateOnly(2027, 1, 1), months[3]);
        Assert.Equal(new DateOnly(2027, 9, 1), months[11]);
        Assert.All(months, m => Assert.Equal(1, m.Day));
    }
}