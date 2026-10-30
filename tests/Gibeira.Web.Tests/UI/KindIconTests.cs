using Bunit;

using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.UI;

public sealed class KindIconTests : BunitContext
{
    // Arrow in, arrow out, D, E, C, each on its kind color (spec component table).
    public static TheoryData<Kind, string, IconName?, string> Badges => new()
    {
        { Kind.Entrada, "var(--kind-entrada)", IconName.ArrowIn, "" },
        { Kind.Saida, "var(--kind-saida)", IconName.ArrowOut, "" },
        { Kind.Diario, "var(--kind-diario)", null, "D" },
        { Kind.Economia, "var(--kind-economia)", null, "E" },
        { Kind.Cartao, "var(--kind-cartao)", null, "C" },
    };

    [Theory]
    [MemberData(nameof(Badges))]
    public void KindIconShowsGlyphAndKindColor(Kind kind, string color, IconName? glyph, string letter)
    {
        var badge = Render<KindIcon>(parameters => parameters.Add(p => p.Kind, kind));
        var muted = Render<KindIcon>(parameters => parameters.Add(p => p.Kind, kind).Add(p => p.Muted, true));

        Assert.Equal($"background: {color}", badge.Find(".kind-icon").GetAttribute("style"));
        Assert.Equal("background: var(--text-zero)", muted.Find(".kind-icon").GetAttribute("style"));
        foreach (var rendered in new[] { badge, muted })
        {
            Assert.Equal(letter, rendered.Find(".kind-icon").TextContent.Trim());
            Assert.Equal(glyph, rendered.FindComponents<Icon>().SingleOrDefault()?.Instance.Name);
        }
    }
}