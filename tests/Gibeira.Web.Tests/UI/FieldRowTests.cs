using Bunit;

using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.UI;

public sealed class FieldRowTests : BunitContext
{
    // Spec component table: icon, label, value, chevron; using the row raises OnClick.
    [Fact]
    public void FieldRowShowsLabelValueAndChevron()
    {
        var clicks = 0;
        var row = Render<FieldRow>(parameters => parameters
            .Add(p => p.Icon, builder =>
            {
                builder.OpenComponent<KindIcon>(0);
                builder.AddComponentParameter(1, nameof(KindIcon.Kind), Kind.Cartao);
                builder.CloseComponent();
            })
            .Add(p => p.Label, "vencimento")
            .Add(p => p.Value, "10/11/26")
            .Add(p => p.Chevron, true)
            .Add(p => p.OnClick, () => clicks++));

        var button = row.Find("button.field-row");
        Assert.Equal("vencimento", button.QuerySelector(".field-row-label")!.TextContent);
        Assert.Equal("10/11/26", button.QuerySelector(".field-row-value")!.TextContent);
        Assert.Equal(Kind.Cartao, row.FindComponent<KindIcon>().Instance.Kind);
        Assert.Equal(IconName.ChevronRight, Assert.Single(row.FindComponents<Icon>()).Instance.Name);

        button.Click();
        Assert.Equal(1, clicks);

        // No value: the label alone, as "tags" before any is checked; no chevron when not asked.
        var bare = Render<FieldRow>(parameters => parameters.Add(p => p.Label, "tags"));
        Assert.Equal("tags", bare.Find(".field-row-label").TextContent);
        Assert.Empty(bare.FindAll(".field-row-value"));
        Assert.Empty(bare.FindComponents<Icon>());

        // With content, the row holds it (the description field) instead of a button.
        var field = Render<FieldRow>(parameters => parameters
            .Add(p => p.Label, "descrição")
            .AddChildContent("<input class=\"description\" placeholder=\"entrada\" />"));
        Assert.Empty(field.FindAll("button"));
        Assert.NotNull(field.Find(".field-row .description"));
    }
}