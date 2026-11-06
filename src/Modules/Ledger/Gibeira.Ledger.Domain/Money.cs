namespace Gibeira.Ledger.Domain;

// An amount in R$: above zero, at most 999.999.999,99, at most two decimals.
public sealed record Money
{
    public const decimal Max = 999_999_999.99m;

    private const decimal Cent = 0.01m;

    private Money(decimal value) => Value = value;

    public decimal Value { get; }

    public static Money Of(decimal value)
    {
        if (value <= 0)
        {
            throw new LedgerRuleException($"An amount must be above zero, not {value}.");
        }

        if (value > Max)
        {
            throw new LedgerRuleException($"An amount must be at most {Max}, not {value}.");
        }

        if (decimal.Round(value, 2) != value)
        {
            throw new LedgerRuleException($"An amount has at most two decimals, not {value}.");
        }

        return new Money(value);
    }

    // At least R$ 0,01 per part.
    public static bool CanSplit(decimal value, int parts) => parts > 0 && value >= parts * Cent;

    // Each part is the amount divided by the parts, cut to cents; the first carries the remainder.
    public IReadOnlyList<Money> SplitInto(int parts)
    {
        if (!CanSplit(Value, parts))
        {
            throw new LedgerRuleException($"{Value} cannot be split into {parts} parts of at least {Cent}.");
        }

        var part = decimal.Floor(Value / parts * 100) / 100;
        var first = Value - (part * (parts - 1));
        return [new Money(first), .. Enumerable.Repeat(new Money(part), parts - 1)];
    }
}