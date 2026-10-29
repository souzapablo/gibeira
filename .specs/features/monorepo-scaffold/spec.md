# Monorepo Scaffold Specification

**Approved**: 2026-10-29 - user: "approved"

## Problem Statement

Gibeira will be a self-hosted, open-source replacement for paid personal-finance apps. It is also the lab where the author keeps up with .NET, Blazor and spec-driven development. No code exists yet. Before any feature lands, the repository needs a fixed structure and rules that hold on every commit, so the first module and every one after it arrive organized.

## Goals

- [ ] A fresh clone builds with zero warnings and passes all tests with `dotnet build` and `dotnet test`.
- [ ] One command (`dotnet run --project src/Host/Gibeira.AppHost`) starts the Blazor app and the Aspire dashboard.
- [ ] Commits with a non-conventional message or unformatted C# are rejected locally.
- [ ] Architecture tests fail on every forbidden dependency between layers and modules, proven against deliberate violations.
- [ ] A new module is created from a template with one command and is covered by the architecture tests without editing them.

## Out of Scope

| Feature | Reason |
| ------- | ------ |
| Any finance module or business code (Ledger, Budgets, ...) | User: "empty template for now, no real code. just preparation so when code lands it is organized." |
| Database and EF Core | User chose "No database yet"; the DB is picked in the first real feature |
| Separate `labs/` area | User: "the whole app is a lab" |
| Build + tests on `git push` | Not selected by the user |
| GitHub Actions / any CI | Not selected by the user; hooks are local and `git commit --no-verify` bypasses them |
| Authentication / authorization | Single user on their own machine; revisited when data exists |
| Deployment, containers, publishing | Local-only hosting via `dotnet run` |
| Creating a GitHub remote or pushing | Remote operations need a separate go-ahead |

---

## Assumptions & Open Questions

| Assumption / decision | Chosen default | Rationale | Confirmed? |
| --------------------- | -------------- | --------- | ---------- |
| Architecture | Modular monolith; each module has Domain, Application, Infrastructure projects; one shared kernel; one Blazor host | User selected "Modular monolith + layers" | y |
| Blazor hosting model | Blazor Web App, Interactive Server render mode | User selected "Interactive Server" | y |
| Convention enforcement | `commit-msg` hook (Conventional Commits) and `pre-commit` hook (`dotnet format --verify-no-changes`) | User selected these two; push checks and CI not selected | y |
| License | MIT, copyright "2026 Pablo Souza" (from `git config user.name`) | User selected MIT | y (license); n (holder name) |
| Labs area | None; the whole app is the lab | User: "the whole app is a lab" | y |
| First modules | None; a module template only | User: "empty template for now, no real code" | y |
| Aspire | AppHost + ServiceDefaults projects | User: "yes" | y |
| Target framework | `net10.0`; `global.json` pins SDK 10.0.x with `rollForward: latestFeature` | SDK 10.0.403 is installed; .NET 10 is the current LTS | n |
| Solution file | `Gibeira.slnx` (XML solution format, default in .NET 10) | Current .NET default | n |
| Project naming | `Gibeira.<Module>.<Layer>` under `src/Modules/<Module>/`; non-module projects: `Gibeira.Web`, `Gibeira.SharedKernel`, `Gibeira.AppHost`, `Gibeira.ServiceDefaults` | Matches the layout the user picked | n |
| Cross-module communication | Forbidden: no module references another module. A `Contracts` project per module is added by the first feature that needs one | No code needs it yet; adding it later is cheap | n |
| Build strictness | Nullable enabled, `TreatWarningsAsErrors`, `EnforceCodeStyleInBuild`, `AnalysisLevel latest-recommended`, central package management (`Directory.Packages.props`) | Conventions fail the build, not only the hook | n |
| Hook tooling | Husky.Net as a local dotnet tool; hooks installed automatically on `dotnet restore` of the solution | .NET-only toolchain, no Node dependency | n |
| Commit message format | Conventional Commits 1.0: `type(scope)!: subject`, types `feat fix docs style refactor perf test build ci chore revert`, header ≤ 100 chars; git-generated `Merge ...` and `Revert "..."` messages pass | Common commitlint defaults | n |
| Test framework | xUnit v3 | Most used .NET test framework | n |
| Architecture test library | ArchUnitNET | Rule DSL covers layer and namespace dependencies; package availability for xUnit v3 confirmed in Design | n |
| Proving rules with no modules | A test-only fixture module that violates each rule; every rule must fail on it and pass on the real solution | With zero modules, rules pass vacuously and prove nothing | n |
| Module template | A `dotnet new` template (`gibeira-module`) in `templates/`, installed from the repo | One command creates a module that already obeys the rules | n |
| Web host binding | Loopback only (`localhost`) | Finance data stays on the machine; LAN access is a later decision | n |
| `.specs/` in git | Tracked | Spec-driven development is part of what the repo demonstrates | n |
| Docs language | English | Request written in English; open-source audience | n |

**Open questions:** none - the defaults marked `n` are for the user to correct before approval.

---

## User Stories

### P1: Structured solution that builds and runs ⭐ MVP

**User Story**: As the developer, I want a solution with the agreed layout that builds and runs with one command so that every future feature has an obvious home.

**Why P1**: Nothing else can land without it.

**Acceptance Criteria**:

1. The repository SHALL contain `src/Host/Gibeira.Web`, `src/Host/Gibeira.AppHost`, `src/Shared/Gibeira.SharedKernel`, `src/Shared/Gibeira.ServiceDefaults`, `src/Modules/`, `tests/Gibeira.ArchitectureTests`, `templates/`, `docs/`, all referenced from `Gibeira.slnx`.
2. WHEN `dotnet build Gibeira.slnx` runs on a fresh clone THEN the build SHALL succeed with 0 warnings and 0 errors.
3. WHEN `dotnet test Gibeira.slnx` runs on a fresh clone THEN every test SHALL pass.
4. WHEN the AppHost starts THEN the `web` resource SHALL respond with HTTP 200 on `/` and the response SHALL contain the text "Gibeira".
5. WHEN the AppHost starts THEN the `web` resource SHALL respond with HTTP 200 on `/health`.
6. The Web host SHALL listen on loopback addresses only.
7. Every project SHALL target `net10.0` with nullable reference types enabled and warnings treated as errors.
8. Every NuGet package version SHALL be declared in `Directory.Packages.props` and no `.csproj` SHALL carry a `Version` attribute on a `PackageReference`.

**Independent Test**: Clone, `dotnet build`, `dotnet test`, then `dotnet run --project src/Host/Gibeira.AppHost` and open the web resource from the dashboard.

---

### P1: Architecture rules enforced by tests ⭐ MVP

**User Story**: As the developer, I want tests that fail when code crosses a layer or module boundary so that the architecture holds without me policing it.

**Why P1**: The user asked for architectural tests explicitly.

**Acceptance Criteria**:

1. IF a Domain assembly depends on its module's Application or Infrastructure assembly THEN the architecture tests SHALL fail naming the offending type.
2. IF an Application assembly depends on its module's Infrastructure assembly THEN the architecture tests SHALL fail naming the offending type.
3. IF a Domain or Application assembly depends on ASP.NET Core or Entity Framework Core namespaces THEN the architecture tests SHALL fail naming the offending type.
4. IF an assembly of one module depends on an assembly of another module THEN the architecture tests SHALL fail naming both modules.
5. IF `Gibeira.SharedKernel` depends on any module, `Gibeira.Web` or `Gibeira.AppHost` THEN the architecture tests SHALL fail.
6. IF any assembly other than `Gibeira.AppHost` depends on `Gibeira.Web` THEN the architecture tests SHALL fail.
7. IF a project under `src/Modules/<Module>/` is not named `Gibeira.<Module>.Domain`, `Gibeira.<Module>.Application` or `Gibeira.<Module>.Infrastructure` THEN the architecture tests SHALL fail naming the project.
8. WHEN a module is added under `src/Modules/` THEN the architecture tests SHALL apply every rule to it without changes to the test project.
9. The architecture tests SHALL pass on the scaffolded solution.

**Independent Test**: Run `dotnet test tests/Gibeira.ArchitectureTests`: each rule passes on the solution and fails on its fixture violation.

---

### P1: Conventions enforced on commit ⭐ MVP

**User Story**: As the developer, I want commits rejected when the message or the formatting breaks the conventions so that history and code stay consistent.

**Why P1**: The user asked for conventions enforced on commits.

**Acceptance Criteria**:

1. WHEN `dotnet restore Gibeira.slnx` runs in a clone THEN the `commit-msg` and `pre-commit` git hooks SHALL be installed.
2. IF a commit message header does not match `type(scope)!: subject` with a type from `feat fix docs style refactor perf test build ci chore revert` THEN the `commit-msg` hook SHALL reject the commit and print the expected format.
3. IF a commit message header is longer than 100 characters THEN the `commit-msg` hook SHALL reject the commit.
4. WHEN a commit message header matches the format THEN the `commit-msg` hook SHALL accept it, with or without a body and trailers.
5. WHEN git generates a `Merge ...` or `Revert "..."` message THEN the `commit-msg` hook SHALL accept it.
6. IF a staged `.cs` file violates `.editorconfig` formatting THEN the `pre-commit` hook SHALL reject the commit and list the file.
7. WHEN no staged file is a `.cs` file THEN the `pre-commit` hook SHALL pass without running `dotnet format`.
8. The repository SHALL contain `.editorconfig`, `.gitattributes` and `.gitignore` at its root.

**Independent Test**: Try `git commit -m "added stuff"` (rejected), then `git commit -m "chore: test"` (accepted); stage a misformatted `.cs` file and commit (rejected).

---

### P2: Module template

**User Story**: As the developer, I want to create a new module with one command so that it starts with the right projects, names and references.

**Why P2**: The first feature needs it; the scaffold itself does not.

**Acceptance Criteria**:

1. WHEN `dotnet new gibeira-module -n <Name>` runs from the repo root after installing the template THEN it SHALL create `src/Modules/<Name>/` with `Gibeira.<Name>.Domain`, `Gibeira.<Name>.Application` and `Gibeira.<Name>.Infrastructure`.
2. WHEN a module is created from the template THEN Application SHALL reference Domain, and Infrastructure SHALL reference Application and Domain, and no other project references SHALL exist between them.
3. WHEN a module created from the template is added to the solution THEN `dotnet build` and the architecture tests SHALL pass.

**Independent Test**: Create a throwaway module from the template in a temp copy of the repo, add it to the solution, build and run the architecture tests.

---

### P2: Documentation

**User Story**: As a contributor, I want the rules written down so that I can follow them without reading the tests.

**Why P2**: Open source; the hooks explain failures, the docs explain why.

**Acceptance Criteria**:

1. The repository SHALL contain `README.md` with prerequisites, how to build, test and run.
2. The repository SHALL contain `CONTRIBUTING.md` with the commit message format, the hook setup and the module template command.
3. The repository SHALL contain `docs/architecture.md` listing every rule from the architecture-tests story.
4. The repository SHALL contain a `LICENSE` file with the MIT license text.

**Independent Test**: Read the files; each rule in `docs/architecture.md` maps to a test.

---

## Edge Cases

- IF the .NET SDK is missing when a hook runs THEN the hook SHALL fail with a message naming the missing `dotnet` command.
- WHEN the `pre-commit` hook runs THEN it SHALL check only staged `.cs` files.
- WHEN hook installation runs a second time THEN it SHALL leave the hooks unchanged and succeed.

## Implicit-requirement sweep

| Dimension | Resolution |
| --------- | ---------- |
| Input validation & bounds | Commit header format and 100-char limit (CONV-02, CONV-03) |
| Failure / partial-failure states | Hook fails clearly when `dotnet` is missing (Edge Cases) |
| Idempotency / retry | Repeated hook installation is a no-op (Edge Cases) |
| Auth boundaries & rate limits | Loopback-only binding (SCAF-06); auth N/A because there is no data yet |
| Concurrency / ordering | N/A because the scaffold has no shared state |
| Data lifecycle / expiry | N/A because there is no database |
| Observability | ServiceDefaults wires OpenTelemetry and `/health` (SCAF-05) |
| External-dependency failure | N/A because the scaffold calls no external service |
| State-transition integrity | N/A because the scaffold has no stateful entities |

---

## Requirement Traceability

| Requirement ID | Story | Phase | Status |
| -------------- | ----- | ----- | ------ |
| SCAF-01 | P1: Structured solution | Design | In Design |
| SCAF-02 | P1: Structured solution | Design | In Design |
| SCAF-03 | P1: Structured solution | Design | In Design |
| SCAF-04 | P1: Structured solution | Execute | T3 done |
| SCAF-05 | P1: Structured solution | Design | In Design |
| SCAF-06 | P1: Structured solution | Execute | T3 done |
| SCAF-07 | P1: Structured solution | Execute | T1 done; test in T9 |
| SCAF-08 | P1: Structured solution | Execute | T1 done; test in T9 |
| ARCH-01 | P1: Architecture rules | Design | In Design |
| ARCH-02 | P1: Architecture rules | Design | In Design |
| ARCH-03 | P1: Architecture rules | Design | In Design |
| ARCH-04 | P1: Architecture rules | Design | In Design |
| ARCH-05 | P1: Architecture rules | Design | In Design |
| ARCH-06 | P1: Architecture rules | Design | In Design |
| ARCH-07 | P1: Architecture rules | Design | In Design |
| ARCH-08 | P1: Architecture rules | Design | In Design |
| ARCH-09 | P1: Architecture rules | Design | In Design |
| CONV-01 | P1: Conventions on commit | Design | In Design |
| CONV-02 | P1: Conventions on commit | Design | In Design |
| CONV-03 | P1: Conventions on commit | Design | In Design |
| CONV-04 | P1: Conventions on commit | Design | In Design |
| CONV-05 | P1: Conventions on commit | Design | In Design |
| CONV-06 | P1: Conventions on commit | Design | In Design |
| CONV-07 | P1: Conventions on commit | Design | In Design |
| CONV-08 | P1: Conventions on commit | Execute | T1 done; test in T9 |
| TMPL-01 | P2: Module template | Design | In Design |
| TMPL-02 | P2: Module template | Design | In Design |
| TMPL-03 | P2: Module template | Design | In Design |
| DOCS-01 | P2: Documentation | Design | In Design |
| DOCS-02 | P2: Documentation | Design | In Design |
| DOCS-03 | P2: Documentation | Design | In Design |
| DOCS-04 | P2: Documentation | Design | In Design |

**Coverage:** 32 total, 0 mapped to tasks, 32 unmapped (tasks not written yet)

## Sources

- User request (2026-10-29): monorepo for learning .NET, Blazor and SDD that replaces paid finance apps; local hosting; open source; scaffold only; conventions enforced on commits; architectural tests.
- Clarification answers (2026-10-29): recorded in Assumptions with `y`.

---

## Success Criteria

- [ ] A fresh clone goes from `git clone` to a running app with three commands: `dotnet build`, `dotnet test`, `dotnet run --project src/Host/Gibeira.AppHost`.
- [ ] Every architecture rule has a fixture that makes it fail.
- [ ] A non-conventional commit message never reaches history without `--no-verify`.
