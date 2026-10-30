using Bunit;

using Gibeira.Web.Components.UI;

namespace Gibeira.Web.Tests.UI;

public sealed class IconTests : BunitContext
{
    // A glyph without a label is decoration; with one it is an image named in pt-BR.
    [Fact]
    public void IconIsHiddenFromScreenReadersWithoutLabel()
    {
        var decorative = Render<Icon>(parameters => parameters.Add(p => p.Name, IconName.Close)).Find("svg");
        var labelled = Render<Icon>(parameters => parameters.Add(p => p.Name, IconName.Close).Add(p => p.Label, "fechar")).Find("svg");

        Assert.Equal("true", decorative.GetAttribute("aria-hidden"));
        Assert.False(decorative.HasAttribute("role"));
        Assert.False(decorative.HasAttribute("aria-label"));

        Assert.False(labelled.HasAttribute("aria-hidden"));
        Assert.Equal("img", labelled.GetAttribute("role"));
        Assert.Equal("fechar", labelled.GetAttribute("aria-label"));
    }
}