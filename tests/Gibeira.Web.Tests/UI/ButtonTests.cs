using Bunit;

using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.UI;

public sealed class ButtonTests : BunitContext
{
    // Primary, secondary, light ("salvar") and kind-colored ("adicionar cartão") (spec component table).
    [Theory]
    [InlineData(ButtonVariant.Primary, "button--primary")]
    [InlineData(ButtonVariant.Secondary, "button--secondary")]
    [InlineData(ButtonVariant.Light, "button--light")]
    public void ButtonVariantsSetTheirClass(ButtonVariant variant, string expected)
    {
        var button = Render<Button>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("salvar")).Find("button");

        Assert.Equal(["button", expected], button.ClassList);
        Assert.False(button.HasAttribute("style"));
        Assert.Equal("salvar", button.TextContent.Trim());
    }

    [Theory]
    [InlineData(Kind.Entrada, "var(--kind-entrada)")]
    [InlineData(Kind.Cartao, "var(--kind-cartao)")]
    public void KindButtonTakesItsKindColor(Kind kind, string color)
    {
        var button = Render<Button>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Kind)
            .Add(p => p.Kind, kind)
            .AddChildContent("adicionar cartão")).Find("button");

        Assert.Equal(["button", "button--kind"], button.ClassList);
        Assert.Equal($"--button-color: {color}", button.GetAttribute("style"));
    }

    [Fact]
    public void DisabledButtonDoesNotFireClick()
    {
        var clicks = 0;
        var enabled = Render<Button>(parameters => parameters.Add(p => p.OnClick, () => clicks++));
        var disabled = Render<Button>(parameters => parameters.Add(p => p.OnClick, () => clicks++).Add(p => p.Disabled, true));
        var enabledIcon = Render<IconButton>(parameters => parameters
            .Add(p => p.Icon, IconName.Plus).Add(p => p.Label, "adicionar").Add(p => p.OnClick, () => clicks++));
        var disabledIcon = Render<IconButton>(parameters => parameters
            .Add(p => p.Icon, IconName.Plus).Add(p => p.Label, "adicionar").Add(p => p.OnClick, () => clicks++).Add(p => p.Disabled, true));

        disabled.Find("button").Click();
        disabledIcon.Find("button").Click();
        Assert.Equal(0, clicks);
        Assert.True(disabled.Find("button").HasAttribute("disabled"));
        Assert.True(disabledIcon.Find("button").HasAttribute("disabled"));

        enabled.Find("button").Click();
        enabledIcon.Find("button").Click();
        Assert.Equal(2, clicks);
        Assert.False(enabled.Find("button").HasAttribute("disabled"));
    }

    // UI-06: an icon-only control is named in pt-BR, and its glyph is not read twice.
    [Fact]
    public void IconButtonHasAriaLabel()
    {
        var button = Render<IconButton>(parameters => parameters
            .Add(p => p.Icon, IconName.ChevronLeft)
            .Add(p => p.Label, "mês anterior"));

        Assert.Equal("mês anterior", button.Find("button").GetAttribute("aria-label"));
        Assert.Equal("true", button.Find("svg").GetAttribute("aria-hidden"));
        Assert.Equal(IconName.ChevronLeft, button.FindComponent<Icon>().Instance.Name);
        Assert.Throws<InvalidOperationException>(() => Render<IconButton>(parameters => parameters.Add(p => p.Icon, IconName.Close)));
    }
}