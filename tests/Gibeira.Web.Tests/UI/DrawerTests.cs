using Bunit;

using Gibeira.Web.Components.UI;

namespace Gibeira.Web.Tests.UI;

public sealed class DrawerTests : BunitContext
{
    // DAY-09: Esc closes the drawer, also when typed inside it; other keys do not.
    [Fact]
    public void DrawerClosesOnEsc()
    {
        var closes = 0;
        var drawer = Render<Drawer>(parameters => parameters
            .Add(p => p.Label, "10/nov")
            .Add(p => p.Header, "<h2>10/nov</h2>")
            .Add(p => p.OnClose, () => closes++)
            .AddChildContent("<button class=\"entry\">padaria</button>"));

        drawer.Find("[role=dialog]").KeyDown(key: "ArrowDown");
        Assert.Equal(0, closes);

        drawer.Find("[role=dialog]").KeyDown(key: "Escape");
        Assert.Equal(1, closes);

        drawer.Find(".entry").KeyDown(key: "Escape");
        Assert.Equal(2, closes);
    }

    // UI-06: the day panel is a dialog named by its label, holding the header and the content, and takes focus.
    [Fact]
    public void DrawerIsADialog()
    {
        var drawer = Render<Drawer>(parameters => parameters
            .Add(p => p.Label, "10/nov")
            .Add(p => p.Header, "<h2 class=\"day\">10/nov</h2>")
            .AddChildContent("<p class=\"empty\">nenhum lançamento</p>"));

        var dialog = drawer.Find("[role=dialog]");
        Assert.Equal("10/nov", dialog.GetAttribute("aria-label"));
        Assert.Equal("-1", dialog.GetAttribute("tabindex"));
        Assert.Equal("10/nov", dialog.QuerySelector(".drawer-header .day")!.TextContent);
        Assert.Equal("nenhum lançamento", dialog.QuerySelector(".drawer-body .empty")!.TextContent);
        JSInterop.VerifyFocusAsyncInvoke();
    }
}