using Bunit;

using Gibeira.Web.Components.UI;

using Microsoft.AspNetCore.Components;

namespace Gibeira.Web.Tests.UI;

public sealed class DropdownTests : BunitContext
{
    private static readonly DropdownOption<string>[] Repetitions =
    [
        new("mensal", "mensalmente", "repete o mesmo valor mensalmente"),
        new("semanal", "semanalmente", "repete o mesmo valor semanalmente"),
        new("nenhuma", "não repete"),
    ];

    private static readonly DropdownOption<string>[] Tags = [new("café", "café"), new("mercado", "mercado"), new("ônibus", "ônibus")];

    // Spec component table: options with optional description and icon, a check on the selected one.
    [Fact]
    public void DropdownChecksSelectedOption()
    {
        RenderFragment icon = builder => builder.AddMarkupContent(0, "<span class=\"option-icon\"></span>");
        DropdownOption<string>[] options = [Repetitions[0] with { Icon = icon }, Repetitions[1], Repetitions[2]];

        var dropdown = Render<Dropdown<string>>(parameters => parameters
            .Add(p => p.Header, "repetir")
            .Add(p => p.Options, options)
            .Add(p => p.Selected, ["semanal"]));

        var buttons = dropdown.FindAll(".dropdown-option");
        Assert.Equal(["mensalmente", "semanalmente", "não repete"], buttons.Select(b => b.QuerySelector(".dropdown-label")!.TextContent));
        Assert.Equal("repete o mesmo valor mensalmente", buttons[0].QuerySelector(".dropdown-description")!.TextContent);
        Assert.Null(buttons[2].QuerySelector(".dropdown-description"));
        Assert.NotNull(buttons[0].QuerySelector(".option-icon"));
        Assert.Null(buttons[1].QuerySelector(".option-icon"));

        Assert.Equal(["false", "true", "false"], buttons.Select(b => b.GetAttribute("aria-pressed")));
        Assert.Equal(IconName.Check, Assert.Single(dropdown.FindComponents<Icon>()).Instance.Name);
        Assert.Equal([0, 1, 0], buttons.Select(b => b.QuerySelectorAll("svg").Length));
        Assert.Empty(dropdown.FindAll(".dropdown-box"));
    }

    // Spec component table: multiple choice checks several, and choosing one leaves the list open.
    [Fact]
    public void MultipleDropdownTogglesWithoutClosing()
    {
        var selected = new HashSet<string> { "café" };
        var closes = 0;
        var dropdown = Render<Dropdown<string>>(parameters => parameters
            .Add(p => p.Header, "tags")
            .Add(p => p.Options, Tags)
            .Add(p => p.Multiple, true)
            .Add(p => p.Selected, selected.ToArray())
            .Add(p => p.OnChoose, value =>
            {
                if (!selected.Remove(value))
                {
                    selected.Add(value);
                }
            })
            .Add(p => p.OnClose, () => closes++));

        void Toggle(int index)
        {
            dropdown.FindAll(".dropdown-option")[index].Click();
            dropdown.Render(parameters => parameters.Add(p => p.Selected, selected.ToArray()));
        }

        Assert.Equal(3, dropdown.FindAll(".dropdown-box").Count);

        Toggle(2);
        Assert.Equal(["true", "false", "true"], dropdown.FindAll(".dropdown-option").Select(b => b.GetAttribute("aria-pressed")));
        Assert.Equal(2, dropdown.FindComponents<Icon>().Count(i => i.Instance.Name == IconName.Check));

        Toggle(0);
        Assert.Equal(["false", "false", "true"], dropdown.FindAll(".dropdown-option").Select(b => b.GetAttribute("aria-pressed")));
        Assert.Equal(["ônibus"], selected);
        Assert.Equal(0, closes);
        Assert.Single(dropdown.FindAll("[role=dialog]"));
    }

    // ADD-25: Esc or a click outside closes the list and chooses nothing.
    [Fact]
    public void DropdownClosesOnEscAndOutsideClickWithoutChoosing()
    {
        var chosen = new List<string>();
        var closes = 0;
        var dropdown = Render<Dropdown<string>>(parameters => parameters
            .Add(p => p.Header, "repetir")
            .Add(p => p.Options, Repetitions)
            .Add(p => p.Selected, ["mensal"])
            .Add(p => p.OnChoose, value => chosen.Add(value))
            .Add(p => p.OnClose, () => closes++));

        // A click inside the list cannot bubble to the outside layer: the layer is beside it, not around it.
        Assert.Null(dropdown.Find("[role=dialog]").Closest(".dropdown-layer"));
        dropdown.Find("[role=dialog]").KeyDown(key: "a");
        Assert.Equal(0, closes);

        dropdown.Find("[role=dialog]").KeyDown(key: "Escape");
        Assert.Equal(1, closes);

        dropdown.Find(".dropdown-layer").Click();
        Assert.Equal(2, closes);
        Assert.Empty(chosen);

        dropdown.FindAll(".dropdown-option")[1].Click();
        Assert.Equal(["semanal"], chosen);
        Assert.Equal(2, closes);
    }

    // UI-06: the list is a dialog named by its header, holding the header action, and takes focus so Esc reaches it.
    [Fact]
    public void DropdownHasDialogRole()
    {
        var dropdown = Render<Dropdown<string>>(parameters => parameters
            .Add(p => p.Header, "cartão")
            .Add(p => p.HeaderAction, builder => builder.AddMarkupContent(0, "<button class=\"add-card\" disabled>adicionar cartão</button>"))
            .Add(p => p.Options, Repetitions));

        var dialog = dropdown.Find("[role=dialog]");
        Assert.Equal("cartão", dialog.GetAttribute("aria-label"));
        Assert.Equal("-1", dialog.GetAttribute("tabindex"));
        Assert.Equal("cartão", dialog.QuerySelector(".dropdown-title")!.TextContent);
        Assert.NotNull(dialog.QuerySelector(".dropdown-header .add-card"));
        JSInterop.VerifyFocusAsyncInvoke();
    }
}