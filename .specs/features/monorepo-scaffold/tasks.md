# Monorepo Scaffold Tasks

## Execution Protocol (MANDATORY -- do not skip)

Implement these tasks with the `tlc-spec-driven` skill: **activate it by name and follow its Execute flow and Critical Rules.** Do not search for skill files by filesystem path. The skill is the source of truth for the full flow (per-task cycle, sub-agent delegation, the walk, check_proofs, the Verifier and the screens sub-agent).

**If the skill cannot be activated, STOP and tell the user - do not proceed without it.**

---

**Design**: `.specs/features/monorepo-scaffold/design.md`
**Status**: Approved
**Approved**: 2026-10-29 - user: "approved"
**Mutations**: 3
**Build**: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`

---

## Test Coverage Matrix

> Generated from the approved spec and design. Guidelines found: none in the repo (empty) - strong defaults applied: every AC and every listed edge case has a test.

| Code Layer | Required Test Type | Coverage Expectation | Location Pattern | Run Command |
| ---------- | ------------------ | -------------------- | ---------------- | ----------- |
| Architecture rules | unit (ArchUnitNET) | Every rule passes on the solution and fails on its fixture, naming the offending type | `tests/Gibeira.ArchitectureTests/**/*.cs` | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj` |
| Hosts (Web, AppHost, ServiceDefaults) | integration (Aspire.Hosting.Testing) | Every SCAF runtime AC: home page, `/health`, loopback | `tests/Gibeira.IntegrationTests/**/*.cs` | `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj` |
| Repository tooling (hooks, template, structure, docs) | integration (real `git` / `dotnet` processes in a temp copy) | Every CONV, TMPL, DOCS AC and edge case: accept and reject paths | `tests/Gibeira.RepositoryTests/**/*.cs` | `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj` |
| Build config, empty projects, fixture assemblies | none | build gate only (config is asserted by RepositoryTests in T9) | - | build gate only |

## Gate Check Commands

| Gate Level | When to Use | Command |
| ---------- | ----------- | ------- |
| Quick | Architecture-rule tasks | `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj` |
| Full | Tasks with integration or repository tests | `dotnet test --solution Gibeira.slnx` |
| Build | Config-only tasks and phase ends | `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx` |

---

## Execution Plan

### Phase 1: Runnable solution

```
T1 → T2 → T3 → T4
```

### Phase 2: Architecture tests

```
T5 → T6 → T7 → T8
```

### Phase 3: Commit conventions

```
T9 → T10 → T11 → T12
```

### Phase 4: Template and docs

```
T13 → T14
```

---

## Task Breakdown

### T1: Root build configuration and repository init

**What**: `git init` on `main`, plus `global.json`, `Directory.Build.props`, `Directory.Packages.props`, `.editorconfig`, `.gitattributes`, `.gitignore` and an empty `Gibeira.slnx`, per design "Build configuration".
**Where**: repository root (config files)
**Depends on**: None
**Reuses**: `dotnet new globaljson`, `dotnet new editorconfig`, `dotnet new gitignore`
**Requirement**: SCAF-07, SCAF-08, CONV-08

**Done when**:

- [x] `global.json` selects SDK 10.0.403 with `test.runner: Microsoft.Testing.Platform`
- [x] `Directory.Build.props` sets net10.0, Nullable, TreatWarningsAsErrors, EnforceCodeStyleInBuild, central package management
- [x] `.specs/` committed with the config
- [x] Gate check passes: `dotnet build Gibeira.slnx -warnaserror` (replaced by a throwaway-project build, see Handoff)

**Tests**: none
**Gate**: build

**Commit**: `build: add root build configuration and solution`

**Status**: ✅ Complete

---

### T2: Gibeira.SharedKernel project

**What**: Empty class library `src/Shared/Gibeira.SharedKernel`, added to the solution.
**Where**: `src/Shared/Gibeira.SharedKernel/Gibeira.SharedKernel.csproj`
**Depends on**: T1
**Reuses**: `dotnet new classlib`
**Requirement**: SCAF-01

**Done when**:

- [x] Project has no `TargetFramework`, `Nullable` or package `Version` of its own (inherited)
- [x] Gate check passes: `dotnet build Gibeira.slnx -warnaserror`

**Tests**: none
**Gate**: build

**Commit**: `build(shared): add SharedKernel project`

**Status**: ✅ Complete

---

### T3: Blazor Web host, AppHost and integration tests

**What**: `Gibeira.Web` (Blazor Web App, Interactive Server, one home page with "Gibeira", localhost-only launch URLs), `Gibeira.AppHost` with a `web` project resource, and `tests/Gibeira.IntegrationTests` proving SCAF-04 and SCAF-06.
**Where**: `src/Host/Gibeira.Web/`, `src/Host/Gibeira.AppHost/`, `tests/Gibeira.IntegrationTests/WebHostTests.cs`
**Depends on**: T2
**Reuses**: `dotnet new blazor --interactivity Server`, Aspire AppHost SDK 13.6.4, `Aspire.Hosting.Testing`
**Requirement**: SCAF-04, SCAF-06

**Done when**:

- [x] `WebHomePageReturnsGibeira` and `WebEndpointsAreLoopbackOnly` pass
- [x] `--filter-method` with a name that matches nothing exits non-zero (confirms the MTP zero-test guard the proofs rely on)
- [x] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [x] Test count: 2 tests pass

**Tests**: integration
**Gate**: full

**Commit**: `feat(host): add Blazor web host and Aspire AppHost`

**Status**: ✅ Complete

---

### T4: Gibeira.ServiceDefaults wired into the Web host

**What**: `Gibeira.ServiceDefaults` (OpenTelemetry, service discovery, resilience, `/health`, `/alive`), called from `Gibeira.Web`, with the SCAF-05 test.
**Where**: `src/Shared/Gibeira.ServiceDefaults/Extensions.cs`, `tests/Gibeira.IntegrationTests/WebHostTests.cs`
**Depends on**: T3
**Reuses**: Aspire ServiceDefaults template shape
**Requirement**: SCAF-05

**Done when**:

- [x] `WebHealthEndpointReturnsOk` passes
- [x] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [x] Test count: 3 tests pass

**Tests**: integration
**Gate**: full

**Commit**: `feat(host): add ServiceDefaults with health endpoints`

**Status**: ✅ Complete

---

### T5: Architecture fixture assemblies

**What**: The six `Fixture.*` projects plus `Fixture.Fakes` from design "Fixture assemblies", each violating its rule, not part of any production reference.
**Where**: `tests/Gibeira.ArchitectureTests.Fixtures/`
**Depends on**: None (runs after Phase 1)
**Reuses**: `dotnet new classlib`
**Requirement**: ARCH-01, ARCH-02, ARCH-03, ARCH-04, ARCH-05, ARCH-06

**Done when**:

- [x] Every fixture project builds
- [x] No `src/` project references a fixture
- [x] Gate check passes: `dotnet build Gibeira.slnx -warnaserror`

**Tests**: none
**Gate**: build

**Commit**: `test(arch): add fixture assemblies with deliberate violations`

**Status**: ✅ Complete

---

### T6: Architecture test project and layer rules

**What**: `Gibeira.ArchitectureTests` with the prefix-based loader and module map, the glob reference to `src/Modules/**`, and rules ARCH-01, ARCH-02, ARCH-03 tested on fixture (fail) and solution (pass).
**Where**: `tests/Gibeira.ArchitectureTests/LayerRules.cs`
**Depends on**: T5
**Reuses**: `TngTech.ArchUnitNET.xUnitV3`
**Requirement**: ARCH-01, ARCH-02, ARCH-03, ARCH-09

**Done when**:

- [x] Each fixture test asserts the offending type's full name in the failure
- [x] Gate check passes: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj`
- [x] Test count: 6 tests pass

**Tests**: unit
**Gate**: quick

**Commit**: `test(arch): add layer dependency rules`

**Status**: ✅ Complete

---

### T7: Boundary rules

**What**: Rules ARCH-04 (module isolation), ARCH-05 (shared kernel), ARCH-06 (only AppHost depends on Web), fixture and solution tests.
**Where**: `tests/Gibeira.ArchitectureTests/BoundaryRules.cs`
**Depends on**: T6
**Reuses**: loader and module map from T6
**Requirement**: ARCH-04, ARCH-05, ARCH-06, ARCH-09

**Done when**:

- [x] ARCH-04 failure names both modules
- [x] Gate check passes: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj`
- [x] Test count: 12 tests pass

**Tests**: unit
**Gate**: quick

**Commit**: `test(arch): add module and host boundary rules`

**Status**: ✅ Complete

---

### T8: Module project naming rule

**What**: Rule ARCH-07 over `src/Modules/<Module>/**/*.csproj`, with a temp-folder fixture of misnamed projects and a pass on the real tree.
**Where**: `tests/Gibeira.ArchitectureTests/NamingRules.cs`
**Depends on**: T7
**Reuses**: repo-root locator
**Requirement**: ARCH-07, ARCH-09

**Done when**:

- [ ] Failure names the misnamed project
- [ ] Gate check passes: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj`
- [ ] Test count: 14 tests pass

**Tests**: unit
**Gate**: quick

**Commit**: `test(arch): add module project naming rule`

---

### T9: Repository test project and structure tests

**What**: `Gibeira.RepositoryTests` with the `TempRepo` fixture, and tests for SCAF-01, SCAF-07, SCAF-08, CONV-08.
**Where**: `tests/Gibeira.RepositoryTests/StructureTests.cs`
**Depends on**: None (runs after Phase 2)
**Reuses**: repo-root locator pattern from T8
**Requirement**: SCAF-01, SCAF-07, SCAF-08, CONV-08

**Done when**:

- [ ] Each test fails when its file or setting is removed (checked once by hand, then restored)
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [ ] Test count: 4 repository tests pass

**Tests**: integration
**Gate**: full

**Commit**: `test(repo): add repository structure tests`

---

### T10: Husky.Net installed on restore

**What**: Local tool manifest with `husky` 0.9.1, `.husky/` initialised, attach target in `Gibeira.ArchitectureTests.csproj`, tests for CONV-01 and repeat install.
**Where**: `tests/Gibeira.RepositoryTests/HookInstallTests.cs`
**Depends on**: T9
**Reuses**: `TempRepo`
**Requirement**: CONV-01

**Done when**:

- [ ] `RestoreInstallsGitHooks` and `RestoreTwiceLeavesHooksInstalled` pass
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [ ] Test count: 6 repository tests pass

**Tests**: integration
**Gate**: full

**Commit**: `build(hooks): install Husky.Net hooks on restore`

---

### T11: commit-msg hook

**What**: `.husky/commit-msg` with the `dotnet` guard, `commit-message-linter` task and `.husky/csx/commit-lint.csx`, tests for CONV-02..05 and the missing-dotnet edge case.
**Where**: `.husky/csx/commit-lint.csx`, `tests/Gibeira.RepositoryTests/CommitMessageHookTests.cs`
**Depends on**: T10
**Reuses**: `TempRepo`
**Requirement**: CONV-02, CONV-03, CONV-04, CONV-05

**Done when**:

- [ ] Rejection output contains the expected format
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [ ] Test count: 11 repository tests pass

**Tests**: integration
**Gate**: full

**Commit**: `build(hooks): enforce Conventional Commits on commit-msg`

---

### T12: pre-commit format hook

**What**: `.husky/pre-commit` with the `dotnet` guard and the `format` task over staged `*.cs`, tests for CONV-06, CONV-07 and staged-only checking.
**Where**: `.husky/task-runner.json`, `tests/Gibeira.RepositoryTests/FormatHookTests.cs`
**Depends on**: T11
**Reuses**: `TempRepo`
**Requirement**: CONV-06, CONV-07

**Done when**:

- [ ] Rejection output lists the unformatted file
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [ ] Test count: 14 repository tests pass

**Tests**: integration
**Gate**: full

**Commit**: `build(hooks): verify formatting of staged C# files on pre-commit`

---

### T13: Module template

**What**: `templates/module` (`gibeira-module`, `sourceName: ModuleName`) producing the three layer projects with the allowed references, and tests for TMPL-01..03 and ARCH-08 (a template module with an injected Domain→Infrastructure dependency fails the architecture tests).
**Where**: `templates/module/`, `tests/Gibeira.RepositoryTests/ModuleTemplateTests.cs`
**Depends on**: None (runs after Phase 3)
**Reuses**: `TempRepo`, `--debug:custom-hive`
**Requirement**: TMPL-01, TMPL-02, TMPL-03, ARCH-08

**Done when**:

- [ ] The user's global `dotnet new` template list is unchanged after the tests
- [ ] Gate check passes: `dotnet test --solution Gibeira.slnx`
- [ ] Test count: 18 repository tests pass

**Tests**: integration
**Gate**: full

**Commit**: `feat(template): add gibeira-module template`

---

### T14: Documentation and license

**What**: `README.md`, `CONTRIBUTING.md`, `docs/architecture.md`, MIT `LICENSE` ("Copyright (c) 2026 Pablo Souza"), with tests for DOCS-01..04.
**Where**: `docs/architecture.md`, `tests/Gibeira.RepositoryTests/DocumentationTests.cs`
**Depends on**: T13
**Reuses**: rule list from `tests/Gibeira.ArchitectureTests`
**Requirement**: DOCS-01, DOCS-02, DOCS-03, DOCS-04

**Done when**:

- [ ] `docs/architecture.md` names every ARCH rule ID
- [ ] Gate check passes: `dotnet build Gibeira.slnx -warnaserror; dotnet format Gibeira.slnx --verify-no-changes; dotnet test --solution Gibeira.slnx`
- [ ] Test count: 22 repository tests pass

**Tests**: integration
**Gate**: build

**Commit**: `docs: add README, contributing guide, architecture rules and license`

---

## Checks

**C1** - The solution has the agreed folders and projects, all in `Gibeira.slnx` (SCAF-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*SolutionContainsAgreedLayout"`

**C2** - A fresh build has 0 warnings and 0 errors (SCAF-02, AC 2)
Proof: `dotnet build Gibeira.slnx -warnaserror`

**C3** - Every test passes on the solution (SCAF-03, AC 3)
Proof: `dotnet test --solution Gibeira.slnx`

**C4** - `web` answers 200 on `/` with "Gibeira" in the body (SCAF-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebHomePageReturnsGibeira"`

**C5** - `web` answers 200 on `/health` (SCAF-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebHealthEndpointReturnsOk"`

**C6** - Every `web` endpoint host is loopback (SCAF-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.IntegrationTests/Gibeira.IntegrationTests.csproj --filter-method "*WebEndpointsAreLoopbackOnly"`

**C7** - Every project targets net10.0 with nullable and warnings-as-errors, none overriding them (SCAF-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*EveryProjectTargetsNet10WithNullableAndWarningsAsErrors"`

**C8** - No `.csproj` has a `Version` on a `PackageReference` (SCAF-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*NoProjectDeclaresPackageVersions"`

**C9** - Domain depending on its Application or Infrastructure fails, naming the type (ARCH-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*DomainDependingOnOuterLayersIsReported"`

**C10** - Application depending on Infrastructure fails, naming the type (ARCH-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*ApplicationDependingOnInfrastructureIsReported"`

**C11** - Domain or Application using ASP.NET Core or EF Core fails, naming the type (ARCH-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*InnerLayersUsingFrameworksAreReported"`

**C12** - One module depending on another fails, naming both modules (ARCH-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*CrossModuleDependencyIsReported"`

**C13** - SharedKernel depending on a module or host fails (ARCH-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*SharedKernelDependingOnModulesOrHostsIsReported"`

**C14** - Anything but AppHost depending on Web fails (ARCH-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*DependencyOnWebOutsideAppHostIsReported"`

**C15** - A misnamed project under `src/Modules/<Module>/` fails, naming the project (ARCH-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*MisnamedModuleProjectIsReported"`

**C16** - A new module is checked by every rule with no test edits: an injected Domain→Infrastructure dependency in a template module fails the architecture tests (ARCH-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*NewModuleIsCoveredByArchitectureTests"`

**C17** - Every rule passes on the scaffolded solution (ARCH-09, AC 9)
Proof: `dotnet test --project tests/Gibeira.ArchitectureTests/Gibeira.ArchitectureTests.csproj --filter-method "*HoldsOnSolution"`

**C18** - `dotnet restore` sets `core.hooksPath` to `.husky` with `commit-msg` and `pre-commit` present (CONV-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*RestoreInstallsGitHooks"`

**C19** - A non-conventional header is rejected and the expected format is printed (CONV-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*NonConventionalHeaderIsRejected"`

**C20** - A 101-character header is rejected; a 100-character one is accepted (CONV-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*HeaderLongerThan100CharactersIsRejected"`

**C21** - A conventional header is accepted alone and with a body and trailers (CONV-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*ConventionalHeaderIsAccepted"`

**C22** - Git's `Merge ...` and `Revert "..."` messages are accepted (CONV-05, AC 5)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*GitGeneratedMessagesAreAccepted"`

**C23** - A staged unformatted `.cs` file is rejected and listed (CONV-06, AC 6)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*UnformattedStagedCsFileIsRejected"`

**C24** - A commit with no staged `.cs` file passes without running `dotnet format` (CONV-07, AC 7)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*CommitWithoutCsFilesSkipsFormat"`

**C25** - `.editorconfig`, `.gitattributes` and `.gitignore` exist at the root (CONV-08, AC 8)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*RootHasConventionFiles"`

**C26** - Without `dotnet` on PATH the hook fails and names `dotnet` (Edge case 1)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*HookWithoutDotnetNamesIt"`

**C27** - An unstaged unformatted `.cs` file does not block a commit (Edge case 2)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*UnstagedFilesAreNotChecked"`

**C28** - A second restore succeeds and leaves the hooks installed (Edge case 3)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*RestoreTwiceLeavesHooksInstalled"`

**C29** - `dotnet new gibeira-module -n Probe` creates the three layer projects under `src/Modules/Probe/` (TMPL-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*TemplateCreatesLayerProjects"`

**C30** - Template references are exactly Application→Domain, Infrastructure→Application, Infrastructure→Domain (TMPL-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*TemplateWiresOnlyAllowedReferences"`

**C31** - A template module added to the solution builds and passes the architecture tests (TMPL-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*TemplateModuleBuildsAndPassesRules"`

**C32** - README documents prerequisites, build, test and run (DOCS-01, AC 1)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*ReadmeDocumentsBuildTestAndRun"`

**C33** - CONTRIBUTING documents commit format, hook setup and the template command (DOCS-02, AC 2)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*ContributingDocumentsConventions"`

**C34** - `docs/architecture.md` lists every ARCH rule (DOCS-03, AC 3)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*ArchitectureDocListsEveryRule"`

**C35** - `LICENSE` holds the MIT text (DOCS-04, AC 4)
Proof: `dotnet test --project tests/Gibeira.RepositoryTests/Gibeira.RepositoryTests.csproj --filter-method "*LicenseIsMit"`

---

## Handoff

**Settled mid-build:** user approved tasks without choosing inline vs sub-agents; executing inline (agent recommendation). T1: the solution build gate cannot pass on an empty `Gibeira.slnx` (NuGet restore: no project to restore), so T1's config was proven by a throwaway classlib under the root inheriting every property and building with 0 warnings; the solution gate starts at T2. User: "Commit T1, note gate (Recommended)" T2: build and format gates pass; `dotnet test --solution` exits 1 with "no test project found" until T3 adds the first test project. T3: (a) the template layout links `Gibeira.Web.styles.css`, so every page body contains "Gibeira"; SCAF-04's "contains Gibeira" is too loose to prove anything, so `WebHomePageReturnsGibeira` asserts `<h1>Gibeira</h1>` (spec-precision gap). (b) Tests start the AppHost with `DOTNET_LAUNCH_PROFILE=http`: under the default `https` profile `web` redirects http to https and the untrusted dev certificate fails the call. (c) `Gibeira.AppHost` suppresses ASPIRE010 and keeps DCP and the dashboard from NuGet, so a clone needs no Aspire CLI. (d) Zero-match `--filter-method` exits 8. Mutation check: Web `http` profile on `0.0.0.0` fails `WebEndpointsAreLoopbackOnly`. T4: no Aspire templates are installed, so `Gibeira.ServiceDefaults` reproduces the Aspire 13 template shape by hand; packages pinned to latest stable (OpenTelemetry 1.20.1, Http.Resilience and ServiceDiscovery 10.11.0). `/health` and `/alive` stay Development-only per design. `WebHealthEndpointReturnsOk` was red (404) before `MapDefaultEndpoints` was wired. Full build gate (build, format, test) passes.
**Dispatched:** none
**Walked:** none

---

## Phase Execution Map

```
Phase 1 → Phase 2 → Phase 3 → Phase 4

Phase 1:  T1 ------→ T2 ------→ T3 ------→ T4
Phase 2:  T5 ------→ T6 ------→ T7 ------→ T8
Phase 3:  T9 ------→ T10 -----→ T11 -----→ T12
Phase 4:  T13 -----→ T14
```

Batches (~7 tasks, whole phases): batch 1 = Phases 1-2 (T1-T8), batch 2 = Phases 3-4 (T9-T14).
