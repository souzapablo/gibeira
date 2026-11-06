namespace Gibeira.Ledger.Domain;

// Typed ids: one kind of id cannot be passed as another. Version 7 sorts by creation time.
public readonly record struct LedgerEntryId(Guid Value)
{
    public static LedgerEntryId New() => new(Guid.CreateVersion7());
}

public readonly record struct RecurringRuleId(Guid Value)
{
    public static RecurringRuleId New() => new(Guid.CreateVersion7());
}

public readonly record struct InstallmentPlanId(Guid Value)
{
    public static InstallmentPlanId New() => new(Guid.CreateVersion7());
}

public readonly record struct CardId(Guid Value)
{
    public static CardId New() => new(Guid.CreateVersion7());
}