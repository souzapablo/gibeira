using Gibeira.Ledger.Domain;

namespace Gibeira.Ledger.Tests.Domain;

public sealed class EntryDetailsTests
{
    private static readonly CardId Card = CardId.New();

    // ENT-07: an empty or all-spaces description is refused.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankDescriptionIsRefused(string description) =>
        Assert.Throws<LedgerRuleException>(() => EntryDetails.Create(EntryKind.Expense, 10m, description, null, []));

    // ENT-08: cartão with no card is refused.
    [Fact]
    public void CreditCardWithoutCardIsRefused() =>
        Assert.Throws<LedgerRuleException>(() => EntryDetails.Create(EntryKind.CreditCard, 10m, "Mercado", null, []));

    // ENT-09: entrada, saída, diário and economia with a card are each refused.
    [Theory]
    [InlineData(EntryKind.Income)]
    [InlineData(EntryKind.Expense)]
    [InlineData(EntryKind.DailySpending)]
    [InlineData(EntryKind.Savings)]
    public void CardOnOtherKindIsRefused(EntryKind kind) =>
        Assert.Throws<LedgerRuleException>(() => EntryDetails.Create(kind, 10m, "Mercado", Card, []));

    // ENT-10: an empty or all-spaces tag is refused.
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void BlankTagIsRefused(string tag) =>
        Assert.Throws<LedgerRuleException>(() => EntryDetails.Create(EntryKind.Expense, 10m, "Mercado", null, ["casa", tag]));

    // ENT-11: the same tag given twice is refused.
    [Fact]
    public void RepeatedTagIsRefused() =>
        Assert.Throws<LedgerRuleException>(() => EntryDetails.Create(EntryKind.Expense, 10m, "Mercado", null, ["casa", "lazer", "casa"]));

    // ENT-12: tags read back in the order they were given.
    [Fact]
    public void TagsKeepTheirGivenOrder()
    {
        var details = EntryDetails.Create(EntryKind.CreditCard, 10m, "Mercado", Card, ["lazer", "casa", "mercado"]);

        Assert.Equal(["lazer", "casa", "mercado"], details.Tags.Select(tag => tag.Name));
        Assert.Equal(Card, details.CardId);
    }
}