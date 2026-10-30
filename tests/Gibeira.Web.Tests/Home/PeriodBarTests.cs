using Bunit;

using Gibeira.Web.Components.Home;
using Gibeira.Web.Ledger;

namespace Gibeira.Web.Tests.Home;

public sealed class PeriodBarTests : BunitContext
{
    private static readonly DateOnly Today = new(2026, 10, 29);

    private Period period = Period.Starting(Today);

    private IRenderedComponent<PeriodBar> RenderBar() => Render<PeriodBar>(parameters => parameters
        .Add(p => p.Today, Today)
        .Bind(p => p.Period, period, value => period = value));

    private static string Pill(IRenderedComponent<PeriodBar> bar) => bar.Find(".period-pill").TextContent.Trim();

    // Clicks, then passes the period back down as the page that owns it does.
    private void Click(IRenderedComponent<PeriodBar> bar, string selector)
    {
        bar.Find(selector).Click();
        bar.Render(parameters => parameters.Add(p => p.Period, period));
    }

    // HOME-03: < and > move the period one month earlier or later.
    [Fact]
    public void PeriodArrowsMoveOneMonth()
    {
        var bar = RenderBar();
        Assert.Equal("out/2026 - set/2027", Pill(bar));

        Click(bar, "button[aria-label='mês anterior']");
        Assert.Equal("set/2026 - ago/2027", Pill(bar));
        Assert.Equal(Period.Starting(new DateOnly(2026, 9, 1)), period);

        Click(bar, "button[aria-label='mês seguinte']");
        Click(bar, "button[aria-label='mês seguinte']");
        Assert.Equal("nov/2026 - out/2027", Pill(bar));
        Assert.Equal(Period.Starting(new DateOnly(2026, 11, 1)), period);
    }

    // HOME-04: << and >> move the period 12 months earlier or later.
    [Fact]
    public void PeriodDoubleArrowsMoveTwelveMonths()
    {
        var bar = RenderBar();

        Click(bar, "button[aria-label='12 meses antes']");
        Assert.Equal("out/2025 - set/2026", Pill(bar));
        Assert.Equal(Period.Starting(new DateOnly(2025, 10, 1)), period);

        Click(bar, "button[aria-label='12 meses depois']");
        Click(bar, "button[aria-label='12 meses depois']");
        Assert.Equal("out/2027 - set/2028", Pill(bar));
        Assert.Equal(Period.Starting(new DateOnly(2027, 10, 1)), period);
    }

    // HOME-05: the pill opens the 25 periods from 12 months before to 12 months after today's month, the shown one checked.
    [Fact]
    public void PeriodPillListsTwentyFivePeriodsWithCurrentChecked()
    {
        var bar = RenderBar();
        Click(bar, "button[aria-label='mês seguinte']");
        Assert.Empty(bar.FindAll("[role=dialog]"));
        Assert.Equal("false", bar.Find(".period-pill").GetAttribute("aria-expanded"));

        bar.Find(".period-pill").Click();

        Assert.Equal("true", bar.Find(".period-pill").GetAttribute("aria-expanded"));
        var options = bar.FindAll("[role=dialog] .dropdown-option");
        Assert.Equal(25, options.Count);
        Assert.Equal("out/2025 - set/2026", options[0].TextContent.Trim());
        Assert.Equal("out/2026 - set/2027", options[12].TextContent.Trim());
        Assert.Equal("out/2027 - set/2028", options[24].TextContent.Trim());

        var checkedOption = Assert.Single(options, o => o.GetAttribute("aria-pressed") == "true");
        Assert.Equal("nov/2026 - out/2027", checkedOption.TextContent.Trim());
    }

    // HOME-06: choosing a period shows it and closes the dropdown.
    [Fact]
    public void ChoosingPeriodShowsItAndClosesDropdown()
    {
        var bar = RenderBar();
        bar.Find(".period-pill").Click();

        bar.FindAll("[role=dialog] .dropdown-option")[3].Click();
        bar.Render(parameters => parameters.Add(p => p.Period, period));

        Assert.Equal("jan/2026 - dez/2026", Pill(bar));
        Assert.Equal(Period.Starting(new DateOnly(2026, 1, 1)), period);
        Assert.Empty(bar.FindAll("[role=dialog]"));
        Assert.Equal("false", bar.Find(".period-pill").GetAttribute("aria-expanded"));
    }
}