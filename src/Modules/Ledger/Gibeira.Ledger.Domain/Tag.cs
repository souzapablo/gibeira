namespace Gibeira.Ledger.Domain;

// A label on an entry, compared by its name.
public sealed record Tag
{
    private Tag(string name) => Name = name;

    public string Name { get; }

    public static Tag Of(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? throw new LedgerRuleException("A tag cannot be blank.")
            : new Tag(name);
}