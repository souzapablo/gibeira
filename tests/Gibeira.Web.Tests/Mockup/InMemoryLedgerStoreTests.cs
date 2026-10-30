using Gibeira.Web.Ledger;
using Gibeira.Web.Mockup;

namespace Gibeira.Web.Tests.Mockup;

public sealed class InMemoryLedgerStoreTests
{
    private static readonly DateOnly Today = new(2026, 10, 29);
    private static readonly DateOnly Day = new(2026, 11, 3);

    private static InMemoryLedgerStore Store(DateOnly today) => new(new FixedTime(today));

    private static NewItem Item(Kind kind, decimal amount, string description, Repetition repetition) =>
        new(kind, amount, description, Day, repetition, null, []);

    private static DayRow Row(InMemoryLedgerStore store, DateOnly date) =>
        store.Months(Period.Starting(Today)).SelectMany(m => m.Days).Single(d => d.Date == date);

    // MOCK-01: every kind has a non-zero day this month, a saldo goes negative within the 12 months, the same on every load.
    [Theory]
    [InlineData(2026, 10, 29)]
    [InlineData(2026, 12, 31)]
    [InlineData(2027, 1, 31)]
    [InlineData(2026, 3, 31)]
    [InlineData(2028, 2, 29)]
    [InlineData(2026, 7, 1)]
    public void SeedHasEveryKindThisMonthAndANegativeSaldo(int year, int month, int day)
    {
        var today = new DateOnly(year, month, day);

        var months = Store(today).Months(Period.Starting(today));

        Assert.All(Enum.GetValues<Kind>(), kind => Assert.Contains(months[0].Days, d => d.Sums[kind] > 0));
        Assert.Contains(months.SelectMany(m => m.Days), d => d.Saldo < 0);
        Assert.Equal(
            months.SelectMany(m => m.Days).Select(d => d.Saldo),
            Store(today).Months(Period.Starting(today)).SelectMany(m => m.Days).Select(d => d.Saldo));
    }

    // MOCK-01: cartão items fall on both cards' due days.
    [Fact]
    public void SeedBooksCartaoOnBothCardsDueDays()
    {
        var store = Store(Today);

        Assert.Equal([new Card("Itaú", 10), new Card("Santander", 6)], store.Cards);
        Assert.NotEmpty(store.Day(new DateOnly(2026, 10, 10), Kind.Cartao));
        Assert.NotEmpty(store.Day(new DateOnly(2026, 10, 6), Kind.Cartao));
    }

    // DAY-12: excluir removes that day's occurrence only, and that day's cell and every later saldo follow.
    [Fact]
    public void RemoveDropsOnlyThatOccurrence()
    {
        var store = Store(Today);
        store.Add(Item(Kind.Saida, 100m, "academia", new Repetition.Monthly()));
        var before = Row(store, Day);
        var laterBefore = Row(store, new DateOnly(2027, 2, 1));
        var earlierBefore = Row(store, new DateOnly(2026, 11, 2));
        var id = store.Day(Day, Kind.Saida).Single(e => e.Description == "academia").ItemId;

        store.Remove(id, Day);

        Assert.DoesNotContain(store.Day(Day, null), e => e.ItemId == id);
        Assert.NotEmpty(store.Day(Day, Kind.Diario));
        Assert.Contains(store.Day(new DateOnly(2026, 12, 3), null), e => e.ItemId == id);
        Assert.Equal(before.Sums[Kind.Saida] - 100m, Row(store, Day).Sums[Kind.Saida]);
        Assert.Equal(before.Saldo + 100m, Row(store, Day).Saldo);
        Assert.Equal(laterBefore.Saldo + 100m, Row(store, new DateOnly(2027, 2, 1)).Saldo);
        Assert.Equal(earlierBefore.Saldo, Row(store, new DateOnly(2026, 11, 2)).Saldo);
    }

    // DAY-04, DAY-07: the day's entries in the order their items were added, with amount, date and repetition label.
    [Fact]
    public void DayListsEntriesInAddedOrder()
    {
        var store = Store(Today);
        var seeded = store.Day(Day, null).Count;
        store.Add(Item(Kind.Saida, 20m, "b", new Repetition.None()));
        store.Add(Item(Kind.Entrada, 30m, "a", new Repetition.None()));
        store.Add(Item(Kind.Saida, 40m, "c", new Repetition.Monthly()));

        var all = store.Day(Day, null);

        Assert.Equal(["b", "a", "c"], all.Skip(seeded).Select(e => e.Description));
        var c = all[^1];
        Assert.Equal((Kind.Saida, 40m, Day, "mensalmente"), (c.Kind, c.Amount, c.Date, c.RepetitionLabel));
        Assert.Null(all[^3].RepetitionLabel);
        Assert.Equal(["b", "c"], store.Day(Day, Kind.Saida).Select(e => e.Description));
        Assert.Equal(["c"], store.Day(new DateOnly(2026, 12, 3), Kind.Saida).Select(e => e.Description));
    }
}