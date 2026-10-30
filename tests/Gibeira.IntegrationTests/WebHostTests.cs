using System.Net;
using System.Text.RegularExpressions;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace Gibeira.IntegrationTests;

public sealed partial class WebHostTests(WebHostFixture fixture) : IClassFixture<WebHostFixture>
{
    [Fact]
    public async Task WebHomePageReturnsGibeira()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.App.CreateHttpClient("web", "http");

        using var response = await client.GetAsync(new Uri("/", UriKind.Relative), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<h1>Gibeira</h1>", body, StringComparison.Ordinal);
    }

    // Neither the page nor any stylesheet it links brings Bootstrap (UI-05).
    [Fact]
    public async Task WebHomePageLoadsNoBootstrap()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.App.CreateHttpClient("web", "http");

        var page = await client.GetStringAsync(new Uri("/", UriKind.Relative), cancellationToken);
        var stylesheets = StylesheetLink().Matches(page).Select(match => match.Groups["href"].Value).ToArray();

        Assert.DoesNotContain("bootstrap", page, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(stylesheets);
        foreach (var href in stylesheets)
        {
            var stylesheet = await client.GetStringAsync(new Uri(href, UriKind.Relative), cancellationToken);
            Assert.False(stylesheet.Contains("bootstrap", StringComparison.OrdinalIgnoreCase), $"{href} contains bootstrap");
        }
    }

    [Fact]
    public async Task WebHealthEndpointReturnsOk()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = fixture.App.CreateHttpClient("web", "http");

        using var response = await client.GetAsync(new Uri("/health", UriKind.Relative), cancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public void WebEndpointsAreLoopbackOnly()
    {
        var model = fixture.App.Services.GetRequiredService<DistributedApplicationModel>();
        var web = model.Resources.OfType<ProjectResource>().Single(r => r.Name == "web");

        Assert.True(web.TryGetEndpoints(out var endpoints));
        Assert.NotEmpty(endpoints);
        Assert.All(endpoints, endpoint =>
        {
            Assert.True(IsLoopback(endpoint.TargetHost), $"{endpoint.Name} binds to {endpoint.TargetHost}");
            Assert.NotNull(endpoint.AllocatedEndpoint);
            Assert.True(IsLoopback(endpoint.AllocatedEndpoint.Address), $"{endpoint.Name} is allocated on {endpoint.AllocatedEndpoint.Address}");
        });
    }

    private static bool IsLoopback(string host) =>
        string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
        || (IPAddress.TryParse(host, out var address) && IPAddress.IsLoopback(address));

    [GeneratedRegex(@"<link[^>]*\brel=""stylesheet""[^>]*\bhref=""(?<href>[^""]+)""")]
    private static partial Regex StylesheetLink();
}