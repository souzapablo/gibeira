using Gibeira.Ledger.Domain;

namespace Gibeira.Ledger.Tests.Domain;

public sealed class RecurringRuleTests
{
    private static readonly DateOnly Start = new(2026, 10, 29);

    // REP-03: a rule's first entry falls on its first day.
    [Theory]
    [InlineData(Frequency.Monthly)]
    [InlineData(Frequency.Weekly)]
    [InlineData(Frequency.Daily)]
    public void FirstEntryFallsOnFirstDay(Frequency frequency)
    {
        var rule = Rule(Start, frequency);

        var entry = Assert.Single(rule.GenerateUpTo(Start));

        Assert.Equal(Start, entry.Date);
    }

    // REP-04: a monthly rule from 31/01 falls on 31/01, 28/02 (29/02 in a leap year), 31/03, 30/04, with no drift.
    [Theory]
    [InlineData(2026, 28)]
    [InlineData(2028, 29)]
    public void MonthlyFromDay31ClampsWithoutDrift(int year, int lastDayOfFebruary)
    {
        var rule = Rule(new DateOnly(year, 1, 31), Frequency.Monthly);

        var entries = rule.GenerateUpTo(new DateOnly(year, 4, 30));

        Assert.Equal(
            [new DateOnly(year, 1, 31), new DateOnly(year, 2, lastDayOfFebruary), new DateOnly(year, 3, 31), new DateOnly(year, 4, 30)],
            entries.Select(entry => entry.Date));
    }

    // REP-05: a weekly rule's entries are 7 days apart.
    [Fact]
    public void WeeklyStepsSevenDays()
    {
        var end = Start.AddYears(1);

        var entries = Rule(Start, Frequency.Weekly).GenerateUpTo(end);

        Assert.Equal(53, entries.Count);
        Assert.All(entries.Zip(entries.Skip(1)), pair => Assert.Equal(pair.First.Date.AddDays(7), pair.Second.Date));
        Assert.True(entries[^1].Date > end.AddDays(-7));
    }

    // REP-06: a daily rule's entries are 1 day apart.
    [Fact]
    public void DailyStepsOneDay()
    {
        var end = Start.AddYears(1);

        var entries = Rule(Start, Frequency.Daily).GenerateUpTo(end);

        Assert.Equal(end.DayNumber - Start.DayNumber + 1, entries.Count);
        Assert.All(entries.Zip(entries.Skip(1)), pair => Assert.Equal(pair.First.Date.AddDays(1), pair.Second.Date));
    }

    // REP-07: a generated entry holds the rule's kind, amount, description, card, tags and add order, and a Repeat origin.
    [Fact]
    public void GeneratedEntryCopiesRuleDetailsAndOrigin()
    {
        var card = CardId.New();
        var details = EntryDetails.Create(EntryKind.CreditCard, 39.90m, "Streaming", card, ["lazer", "casa"]);
        var rule = RecurringRule.Create(Start, details, Frequency.Weekly, addOrder: 4);

        var entries = rule.GenerateUpTo(Start.AddDays(14));

        Assert.Equal(3, entries.Count);
        Assert.Equal(3, entries.Select(entry => entry.Id).Distinct().Count());
        Assert.All(entries, entry =>
        {
            Assert.Equal(EntryKind.CreditCard, entry.Kind);
            Assert.Equal(39.90m, entry.Amount.Value);
            Assert.Equal("Streaming", entry.Description);
            Assert.Equal(card, entry.CardId);
            Assert.Equal(["lazer", "casa"], entry.Tags.Select(tag => tag.Name));
            Assert.Equal(4, entry.AddOrder);
            Assert.Equal(new EntryOrigin.Repeat(rule.Id, Frequency.Weekly), entry.Origin);
        });
    }

    // REP-08: generating the same date twice creates nothing new; a later date creates only the days after it.
    [Fact]
    public void GeneratingTheSameDateTwiceCreatesNothingNew()
    {
        var rule = Rule(Start, Frequency.Daily);

        Assert.Equal(5, rule.GenerateUpTo(Start.AddDays(4)).Count);
        Assert.Empty(rule.GenerateUpTo(Start.AddDays(4)));
        Assert.Empty(rule.GenerateUpTo(Start.AddDays(1)));
        Assert.Equal([Start.AddDays(5), Start.AddDays(6)], rule.GenerateUpTo(Start.AddDays(6)).Select(entry => entry.Date));
    }

    // REP-11: a date before the rule's first day creates no entry.
    [Fact]
    public void GeneratingBeforeFirstDayCreatesNothing()
    {
        var rule = Rule(Start, Frequency.Monthly);

        Assert.Empty(rule.GenerateUpTo(Start.AddDays(-1)));
        Assert.Equal(Start, Assert.Single(rule.GenerateUpTo(Start)).Date);
    }

    private static RecurringRule Rule(DateOnly firstDate, Frequency frequency) =>
        RecurringRule.Create(firstDate, EntryDetails.Create(EntryKind.Expense, 50m, "Aluguel", null, []), frequency, addOrder: 1);
}