using Gibeira.Web.Ledger;

namespace Gibeira.Web.Mockup;

// Temporary (AD-007): one per circuit, seeded on creation and gone on reload.
public sealed class InMemoryLedgerStore : ILedgerStore
{
    private readonly List<LedgerItem> items = [];
    private readonly HashSet<(Guid ItemId, DateOnly Day)> removed = [];
    private readonly TimeProvider time;

    public InMemoryLedgerStore(TimeProvider time)
    {
        this.time = time;
        foreach (var item in MockSeed.Create(Today))
        {
            Add(item);
        }
    }

    public IReadOnlyList<Card> Cards => MockSeed.Cards;

    public IReadOnlyList<string> Tags => MockSeed.Tags;

    private DateOnly Today => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

    // Every occurrence up to the period's last day, so the saldo carries in from before it.
    public IReadOnlyList<MonthLedger> Months(Period period)
    {
        var last = period.First.AddMonths(12).AddDays(-1);
        var occurrences = items.SelectMany(i => Schedule.Expand(i, DateOnly.MinValue, last)).Where(Kept).ToList();
        return LedgerCalculator.Months(occurrences, period, Today);
    }

    public IReadOnlyList<DayEntry> Day(DateOnly day, Kind? filter) =>
        items
            .Where(i => filter is null || i.Data.Kind == filter)
            .SelectMany(i => Schedule.Expand(i, day, day))
            .Where(Kept)
            .Select(o => new DayEntry(o.Item.Id, o.Item.Data.Kind, o.Amount, o.Item.Data.Description, o.Date, o.RepetitionLabel))
            .ToList();

    public DateOnly DueDate(Card card, DateOnly onOrAfter, int monthsAhead) => CardDueDates.DueDate(card, onOrAfter, monthsAhead);

    public void Add(NewItem item) => items.Add(new LedgerItem(Guid.NewGuid(), items.Count, item));

    public void Remove(Guid itemId, DateOnly day) => removed.Add((itemId, day));

    private bool Kept(Occurrence o) => !removed.Contains((o.Item.Id, o.Date));
}