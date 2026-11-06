using Gibeira.Ledger.Domain;

namespace Gibeira.Ledger.Tests.Domain;

public sealed class InstallmentPlanTests
{
    private static readonly DateOnly Start = new(2026, 1, 31);

    // INS-01: parcelado em 3x from 31/01 creates 3 entries on 31/01, 28/02, 31/03, each a complete record.
    [Fact]
    public void PlanCreatesOneEntryPerMonthClamped()
    {
        var card = CardId.New();
        var details = EntryDetails.Create(EntryKind.CreditCard, 90m, "Geladeira", card, ["casa"]);

        var entries = InstallmentPlan.Create(Start, details, 3, addOrder: 5);

        Assert.Equal([new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 31)], entries.Select(entry => entry.Date));
        Assert.Equal(3, entries.Select(entry => entry.Id).Distinct().Count());
        Assert.All(entries, entry =>
        {
            Assert.Equal(EntryKind.CreditCard, entry.Kind);
            Assert.Equal("Geladeira", entry.Description);
            Assert.Equal(card, entry.CardId);
            Assert.Equal(["casa"], entry.Tags.Select(tag => tag.Name));
            Assert.Equal(5, entry.AddOrder);
        });
    }

    // INS-02: R$ 100,00 in 3x gives R$ 33,34, R$ 33,33, R$ 33,33.
    [Fact]
    public void PlanOfOneHundredInThreeSplitsWithRemainderOnFirst() =>
        Assert.Equal([33.34m, 33.33m, 33.33m], InstallmentPlan.Create(Start, Details(100m), 3, 1).Select(entry => entry.Amount.Value));

    // INS-03: the parts share one plan id and are numbered 1 to N of N; 2 and 48 are the bounds.
    [Theory]
    [InlineData(2)]
    [InlineData(10)]
    [InlineData(48)]
    public void PartsShareOnePlanIdAndNumberOneToN(int parts)
    {
        var entries = InstallmentPlan.Create(Start, Details(100m), parts, 1);

        var origins = entries.Select(entry => Assert.IsType<EntryOrigin.Installment>(entry.Origin)).ToList();
        Assert.Single(origins.Select(origin => origin.PlanId).Distinct());
        Assert.Equal(Enumerable.Range(1, parts), origins.Select(origin => origin.Number));
        Assert.All(origins, origin => Assert.Equal(parts, origin.Count));
    }

    // INS-04: 1 and 49 parts are refused.
    [Theory]
    [InlineData(1)]
    [InlineData(49)]
    public void PartCountOutsideTwoToFortyEightIsRefused(int parts) =>
        Assert.Throws<LedgerRuleException>(() => InstallmentPlan.Create(Start, Details(100m), parts, 1));

    // INS-05: R$ 0,01 in 2x is refused.
    [Fact]
    public void AmountBelowOneCentPerPartIsRefused() =>
        Assert.Throws<LedgerRuleException>(() => InstallmentPlan.Create(Start, Details(0.01m), 2, 1));

    private static EntryDetails Details(decimal amount) =>
        EntryDetails.Create(EntryKind.Expense, amount, "Curso", null, []);
}