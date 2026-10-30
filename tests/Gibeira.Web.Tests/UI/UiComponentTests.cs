using Bunit;

using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.UI;

public sealed class UiComponentTests
{
    // One row per component of the spec's component table, each rendered from its parameters only.
    private static readonly Dictionary<string, Func<BunitContext, string>> Renderers = new()
    {
        ["Icon"] = context => context.Render<Icon>(parameters => parameters.Add(p => p.Name, IconName.Plus)).Markup,
        ["KindIcon"] = context => context.Render<KindIcon>(parameters => parameters.Add(p => p.Kind, Kind.Cartao)).Markup,
        ["Money"] = context => context.Render<Money>(parameters => parameters.Add(p => p.Value, 1189.27m)).Markup,
        ["Button"] = context => context.Render<Button>(parameters => parameters.Add(p => p.Variant, ButtonVariant.Primary).AddChildContent("salvar")).Markup,
        ["IconButton"] = context => context.Render<IconButton>(parameters => parameters.Add(p => p.Icon, IconName.Close).Add(p => p.Label, "fechar")).Markup,
        ["Dropdown"] = context => context.Render<Dropdown<string>>(parameters => parameters
            .Add(p => p.Header, "repetir").Add(p => p.Options, [new DropdownOption<string>("nenhuma", "não repete")])).Markup,
    };

    public static TheoryData<string> Components => new(Renderers.Keys);

    // No page, no store and no service: a context with nothing registered renders it (UI-03).
    [Theory]
    [MemberData(nameof(Components))]
    public void EveryUiComponentRendersFromParametersAlone(string component)
    {
        using var context = new BunitContext();

        var markup = Renderers[component](context);

        Assert.False(string.IsNullOrWhiteSpace(markup), $"{component} rendered nothing");
    }
}