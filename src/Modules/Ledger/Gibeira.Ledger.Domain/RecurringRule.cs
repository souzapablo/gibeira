using System.Diagnostics;

namespace Gibeira.Ledger.Domain;

// Aggregate root: creates the entries of a repetition with no end, one day at a time.
public sealed class RecurringRule
{
    // The index of the next entry to create. It only moves forward, so no day is created twice.
    private int _nextIndex;

    private RecurringRule(RecurringRuleId id, DateOnly firstDate, EntryDetails details, Frequency frequency, long addOrder)
    {
        Id = id;
        FirstDate = firstDate;
        Details = details;
        Frequency = frequency;
        AddOrder = addOrder;
    }

    public RecurringRuleId Id { get; }

    public DateOnly FirstDate { get; }

    public EntryDetails Details { get; }

    public Frequency Frequency { get; }

    public long AddOrder { get; }

    public static RecurringRule Create(DateOnly firstDate, EntryDetails details, Frequency frequency, long addOrder) =>
        new(RecurringRuleId.New(), firstDate, details, frequency, addOrder);

    // The entries from the rule's position up to the date, both included; none when it is already there.
    public IReadOnlyList<LedgerEntry> GenerateUpTo(DateOnly date)
    {
        var created = new List<LedgerEntry>();
        var origin = new EntryOrigin.Repeat(Id, Frequency);
        while (DateAt(_nextIndex) <= date)
        {
            created.Add(LedgerEntry.From(DateAt(_nextIndex), Details, origin, AddOrder));
            _nextIndex++;
        }

        return created;
    }

    // AddMonths from the first date clamps day 31 to the month's last day without drifting.
    private DateOnly DateAt(int index) => Frequency switch
    {
        Frequency.Monthly => FirstDate.AddMonths(index),
        Frequency.Weekly => FirstDate.AddDays(7 * index),
        Frequency.Daily => FirstDate.AddDays(index),
        _ => throw new UnreachableException($"Unknown frequency {Frequency}."),
    };
}