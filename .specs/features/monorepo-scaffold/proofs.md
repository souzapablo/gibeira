# monorepo-scaffold proofs

**HEAD**: 8f52751
**Tree**: clean
**Result**: 40 pass, 0 fail; mutations 3 killed, 0 not

| Check | Proof | Ran | Passed | Result |
| --- | --- | --- | --- | --- |
| C1 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*SolutionC` | count unknown | - | pass |
| C2 | `dotnet build Gibeira.slnx -warnaserror` | count unknown | - | pass |
| C3 | `dotnet test --solution Gibeira.slnx` | count unknown | - | pass |
| C4 | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebHome` | count unknown | - | pass |
| C5 | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebHeal` | count unknown | - | pass |
| C6 | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebEndp` | count unknown | - | pass |
| C7 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*EveryProj` | count unknown | - | pass |
| C8 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*NoProject` | count unknown | - | pass |
| C9 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Domai` | count unknown | - | pass |
| C10 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Appli` | count unknown | - | pass |
| C11 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Inner` | count unknown | - | pass |
| C12 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Cross` | count unknown | - | pass |
| C13 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Share` | count unknown | - | pass |
| C14 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Depen` | count unknown | - | pass |
| C15 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Misna` | count unknown | - | pass |
| C16 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*NewModule` | count unknown | - | pass |
| C17 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Holds` | count unknown | - | pass |
| C18 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*RestoreIn` | count unknown | - | pass |
| C19 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*NonConven` | count unknown | - | pass |
| C20 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*HeaderLon` | count unknown | - | pass |
| C21 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*Conventio` | count unknown | - | pass |
| C22 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*GitGenera` | count unknown | - | pass |
| C23 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*Unformatt` | count unknown | - | pass |
| C24 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*CommitWit` | count unknown | - | pass |
| C25 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*RootHasCo` | count unknown | - | pass |
| C26 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*HookWitho` | count unknown | - | pass |
| C27 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*UnstagedF` | count unknown | - | pass |
| C28 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*RestoreTw` | count unknown | - | pass |
| C29 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*TemplateC` | count unknown | - | pass |
| C30 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*TemplateW` | count unknown | - | pass |
| C31 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*TemplateM` | count unknown | - | pass |
| C32 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*ReadmeDoc` | count unknown | - | pass |
| C33 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*Contribut` | count unknown | - | pass |
| C34 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*Architect` | count unknown | - | pass |
| C35 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*LicenseIs` | count unknown | - | pass |
| C36 | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*Multi` | count unknown | - | pass |
| C37 | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*StagedCon` | count unknown | - | pass |
| Build | `dotnet build Gibeira.slnx -warnaserror` | - | - | pass |
| Build | `dotnet format Gibeira.slnx --verify-no-changes` | - | - | pass |
| Build | `dotnet test --solution Gibeira.slnx` | - | - | pass |

## Mutations

| Mutation | Location | Killed |
| --- | --- | --- |
| commit-msg: type perf dropped from the allowed types | `.husky\csx\commit-lint.csx:14` | yes |
| ARCH-03: Application layer no longer checked for ASP.NET Core / EF Core | `tests\Gibeira.ArchitectureTests\LayerRules.cs:23` | yes |
| ARCH-05: SharedKernel -> AppHost no longer reported | `tests\Gibeira.ArchitectureTests\BoundaryRules.cs:20` | yes |
