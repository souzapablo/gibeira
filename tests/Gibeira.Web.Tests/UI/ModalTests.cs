using Bunit;

using Gibeira.Web.Components.UI;

using Microsoft.AspNetCore.Components;

namespace Gibeira.Web.Tests.UI;

public sealed class ModalTests : BunitContext
{
    // ADD-24: X, Esc and a backdrop click each close the modal.
    [Fact]
    public void ModalClosesOnXEscAndBackdrop()
    {
        var closes = 0;
        var modal = Render<Modal>(parameters => parameters
            .Add(p => p.Title, "adicionar")
            .Add(p => p.OnClose, () => closes++)
            .AddChildContent("<input class=\"amount\" />"));

        modal.Find("button[aria-label='fechar']").Click();
        Assert.Equal(1, closes);

        modal.Find("[role=dialog]").KeyDown(key: "Enter");
        Assert.Equal(1, closes);
        modal.Find("[role=dialog]").KeyDown(key: "Escape");
        Assert.Equal(2, closes);

        // Esc typed in a field of the modal reaches it too.
        modal.Find(".amount").KeyDown(key: "Escape");
        Assert.Equal(3, closes);

        modal.Find(".modal-backdrop").Click();
        Assert.Equal(4, closes);
        Assert.Null(modal.Find("[role=dialog]").Closest(".modal-backdrop"));
    }

    // UI-06: an aria dialog named by its title, with its content, taking focus when it opens.
    [Fact]
    public void ModalIsAnAriaDialog()
    {
        var modal = Render<Modal>(parameters => parameters
            .Add(p => p.Title, "adicionar")
            .AddChildContent("<p class=\"rows\">linhas</p>"));

        var dialog = modal.Find("[role=dialog]");
        Assert.Equal("true", dialog.GetAttribute("aria-modal"));
        Assert.Equal("adicionar", dialog.GetAttribute("aria-label"));
        Assert.Equal("-1", dialog.GetAttribute("tabindex"));
        Assert.Equal("adicionar", dialog.QuerySelector(".modal-title")!.TextContent);
        Assert.Equal("linhas", dialog.QuerySelector(".modal-body .rows")!.TextContent);
        Assert.NotNull(dialog.QuerySelector("button[aria-label='fechar']"));
        JSInterop.VerifyFocusAsyncInvoke();
    }

    // ADD-25: Esc in a dropdown or popover inside the modal closes that one and leaves the modal open.
    [Fact]
    public void EscInsideAPopoverClosesOnlyThePopover()
    {
        var modalCloses = 0;
        var popoverCloses = 0;
        RenderFragment content = builder =>
        {
            builder.OpenComponent<Dropdown<string>>(0);
            builder.AddComponentParameter(1, nameof(Dropdown<string>.Header), "repetir");
            builder.AddComponentParameter(2, nameof(Dropdown<string>.Options), new[] { new DropdownOption<string>("nenhuma", "não repete") });
            builder.AddComponentParameter(3, nameof(Dropdown<string>.OnClose), EventCallback.Factory.Create(this, () => popoverCloses++));
            builder.CloseComponent();
            builder.OpenComponent<StepperPopover<int>>(4);
            builder.AddComponentParameter(5, nameof(StepperPopover<int>.Header), "parcelas");
            builder.AddComponentParameter(6, nameof(StepperPopover<int>.Value), 2);
            builder.AddComponentParameter(7, nameof(StepperPopover<int>.Format), (Func<int, string>)(count => $"{count}x"));
            builder.AddComponentParameter(8, nameof(StepperPopover<int>.OnClose), EventCallback.Factory.Create(this, () => popoverCloses++));
            builder.CloseComponent();
        };
        var modal = Render<Modal>(parameters => parameters
            .Add(p => p.Title, "adicionar")
            .Add(p => p.OnClose, () => modalCloses++)
            .Add(p => p.ChildContent, content));

        modal.Find(".dropdown").KeyDown(key: "Escape");
        modal.Find(".stepper").KeyDown(key: "Escape");

        Assert.Equal(2, popoverCloses);
        Assert.Equal(0, modalCloses);
    }
}