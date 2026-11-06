namespace Gibeira.Ledger.Domain;

// Aggregate root: one complete record of one day. Equal by id; nothing changes after creation.
public sealed class LedgerEntry : IEquatable<LedgerEntry>
{
    private LedgerEntry(LedgerEntryId id, DateOnly date, EntryDetails details, EntryOrigin origin, long addOrder)
    {
        Id = id;
        Date = date;
        Kind = details.Kind;
        Amount = details.Amount;
        Description = details.Description;
        CardId = details.CardId;
        Tags = details.Tags;
        Origin = origin;
        AddOrder = addOrder;
    }

    public LedgerEntryId Id { get; }

    public DateOnly Date { get; }

    public EntryKind Kind { get; }

    public Money Amount { get; }

    public string Description { get; }

    public CardId? CardId { get; }

    public IReadOnlyList<Tag> Tags { get; }

    public EntryOrigin Origin { get; }

    // The place of the item that created it in the order of adding (ORD-01).
    public long AddOrder { get; }

    // Income adds to the balance; every other kind subtracts.
    public decimal BalanceEffect => Kind == EntryKind.Income ? Amount.Value : -Amount.Value;

    public static LedgerEntry OneOff(DateOnly date, EntryDetails details, long addOrder) =>
        new(LedgerEntryId.New(), date, details, new EntryOrigin.OneOff(), addOrder);

    public bool Equals(LedgerEntry? other) => other is not null && Id == other.Id;

    public override bool Equals(object? obj) => Equals(obj as LedgerEntry);

    public override int GetHashCode() => Id.GetHashCode();
}