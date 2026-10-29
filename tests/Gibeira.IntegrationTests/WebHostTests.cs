using System.Net;

using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace Gibeira.IntegrationTests;

public sealed class WebHostTests(WebHostFixture fixture) : IClassFixture<WebHostFixture>
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
}