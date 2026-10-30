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

    private static BrowserNewContextOptions Viewport(int width, int height) =>
        new() { ViewportSize = new ViewportSize { Width = width, Height = height } };

    private static async Task<IPage> OpenHomeAsync(IBrowserContext context)
    {
        var page = await context.NewPageAsync();
        await page.GotoAsync("/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return page;
    }

    private static async Task<LocatorBoundingBoxResult> Box(ILocator locator) =>
        await locator.BoundingBoxAsync() ?? throw new InvalidOperationException($"{locator} is not visible");
}