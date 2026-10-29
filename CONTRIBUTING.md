# Contributing

## Git hooks

`dotnet restore Gibeira.slnx` (and so any `dotnet build` or `dotnet test`) installs the git hooks. It restores the local [Husky.Net](https://alirezanet.github.io/Husky.Net/) tool and points `core.hooksPath` at `.husky/`. Running it again changes nothing.

Two hooks run on every commit:

| Hook | Checks |
| ---- | ------ |
| `pre-commit` | `dotnet format --verify-no-changes` on the staged `.cs` files. A commit with no staged `.cs` file skips it. |
| `commit-msg` | The message header follows the commit format below. |

Both hooks need the `dotnet` command; without it they stop the commit and say so.

- `HUSKY=0` skips the install and the hooks.
- `git commit --no-verify` skips the hooks for one commit. There is no CI to catch what it lets through.

### Fixing a formatting failure

The `pre-commit` hook lists each file and line it rejects. Fix them with:

```sh
dotnet format Gibeira.slnx
```

Then stage the fixed files again. The hook checks the files as they are on disk, so a file fixed but not re-staged passes the hook while the commit still holds the unformatted version.

## Commit format

Commits follow [Conventional Commits 1.0](https://www.conventionalcommits.org/en/v1.0.0/):

```text
type(scope)!: subject

optional body

optional trailers
```

- `type` is one of `feat fix docs style refactor perf test build ci chore revert`.
- `(scope)` is optional: lowercase letters, digits and dashes, such as `(ledger)` or `(web)`.
- `!` is optional and marks a breaking change.
- The header is at most 100 characters.
- Messages git writes itself, `Merge ...` and `Revert "..."`, are accepted as they are.

Examples:

```text
feat(ledger): add account import
fix(web): handle an empty home page
build!: move to .NET 11
```

## Adding a module

Install the template once from the repository root:

```sh
dotnet new install ./templates/module
```

Then create the module from the repository root:

```sh
dotnet new gibeira-module -n Budgets
```

This creates `src/Modules/Budgets/` with `Gibeira.Budgets.Domain`, `Gibeira.Budgets.Application` and `Gibeira.Budgets.Infrastructure`, wired as Application → Domain and Infrastructure → Application and Domain. Add them to the solution:

```sh
dotnet sln Gibeira.slnx add --solution-folder src/Modules/Budgets src/Modules/Budgets/*/*.csproj
```

The architecture tests pick the module up on their own; see [docs/architecture.md](docs/architecture.md) for the rules it must follow.
