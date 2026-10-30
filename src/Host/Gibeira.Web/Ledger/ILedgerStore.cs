namespace Gibeira.Web.Ledger;

// The page's only data access. A finance module implements it later; until then the mockup does.
public interface ILedgerStore
{
    IReadOnlyList<Card> Cards { get; }

    IReadOnlyList<string> Tags { get; }

    // Day rows with sums by kind, saldo and month totals.
    IReadOnlyList<MonthLedger> Months(Period period);

    // The day's entries in the order their items were added; every kind when filter is null.
    IReadOnlyList<DayEntry> Day(DateOnly day, Kind? filter);

    // The card's first due date on or after a date, stepped by months.
    DateOnly DueDate(Card card, DateOnly onOrAfter, int monthsAhead);

    void Add(NewItem item);

    // Removes that occurrence only.
    void Remove(Guid itemId, DateOnly day);
}

public abstract record Repetition
{
    public sealed record None : Repetition;

    public sealed record Monthly : Repetition;

    public sealed record Weekly : Repetition;

    public sealed record Daily : Repetition;

    // 2 to 48 monthly parts.
    public sealed record Installments(int Count) : Repetition;
}

public sealed record Card(string Name, int DueDay);

public sealed record NewItem(
    Kind Kind,
    decimal Amount,
    string Description,
    DateOnly Date,
    Repetition Repetition,
    string? Card,
    IReadOnlyList<string> Tags);

public sealed record DayRow(
    DateOnly Date,
    IReadOnlyDictionary<Kind, decimal> Sums,
    decimal Saldo,
    bool IsWeekend,
    bool IsToday);

public sealed record MonthLedger(DateOnly Month, IReadOnlyList<DayRow> Days, IReadOnlyDictionary<Kind, decimal> Totals);

public sealed record DayEntry(
    Guid ItemId,
    Kind Kind,
    decimal Amount,
    string Description,
    DateOnly Date,
    string? RepetitionLabel);