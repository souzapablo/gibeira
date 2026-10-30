using Bunit;

using Gibeira.Web.Components.UI;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.UI;

public sealed class StepperPopoverTests : BunitContext
{
    // Spec component table: header, < value >, "salvar"; the arrows step and salvar saves what is shown.
    [Fact]
    public void StepperRaisesStepAndSave()
    {
        var value = new DateOnly(2026, 11, 10);
        var steps = new List<int>();
        var saves = 0;
        var closes = 0;
        var popover = Render<StepperPopover<DateOnly>>(parameters => parameters
            .Add(p => p.Header, "vencimento")
            .Add(p => p.Value, value)
            .Add(p => p.Format, PtBr.LongDate)
            .Add(p => p.OnStep, step => steps.Add(step))
            .Add(p => p.OnSave, () => saves++)
            .Add(p => p.OnClose, () => closes++));

        Assert.Equal("vencimento", popover.Find(".stepper-title").TextContent);
        Assert.Equal("10/11/2026", popover.Find(".stepper-value").TextContent);
        popover.Find("button[aria-label='próximo']").Click();
        popover.Find("button[aria-label='anterior']").Click();
        popover.Find("button[aria-label='próximo']").Click();
        Assert.Equal([1, -1, 1], steps);
        Assert.Equal(0, saves);

        popover.Render(parameters => parameters.Add(p => p.Value, new DateOnly(2026, 12, 10)));
        Assert.Equal("10/12/2026", popover.Find(".stepper-value").TextContent);

        var salvar = popover.Find("button.button--light");
        Assert.Equal("salvar", salvar.TextContent.Trim());
        salvar.Click();
        Assert.Equal(1, saves);
        Assert.Equal(0, closes);
    }

    // ADD-25 and UI-06: the popover is a dialog; Esc or a click outside closes it and saves nothing.
    [Fact]
    public void StepperClosesOnEscWithoutSaving()
    {
        var saves = 0;
        var closes = 0;
        var steps = 0;
        var popover = Render<StepperPopover<int>>(parameters => parameters
            .Add(p => p.Header, "parcelas")
            .Add(p => p.Value, 2)
            .Add(p => p.Format, count => $"{count}x")
            .Add(p => p.OnStep, _ => steps++)
            .Add(p => p.OnSave, () => saves++)
            .Add(p => p.OnClose, () => closes++));

        var dialog = popover.Find("[role=dialog]");
        Assert.Equal("parcelas", dialog.GetAttribute("aria-label"));
        Assert.Equal("-1", dialog.GetAttribute("tabindex"));
        Assert.Null(dialog.Closest(".stepper-layer"));
        JSInterop.VerifyFocusAsyncInvoke();

        dialog.KeyDown(key: "Enter");
        Assert.Equal(0, closes);

        dialog.KeyDown(key: "Escape");
        Assert.Equal(1, closes);

        popover.Find(".stepper-layer").Click();
        Assert.Equal(2, closes);
        Assert.Equal(0, saves);
        Assert.Equal(0, steps);
    }
}