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
}