# Architecture

Gibeira is a modular monolith: one Blazor host, and one folder per finance module under `src/Modules/`. Each module has three projects:

| Project | Holds | May depend on |
| ------- | ----- | ------------- |
| `Gibeira.<Module>.Domain` | Entities, value objects and domain rules | `Gibeira.SharedKernel` |
| `Gibeira.<Module>.Application` | Use cases | Its Domain |
| `Gibeira.<Module>.Infrastructure` | Persistence and external services | Its Application and Domain |

`Gibeira.Web` composes the modules; `Gibeira.AppHost` runs `Gibeira.Web`. No module depends on another. When two modules need to talk, the first feature that needs it adds a `Contracts` project on purpose.

## Rules

`tests/Gibeira.ArchitectureTests` enforces every rule below. Each rule is checked twice: it must hold on the solution, and it must fail on a fixture assembly in `tests/Gibeira.ArchitectureTests.Fixtures` that breaks it on purpose. Without the fixture, a rule over a solution with no modules would pass without proving anything.

| ID | Rule | Tests |
| -- | ---- | ----- |
| ARCH-01 | A Domain project does not depend on its module's Application or Infrastructure. The failure names the offending type. | `DomainDependingOnOuterLayersIsReported`, `DomainLayerRuleHoldsOnSolution` |
| ARCH-02 | An Application project does not depend on its module's Infrastructure. The failure names the offending type. | `ApplicationDependingOnInfrastructureIsReported`, `ApplicationLayerRuleHoldsOnSolution` |
| ARCH-03 | Domain and Application projects do not use ASP.NET Core (`Microsoft.AspNetCore.*`) or EF Core (`Microsoft.EntityFrameworkCore.*`). The failure names the offending type. | `InnerLayersUsingFrameworksAreReported`, `FrameworkRuleHoldsOnSolution` |
| ARCH-04 | No project of one module depends on a project of another module. The failure names both modules. | `CrossModuleDependencyIsReported`, `ModuleIsolationRuleHoldsOnSolution` |
| ARCH-05 | `Gibeira.SharedKernel` depends on no module, on `Gibeira.Web` and on `Gibeira.AppHost`. | `SharedKernelDependingOnModulesOrHostsIsReported`, `SharedKernelRuleHoldsOnSolution` |
| ARCH-06 | Only `Gibeira.AppHost` depends on `Gibeira.Web`. | `DependencyOnWebOutsideAppHostIsReported`, `WebRuleHoldsOnSolution` |
| ARCH-07 | Every project under `src/Modules/<Module>/` is named `Gibeira.<Module>.Domain`, `Gibeira.<Module>.Application` or `Gibeira.<Module>.Infrastructure`. The failure names the project. | `MisnamedModuleProjectIsReported`, `NamingRuleHoldsOnSolution` |
| ARCH-08 | A module added under `src/Modules/` is checked by every rule with no change to the test project: the test project references `src/Modules/**/*.csproj`. | `NewModuleIsCoveredByArchitectureTests` (in `tests/Gibeira.RepositoryTests`) |
| ARCH-09 | Every rule holds on the solution. | every `*HoldsOnSolution` test |
| ARCH-10 | A module name is one segment of letters and digits: `Gibeira.A.B.Domain` is reported, not skipped by every other rule. The failure names the assembly. | `MultiSegmentModuleNameIsReported`, `ModuleNameRuleHoldsOnSolution` |

## How the rules work

- A rule is a function over the loaded assemblies and a name prefix: `Gibeira` selects the solution, `Fixture` the deliberate violations.
- An assembly named `<prefix>.<Module>.<Layer>`, with `<Layer>` one of Domain, Application or Infrastructure, belongs to that module.
- A dependency is any type one assembly's types use from another assembly: base types, fields, properties, method signatures and bodies, as [ArchUnitNET](https://github.com/TNG/ArchUnitNET) reads them.
