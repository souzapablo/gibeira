using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace Gibeira.IntegrationTests;

public sealed class WebHostFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartTimeout = TimeSpan.FromMinutes(2);

    // The http profile keeps the tests off TLS, so an untrusted dev certificate cannot fail them.
    private static readonly string[] AppHostArgs = ["DOTNET_LAUNCH_PROFILE=http"];

    public DistributedApplication App { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        using var cts = new CancellationTokenSource(StartTimeout);
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.Gibeira_AppHost>(AppHostArgs, cts.Token);
        App = await builder.BuildAsync(cts.Token);
        await App.StartAsync(cts.Token);
        await App.ResourceNotifications.WaitForResourceHealthyAsync("web", cts.Token);
    }

    public async ValueTask DisposeAsync() => await App.DisposeAsync();
}