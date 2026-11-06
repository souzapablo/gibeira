namespace Gibeira.Ledger.Domain;

// Aggregate root: a credit card and the day its bill falls due.
public sealed class Card
{
    private Card(CardId id, string name, int dueDay)
    {
        Id = id;
        Name = name;
        DueDay = dueDay;
    }

    public CardId Id { get; }

    public string Name { get; }

    public int DueDay { get; }

    public static Card Create(string name, int dueDay)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new LedgerRuleException("A card name cannot be blank.");
        }

        if (dueDay is < 1 or > 31)
        {
            throw new LedgerRuleException($"A due day is from 1 to 31, not {dueDay}.");
        }

        return new Card(CardId.New(), name, dueDay);
    }

    // The first due date on or after a date, moved by monthsAhead months.
    public DateOnly DueDate(DateOnly onOrAfter, int monthsAhead)
    {
        var month = new DateOnly(onOrAfter.Year, onOrAfter.Month, 1);
        if (DueIn(month) < onOrAfter)
        {
            month = month.AddMonths(1);
        }

        return DueIn(month.AddMonths(monthsAhead));
    }

    // The due day, or the month's last day when the month is shorter.
    private DateOnly DueIn(DateOnly month) =>
        new(month.Year, month.Month, Math.Min(DueDay, DateTime.DaysInMonth(month.Year, month.Month)));
}