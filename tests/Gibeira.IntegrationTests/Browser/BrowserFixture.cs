using Aspire.Hosting.Testing;

using Microsoft.Playwright;

namespace Gibeira.IntegrationTests.Browser;

public sealed class BrowserFixture : IAsyncLifetime
{
    // dotnet build does not download the browser; CONTRIBUTING.md lists this command too.
    private const string InstallCommand = "pwsh tests/Gibeira.IntegrationTests/bin/Debug/net10.0/playwright.ps1 install chromium";

    private readonly WebHostFixture host = new();
    private IPlaywright? playwright;
    private IBrowser? browser;
    private Uri baseAddress = null!;

    public async ValueTask InitializeAsync()
    {
        await host.InitializeAsync();
        baseAddress = host.App.GetEndpoint("web", "http");
        playwright = await Playwright.CreateAsync();
        try
        {
            browser = await playwright.Chromium.LaunchAsync();
        }
        catch (PlaywrightException exception) when (exception.Message.Contains("Executable doesn't exist", StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Chromium for Playwright is not installed. From the repository root, run: {InstallCommand}", exception);
        }
    }

    // A fresh context per test, so no cookie, storage or circuit leaks between tests.
    public Task<IBrowserContext> NewContextAsync(BrowserNewContextOptions? options = null)
    {
        options ??= new BrowserNewContextOptions();
        options.BaseURL = baseAddress.ToString();
        return browser!.NewContextAsync(options);
    }

    public async ValueTask DisposeAsync()
    {
        if (browser is not null)
        {
            await browser.DisposeAsync();
        }

        playwright?.Dispose();
        await host.DisposeAsync();
    }
}