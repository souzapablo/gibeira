using System.Globalization;

using Gibeira.Ledger.Domain;

namespace Gibeira.Ledger.Tests.Domain;

public sealed class MoneyTests
{
    // ENT-04: an amount of 0 or -1 is refused.
    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    public void MoneyRefusesZeroAndNegative(string value) =>
        Assert.Throws<LedgerRuleException>(() => Money.Of(Parse(value)));

    // ENT-05: 1.000.000.000,00 is refused.
    [Fact]
    public void MoneyRefusesAboveMax() =>
        Assert.Throws<LedgerRuleException>(() => Money.Of(1_000_000_000.00m));

    // ENT-05: 999.999.999,99 is accepted.
    [Fact]
    public void MoneyAcceptsMax()
    {
        Assert.Equal(999_999_999.99m, Money.Max);
        Assert.Equal(999_999_999.99m, Money.Of(999_999_999.99m).Value);
    }

    // ENT-06: an amount of 10,005 is refused.
    [Fact]
    public void MoneyRefusesThreeDecimals() =>
        Assert.Throws<LedgerRuleException>(() => Money.Of(10.005m));

    // INS-02: each part is the amount divided by N cut to cents; the first carries the remainder.
    [Fact]
    public void SplitGivesFirstPartTheRemainder() =>
        Assert.Equal([33.34m, 33.33m, 33.33m], Money.Of(100m).SplitInto(3).Select(part => part.Value));

    // INS-02 edge case: the parts always sum to the amount; 100/6 and 10/6 are where cutting and rounding differ.
    [Theory]
    [InlineData("100", 4, "25")]
    [InlineData("10", 3, "3.34")]
    [InlineData("0.05", 2, "0.03")]
    [InlineData("999999999.99", 48, "20833333.48")]
    [InlineData("100", 6, "16.70")]
    [InlineData("10", 6, "1.70")]
    public void SplitPartsSumToTheAmount(string amount, int parts, string first)
    {
        var value = Parse(amount);

        var split = Money.Of(value).SplitInto(parts);

        Assert.Equal(parts, split.Count);
        Assert.Equal(value, split.Sum(part => part.Value));
        Assert.Equal(Parse(first), split[0].Value);
        Assert.All(split.Skip(1), part => Assert.Equal(split[1].Value, part.Value));
    }

    // INS-05: a split needs at least R$ 0,01 per part.
    [Fact]
    public void CanSplitNeedsOneCentPerPart()
    {
        Assert.True(Money.CanSplit(0.02m, 2));
        Assert.False(Money.CanSplit(0.01m, 2));
        Assert.Throws<LedgerRuleException>(() => Money.Of(0.01m).SplitInto(2));
    }

    private static decimal Parse(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);
}