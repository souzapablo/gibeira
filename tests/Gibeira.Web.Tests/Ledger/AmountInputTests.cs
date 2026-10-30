using System.Globalization;

using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Ledger;

public sealed class AmountInputTests
{
    private const string Previous = "7";

    // ADD-02: digits and one comma with at most two digits after it, up to 999.999.999,99.
    // ADD-20: the field can be emptied.
    [Theory]
    [InlineData("1", true)]
    [InlineData("12,", true)]
    [InlineData("12,3", true)]
    [InlineData("12,34", true)]
    [InlineData("999999999,99", true)]
    [InlineData("", true)]
    [InlineData("a", false)]
    [InlineData("12a", false)]
    [InlineData("1,2,", false)]
    [InlineData("12,345", false)]
    [InlineData("1000000000", false)]
    [InlineData("-1", false)]
    [InlineData("1.0", false)]
    [InlineData(",5", false)]
    [InlineData(" 1", false)]
    [InlineData("١٢", false)]
    public void AmountAcceptsDigitsAndOneCommaWithTwoDecimals(string proposed, bool accepted) =>
        Assert.Equal(accepted ? proposed : Previous, AmountInput.Accept(Previous, proposed));

    [Theory]
    [InlineData("1189,27", "1189.27")]
    [InlineData("12,", "12")]
    [InlineData("12,3", "12.3")]
    [InlineData("0,05", "0.05")]
    [InlineData("999999999,99", "999999999.99")]
    [InlineData("", "0")]
    public void ParseReadsCommaDecimals(string text, string expected) =>
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), AmountInput.Parse(text));
}