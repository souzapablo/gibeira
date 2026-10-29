#!/bin/sh
# Verifies the formatting of the staged content of every staged .cs file (CONV-06).
# dotnet format reads files from disk, so a file whose working copy differs from the index
# is checked through a copy of its staged blob, written next to it so its project picks it up.
set -eu

staged=$(git diff --cached --name-only --diff-filter=ACMR -- '*.cs')
[ -n "$staged" ] || exit 0

copies=""
trap 'printf "%s" "$copies" | while IFS= read -r copy; do rm -f "$copy"; done' EXIT

set --
while IFS= read -r file; do
  if git diff --quiet -- "$file"; then
    set -- "$@" "$file"
  else
    copy="${file%.cs}.staged.cs"
    if [ -e "$copy" ]; then
      echo "$copy already exists: cannot check the staged content of $file"
      exit 1
    fi
    git show ":$file" > "$copy"
    copies="$copies$copy
"
    set -- "$@" "$copy"
  fi
done <<STAGED
$staged
STAGED

status=0
output=$(dotnet format Gibeira.slnx --verify-no-changes --include "$@" 2>&1) || status=$?
printf '%s\n' "$output" | sed 's/\.staged\.cs/.cs/g'
exit "$status"
