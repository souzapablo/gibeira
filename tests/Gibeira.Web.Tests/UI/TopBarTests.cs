using Bunit;

using Gibeira.Web.Components.UI;

namespace Gibeira.Web.Tests.UI;

public sealed class TopBarTests : BunitContext
{
    // AD-004 and HOME-18: the gibeira logo and the "adicionar" button, which raises OnAdd.
    [Fact]
    public void TopBarAddRaisesOnAdd()
    {
        var adds = 0;
        var bar = Render<TopBar>(parameters => parameters.Add(p => p.OnAdd, () => adds++));

        Assert.Equal("gibeira", bar.Find(".top-bar .logo").TextContent.Trim());
        var add = bar.Find(".top-bar button.button--primary");
        Assert.Equal("adicionar", add.TextContent.Trim());
        Assert.Contains(bar.FindComponents<Icon>(), i => i.Instance.Name == IconName.Plus);

        add.Click();
        Assert.Equal(1, adds);
    }
}