namespace Gibeira.Ledger.Domain;

// Where an entry came from: added on its own, created by a recurring rule, or one part of an installment plan.
public abstract record EntryOrigin
{
    public sealed record OneOff : EntryOrigin;

    public sealed record Repeat(RecurringRuleId RuleId, Frequency Frequency) : EntryOrigin;

    // Part `Number` of `Count`, numbered from 1.
    public sealed record Installment(InstallmentPlanId PlanId, int Number, int Count) : EntryOrigin;
}