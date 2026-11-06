namespace Gibeira.Ledger.Domain;

// Factory for parcelado: every part at once, one month apart, with no aggregate of its own.
public static class InstallmentPlan
{
    public const int MinParts = 2;
    public const int MaxParts = 48;

    public static IReadOnlyList<LedgerEntry> Create(DateOnly firstDate, EntryDetails details, int parts, long addOrder)
    {
        if (parts is < MinParts or > MaxParts)
        {
            throw new LedgerRuleException($"An installment plan has {MinParts} to {MaxParts} parts, not {parts}.");
        }

        var amounts = details.Amount.SplitInto(parts);
        var planId = InstallmentPlanId.New();

        // AddMonths from the first date clamps day 31 to the month's last day without drifting.
        return [.. amounts.Select((amount, index) => LedgerEntry.From(
            firstDate.AddMonths(index),
            details.WithAmount(amount),
            new EntryOrigin.Installment(planId, index + 1, parts),
            addOrder))];
    }
}