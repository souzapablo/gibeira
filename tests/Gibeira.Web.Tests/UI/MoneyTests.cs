using Bunit;

using Gibeira.Web.Components.UI;

namespace Gibeira.Web.Tests.UI;

public sealed class MoneyTests : BunitContext
{
    // HOME-08 and HOME-11: R$ 1.189,27 and R$ -477,67, in the text's own color.
    [Theory]
    [InlineData(1189.27, "R$ 1.189,27")]
    [InlineData(-477.67, "R$ -477,67")]
    public void MoneyRendersFormattedValue(double value, string expected)
    {
        var money = Render<Money>(parameters => parameters.Add(p => p.Value, (decimal)value)).Find(".money");

        Assert.Equal(expected, money.TextContent);
        Assert.False(money.HasAttribute("style"));
    }

    // HOME-09: a zero shows R$ 0,00 in the zero text color.
    [Fact]
    public void MutedMoneyUsesZeroColor()
    {
        var money = Render<Money>(parameters => parameters.Add(p => p.Value, 0m).Add(p => p.Muted, true)).Find(".money");

        Assert.Equal("R$ 0,00", money.TextContent);
        Assert.Equal("color: var(--text-zero)", money.GetAttribute("style"));
    }
}