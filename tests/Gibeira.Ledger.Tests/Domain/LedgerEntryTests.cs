using System.Reflection;

using Gibeira.Ledger.Domain;

namespace Gibeira.Ledger.Tests.Domain;

public sealed class LedgerEntryTests
{
    private static readonly DateOnly Day = new(2026, 10, 29);

    // ENT-01: an entry reads back its id, day, type, amount, description, card id on cartão, tags and origin.
    [Fact]
    public void EntryReadsBackComplete()
    {
        var card = CardId.New();
        var details = EntryDetails.Create(EntryKind.CreditCard, 250.50m, "Geladeira", card, ["casa", "lazer"]);

        var entry = LedgerEntry.OneOff(Day, details, addOrder: 7);

        Assert.NotEqual(Guid.Empty, entry.Id.Value);
        Assert.Equal(Day, entry.Date);
        Assert.Equal(EntryKind.CreditCard, entry.Kind);
        Assert.Equal(250.50m, entry.Amount.Value);
        Assert.Equal("Geladeira", entry.Description);
        Assert.Equal(card, entry.CardId);
        Assert.Equal(["casa", "lazer"], entry.Tags.Select(tag => tag.Name));
        Assert.IsType<EntryOrigin.OneOff>(entry.Origin);
        Assert.Equal(7, entry.AddOrder);
    }

    // ENT-02: two entries with equal values and different ids differ; an entry equals itself.
    [Fact]
    public void EntriesAreEqualOnlyByIdentity()
    {
        var details = EntryDetails.Create(EntryKind.Expense, 10m, "Padaria", null, []);
        var first = LedgerEntry.OneOff(Day, details, 1);
        var second = LedgerEntry.OneOff(Day, details, 1);

        Assert.NotEqual(first.Id, second.Id);
        Assert.False(first.Equals(second));
        Assert.False(first.Equals((object)second));
        Assert.True(first.Equals(first));
        Assert.True(first.Equals((object)first));
        Assert.Equal(first.GetHashCode(), first.GetHashCode());
    }

    // ENT-03: no public setter, no public mutable field, no public method other than queries, and tags that cannot be changed.
    [Fact]
    public void EntryHasNoMutatingMember()
    {
        const BindingFlags Public = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        var type = typeof(LedgerEntry);

        Assert.All(type.GetProperties(Public), property => Assert.Null(property.GetSetMethod()));
        Assert.All(type.GetFields(Public), field => Assert.True(field.IsInitOnly || field.IsLiteral, field.Name));

        string[] queries = ["Equals", "GetHashCode", "ToString", "GetType"];
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method => !method.IsSpecialName);
        Assert.All(methods, method => Assert.Contains(method.Name, queries));

        var entry = LedgerEntry.OneOff(Day, EntryDetails.Create(EntryKind.Expense, 10m, "Padaria", null, ["casa"]), 1);
        Assert.Throws<NotSupportedException>(() => ((IList<Tag>)entry.Tags)[0] = Tag.Of("lazer"));
    }

    // ENT-13: an entrada of R$ 100,00 has a balance effect of +100,00.
    [Fact]
    public void IncomeAddsToBalance() =>
        Assert.Equal(100.00m, LedgerEntry.OneOff(Day, EntryDetails.Create(EntryKind.Income, 100m, "Salário", null, []), 1).BalanceEffect);

    // ENT-14: saída, diário, economia and cartão of R$ 100,00 each have a balance effect of -100,00.
    [Theory]
    [InlineData(EntryKind.Expense)]
    [InlineData(EntryKind.DailySpending)]
    [InlineData(EntryKind.Savings)]
    [InlineData(EntryKind.CreditCard)]
    public void OtherKindsSubtractFromBalance(EntryKind kind)
    {
        CardId? card = kind == EntryKind.CreditCard ? CardId.New() : null;

        var entry = LedgerEntry.OneOff(Day, EntryDetails.Create(kind, 100m, "Conta", card, []), 1);

        Assert.Equal(-100.00m, entry.BalanceEffect);
    }
}