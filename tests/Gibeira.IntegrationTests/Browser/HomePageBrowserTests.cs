using System.Collections.Concurrent;

using Microsoft.Playwright;

namespace Gibeira.IntegrationTests.Browser;

public sealed class HomePageBrowserTests(BrowserFixture fixture) : IClassFixture<BrowserFixture>
{
    // Loads both families whether or not the page uses them yet, then lists the ones that loaded.
    private const string LoadFonts = """
        async () => {
            await Promise.all(['"Bricolage Grotesque"', '"Instrument Sans"'].map(family => document.fonts.load(`16px ${family}`)));
            await document.fonts.ready;
            const loaded = [...document.fonts].filter(face => face.status === 'loaded').map(face => face.family.replaceAll('"', ''));
            return [...new Set(loaded)].sort();
        }
        """;

    // The page, its scripts, its styles and its fonts all come from the app itself (UI-04).
    [Fact]
    public async Task HomePageLoadsNothingFromAnotherHost()
    {
        await using var context = await fixture.NewContextAsync();
        var page = await context.NewPageAsync();
        var requests = new ConcurrentQueue<string>();
        page.Request += (_, request) => requests.Enqueue(request.Url);
        page.WebSocket += (_, socket) => requests.Enqueue(socket.Url);

        await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        var fonts = await page.EvaluateAsync<string[]>(LoadFonts);
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        Assert.Equal(["Bricolage Grotesque", "Instrument Sans"], fonts);
        Assert.Contains(requests, url => url.EndsWith(".woff2", StringComparison.Ordinal));
        Assert.All(requests, url => Assert.True(new Uri(url).IsLoopback, $"{url} goes to another host"));
    }

    // HOME-01: the top bar, then the centered period bar (<<, <, pill, >, >>), then the twelve months side by side.
    [Fact]
    public async Task HomeShowsTopBarPeriodBarAndMonthTablesInOrder()
    {
        await using var context = await fixture.NewContextAsync(Viewport(1290, 902));
        var page = await OpenHomeAsync(context);

        var top = await Box(page.Locator(".top-bar"));
        var bar = await Box(page.Locator(".period-bar"));
        var ledger = await Box(page.Locator(".ledger"));
        Assert.True(top.Y + top.Height <= bar.Y, "the period bar is below the top bar");
        Assert.True(bar.Y + bar.Height <= ledger.Y, "the ledger is below the period bar");

        var controls = page.Locator(".period-bar > *");
        Assert.Equal(5, await controls.CountAsync());
        Assert.Equal("12 meses antes", await controls.Nth(0).GetAttributeAsync("aria-label"));
        Assert.Equal("mês anterior", await controls.Nth(1).GetAttributeAsync("aria-label"));
        Assert.Equal(ExpectedPeriod.Label, (await controls.Nth(2).Locator(".period-pill").InnerTextAsync()).Trim());
        Assert.Equal(1, await controls.Nth(2).Locator(".period-pill svg").CountAsync());
        Assert.Equal("mês seguinte", await controls.Nth(3).GetAttributeAsync("aria-label"));
        Assert.Equal("12 meses depois", await controls.Nth(4).GetAttributeAsync("aria-label"));
        var boxes = new List<LocatorBoundingBoxResult>();
        for (var i = 0; i < 5; i++)
        {
            boxes.Add(await Box(controls.Nth(i)));
        }

        Assert.All(boxes.Zip(boxes.Skip(1)), pair => Assert.True(pair.First.X + pair.First.Width <= pair.Second.X, "the controls run left to right"));
        var middle = (boxes[0].X + boxes[4].X + boxes[4].Width) / 2;
        Assert.InRange(middle, 645 - 1, 645 + 1);

        Assert.Equal(ExpectedPeriod.MonthTitles, (await page.Locator(".month-title").AllInnerTextsAsync()).Select(t => t.Trim()));
        var months = page.Locator(".month");
        var first = await Box(months.Nth(0));
        for (var i = 1; i < 12; i++)
        {
            var previous = await Box(months.Nth(i - 1));
            var month = await Box(months.Nth(i));
            Assert.Equal(first.Y, month.Y);
            Assert.True(previous.X + previous.Width <= month.X, $"month {i + 1} sits right of month {i}");
        }
    }

    // HOME-16: with more days than fit, the header and total rows stay in view while the days scroll.
    [Fact]
    public async Task HeaderAndTotalRowsStayInViewWhileDaysScroll()
    {
        await using var context = await fixture.NewContextAsync(Viewport(1290, 902));
        var page = await OpenHomeAsync(context);
        var month = page.Locator(".month").First;
        var scroller = month.Locator(".month-scroll");

        Assert.True(await scroller.EvaluateAsync<bool>("el => el.scrollHeight > el.clientHeight"), "the day rows are taller than the window");
        await scroller.EvaluateAsync("el => { el.scrollTop = el.scrollHeight / 2; }");
        Assert.True(await scroller.EvaluateAsync<double>("el => el.scrollTop") > 0);

        var view = await Box(scroller);
        var head = await Box(month.Locator("thead th").First);
        var foot = await Box(month.Locator("tfoot th").First);
        var firstDay = await Box(month.Locator("tbody tr").First);
        Assert.InRange(head.Y, view.Y - 1, view.Y + 1);
        Assert.InRange(foot.Y + foot.Height, view.Y + view.Height - 1, view.Y + view.Height + 1);
        Assert.True(foot.Y + foot.Height <= 902, "the total row is inside the window");
        Assert.True(firstDay.Y + firstDay.Height <= head.Y + head.Height, "the first day scrolled under the header");
    }

    // HOME-17: at 360 px the ledger scrolls sideways inside itself and the page does not.
    [Fact]
    public async Task LedgerScrollsSidewaysWithoutPageScrollAt360()
    {
        await using var context = await fixture.NewContextAsync(Viewport(360, 800));
        var page = await OpenHomeAsync(context);

        Assert.True(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth <= document.documentElement.clientWidth"), "the page has no horizontal scroll");
        var ledger = page.Locator(".ledger");
        Assert.True(await ledger.EvaluateAsync<bool>("el => el.scrollWidth > el.clientWidth"), "the months are wider than the ledger");

        await ledger.EvaluateAsync("el => { el.scrollLeft = 400; }");
        Assert.Equal(400, await ledger.EvaluateAsync<double>("el => el.scrollLeft"));
        Assert.Equal(0, await page.EvaluateAsync<double>("() => window.scrollX"));
    }

    // SCR-02: below 432 px the drawer is the full window width; from 433 px it is its own 400 px at the right.
    // 420 px is wider than the drawer, so only the narrow-window rule makes it fill the window.
    [Theory]
    [InlineData(420, 0, 420)]
    [InlineData(433, 33, 400)]
    public async Task DrawerFillsWidthBelow432(int width, int x, int drawerWidth)
    {
        await using var context = await fixture.NewContextAsync(Viewport(width, 800));
        var page = await OpenHomeAsync(context);

        var drawer = await OpenCreditCardDrawerAsync(page);

        var box = await Box(drawer);
        Assert.Equal(x, box.X);
        Assert.Equal(drawerWidth, box.Width);
    }

    // DAY-04: a description too long for its row stays on one line and ends in an ellipsis, leaving the amount and ⋮ in view.
    [Fact]
    public async Task LongDescriptionIsCutWithEllipsis()
    {
        await using var context = await fixture.NewContextAsync(Viewport(400, 800));
        var page = await OpenHomeAsync(context);
        var drawer = await OpenCreditCardDrawerAsync(page);
        var entry = drawer.Locator(".entry").First;
        var description = entry.Locator(".entry-description");
        var height = (await Box(entry)).Height;

        // The seed has no long description; the add modal that could make one comes later in the page.
        await description.EvaluateAsync("el => { el.textContent = 'compra parcelada da geladeira nova da cozinha com instalação e frete'; }");

        Assert.True(await description.EvaluateAsync<bool>("el => el.scrollWidth > el.clientWidth"), "the description is wider than its row");
        Assert.Equal("ellipsis", await description.EvaluateAsync<string>("el => getComputedStyle(el).textOverflow"));
        Assert.Equal("nowrap", await description.EvaluateAsync<string>("el => getComputedStyle(el).whiteSpace"));
        Assert.Equal("hidden", await description.EvaluateAsync<string>("el => getComputedStyle(el).overflowX"));
        Assert.Equal(height, (await Box(entry)).Height);
        var menu = await Box(entry.Locator("button[aria-label='opções']"));
        Assert.True(menu.X + menu.Width <= 400, "⋮ stays inside the window");
        Assert.True(await entry.Locator(".money").IsVisibleAsync());
    }

    // ADD-02: a letter or a third decimal typed in the amount field never shows.
    [Fact]
    public async Task AmountFieldDropsRejectedCharacters()
    {
        await using var context = await fixture.NewContextAsync(Viewport(1290, 902));
        var page = await OpenHomeAsync(context);
        var modal = await OpenModalAsync(page);
        var amount = modal.Locator("input.amount-input");

        // The filter runs on the server: type at a person's pace, so each key meets the text the last one left.
        var typing = new LocatorPressSequentiallyOptions { Delay = 100 };
        await amount.PressSequentiallyAsync("12a,3x45", typing);

        Assert.Equal("12,34", await amount.InputValueAsync());
        await amount.PressSequentiallyAsync(",", typing);
        Assert.Equal("12,34", await amount.InputValueAsync());
        Assert.True(await modal.Locator(".add-button-slot .button").IsEnabledAsync());
    }

    // HOME-22: the period list opens with the checked period, the 13th of 25, in view inside the list.
    [Theory]
    [InlineData(1290, 902)]
    [InlineData(360, 800)]
    public async Task PeriodDropdownOpensWithCheckedPeriodInView(int width, int height)
    {
        await using var context = await fixture.NewContextAsync(Viewport(width, height));
        var page = await OpenHomeAsync(context);

        await page.Locator(".period-pill").ClickAsync();
        var list = page.Locator(".period-bar .dropdown-options");
        await list.WaitForAsync();
        var checkedOption = list.Locator(".dropdown-option[aria-pressed=true]");
        await Assertions.Expect(checkedOption).ToBeFocusedAsync();

        Assert.Equal(13, await list.EvaluateAsync<int>("el => [...el.children].findIndex(o => o.getAttribute('aria-pressed') === 'true') + 1"));
        var box = await Box(list);
        var option = await Box(checkedOption);
        Assert.True(option.Y >= box.Y && option.Y + option.Height <= box.Y + box.Height, $"checked option at {option.Y}, list {box.Y} to {box.Y + box.Height}");
    }

    // HOME-23: at 360 px the period bar's five controls share one line and the open period list stays inside the window.
    [Fact]
    public async Task PeriodBarFitsOneLineAt360()
    {
        await using var context = await fixture.NewContextAsync(Viewport(360, 800));
        var page = await OpenHomeAsync(context);
        var bar = page.Locator(".period-bar");

        var tops = await bar.EvaluateAsync<double[]>("el => [...el.children].map(c => c.getBoundingClientRect().top)");
        Assert.Equal(5, tops.Length);
        Assert.All(tops, top => Assert.Equal(tops[0], top));
        foreach (var button in await bar.Locator("button").AllAsync())
        {
            Assert.True((await Box(button)).Height >= 44, "every control stays 44 px high");
        }

        await page.Locator(".period-pill").ClickAsync();
        var dropdown = await Box(page.Locator(".period-bar .dropdown"));
        Assert.True(dropdown.X >= 0 && dropdown.X + dropdown.Width <= 360, $"dropdown from {dropdown.X} to {dropdown.X + dropdown.Width}");
        Assert.False(await page.EvaluateAsync<bool>("() => document.documentElement.scrollWidth > document.documentElement.clientWidth"));
    }

    // MOCK-02: an item added before a reload is gone after it.
    [Fact]
    public async Task ReloadDiscardsChanges()
    {
        await using var context = await fixture.NewContextAsync(Viewport(1290, 902));
        var page = await OpenHomeAsync(context);
        var incomeTotal = page.Locator(".month").First.Locator("tfoot td.money-cell").First.Locator(".money");
        var before = (await incomeTotal.InnerTextAsync()).Trim();

        // Typed at a person's pace and read with waiting assertions: each answer comes back over the circuit.
        var modal = await OpenModalAsync(page);
        await modal.Locator("input.amount-input").PressSequentiallyAsync("777", new LocatorPressSequentiallyOptions { Delay = 100 });
        await Assertions.Expect(modal.Locator("input.amount-input")).ToHaveValueAsync("777");
        await modal.Locator(".add-button-slot .button").ClickAsync();
        await modal.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
        await Assertions.Expect(incomeTotal).Not.ToHaveTextAsync(before);

        await page.ReloadAsync();
        await WaitForCircuitAsync(page);

        await Assertions.Expect(incomeTotal).ToHaveTextAsync(before);
    }

    // SCR-01: at 400 px the modal is the window width minus 16 px each side.
    [Fact]
    public async Task ModalFillsWidthMinusGuttersBelow432()
    {
        await using var context = await fixture.NewContextAsync(Viewport(400, 800));
        var page = await OpenHomeAsync(context);

        var modal = await OpenModalAsync(page);

        var box = await Box(modal);
        Assert.Equal(16, box.X);
        Assert.Equal(368, box.Width);
    }

    // The 10th of the current month holds the seed's Itaú cartão item.
    private static async Task<ILocator> OpenCreditCardDrawerAsync(IPage page)
    {
        var cell = page.Locator(".month").First.Locator("tbody tr").Nth(9).Locator("td.money-cell").Nth(4).Locator("button");
        var drawer = await ClickToOpenAsync(cell, page.Locator(".drawer"));
        Assert.Equal("cartão", (await drawer.Locator(".filter-pill").InnerTextAsync()).Trim());
        return drawer;
    }

    private static Task<ILocator> OpenModalAsync(IPage page) => ClickToOpenAsync(page.Locator(".top-bar .button"), page.Locator(".modal"));

    // OpenHomeAsync has waited for the circuit, so one click reaches its handler.
    private static async Task<ILocator> ClickToOpenAsync(ILocator control, ILocator target)
    {
        await control.ClickAsync();
        await target.WaitForAsync();
        return target;
    }

    private static BrowserNewContextOptions Viewport(int width, int height) =>
        new() { ViewportSize = new ViewportSize { Width = width, Height = height } };

    private static async Task<IPage> OpenHomeAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.Commit });
        await WaitForCircuitAsync(page);
        return page;
    }

    // The circuit's first render replaces the prerendered page, which would undo a scroll made before it.
    private static async Task WaitForCircuitAsync(IPage page)
    {
        await page.Locator(".ledger").EvaluateAsync("el => { el.__prerendered = true; }");
        await page.WaitForFunctionAsync("() => !document.querySelector('.ledger').__prerendered");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private static async Task<LocatorBoundingBoxResult> Box(ILocator locator) =>
        await locator.BoundingBoxAsync() ?? throw new InvalidOperationException($"{locator} is not visible");
}