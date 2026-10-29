# Gibeira

Gibeira is a self-hosted, open-source personal-finance app: a replacement for paid finance apps that runs on your own machine. It is also a lab for .NET, Blazor and spec-driven development.

The repository is a scaffold for now. It has the solution layout, the architecture rules and the commit conventions; finance modules arrive with the first features.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) 10.0.403 or a later 10.0 feature band (`global.json` pins it)
- [Git](https://git-scm.com/)

Nothing else: the Aspire dashboard and orchestrator come from NuGet, and the git hooks come from a local dotnet tool.

## Build

```sh
dotnet build Gibeira.slnx
```

The build treats warnings as errors, so a clean build has 0 warnings and 0 errors.

## Test

```sh
dotnet test Gibeira.slnx
```

This runs the architecture tests, the integration tests (they start the app through Aspire) and the repository tests (they exercise the git hooks and the module template in a temporary copy of the repository).

## Run

```sh
dotnet run --project src/Host/Gibeira.AppHost
```

The Aspire dashboard opens with one resource, `web`, the Blazor app. Open `web` from the dashboard. It listens on `localhost` only.

## Layout

| Path | Holds |
| ---- | ----- |
| `src/Host/Gibeira.Web` | The Blazor Web App (Interactive Server) |
| `src/Host/Gibeira.AppHost` | The Aspire AppHost that runs `web` and the dashboard |
| `src/Shared/Gibeira.SharedKernel` | Primitives shared across modules |
| `src/Shared/Gibeira.ServiceDefaults` | OpenTelemetry, health checks, service discovery, resilience |
| `src/Modules/<Module>/` | One folder per finance module, each with Domain, Application and Infrastructure projects |
| `tests/` | Architecture, integration and repository tests |
| `templates/module` | The `gibeira-module` template for new modules |
| `docs/` | [Architecture rules](docs/architecture.md) |

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for the commit format, the git hooks and how to add a module.

## License

[MIT](LICENSE)
