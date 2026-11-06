namespace Gibeira.Ledger.Domain;

// Every rule an entry shares with the rule that creates it, so neither can be built from invalid details.
public sealed class EntryDetails
{
    private EntryDetails(EntryKind kind, Money amount, string description, CardId? cardId, IReadOnlyList<Tag> tags)
    {
        Kind = kind;
        Amount = amount;
        Description = description;
        CardId = cardId;
        Tags = tags;
    }

    public EntryKind Kind { get; }

    public Money Amount { get; }

    public string Description { get; }

    public CardId? CardId { get; }

    public IReadOnlyList<Tag> Tags { get; }

    public static EntryDetails Create(EntryKind kind, decimal amount, string description, CardId? cardId, IEnumerable<string> tags)
    {
        var money = Money.Of(amount);

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new LedgerRuleException("A description cannot be blank.");
        }

        if (kind == EntryKind.CreditCard && cardId is null)
        {
            throw new LedgerRuleException("A credit card entry needs a card.");
        }

        if (kind != EntryKind.CreditCard && cardId is not null)
        {
            throw new LedgerRuleException($"A {kind} entry cannot have a card.");
        }

        var given = tags.Select(Tag.Of).ToArray();
        var repeated = given.GroupBy(tag => tag).FirstOrDefault(group => group.Count() > 1);
        if (repeated is not null)
        {
            throw new LedgerRuleException($"The tag '{repeated.Key.Name}' is given twice.");
        }

        return new EntryDetails(kind, money, description, cardId, given);
    }
}