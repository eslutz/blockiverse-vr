#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ROADMAP_FILE="$ROOT_DIR/scripts/github/roadmap.tsv"

cd "$ROOT_DIR"

if ! command -v gh >/dev/null 2>&1; then
  echo "Missing required tool: gh" >&2
  exit 1
fi

OWNER="${OWNER:-$(gh api user --jq '.login')}"
REPO="${REPO:-$OWNER/blockiverse-vr}"
REPO_OWNER="${REPO%%/*}"
REPO_NAME="${REPO#*/}"

tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT

issues_file="$tmp_dir/issues.tsv"
roadmap_file="$tmp_dir/roadmap.tsv"
failures_file="$tmp_dir/failures.txt"

gh api --paginate "repos/$REPO/issues?state=all&per_page=100" \
  --jq '.[] | select(.pull_request | not) | [.title, .number] | @tsv' > "$issues_file"

tail -n +2 "$ROADMAP_FILE" > "$roadmap_file"
touch "$failures_file"

issue_number_for_key() {
  local key="$1"
  local title
  title="$(awk -F '|' -v key="$key" '$1 == key { print $3; exit }' "$roadmap_file")"
  awk -F '\t' -v title="$title" '$1 == title { print $2; exit }' "$issues_file"
}

checked=0

while IFS='|' read -r child_key _type _title parent_key _phase _milestone _area _priority _risk _target _effort; do
  if [ -z "$child_key" ] || [ -z "$parent_key" ]; then
    continue
  fi

  child_number="$(issue_number_for_key "$child_key")"
  expected_parent_number="$(issue_number_for_key "$parent_key")"

  actual_parent_number="$(gh api graphql \
    -f query="query { repository(owner:\"$REPO_OWNER\", name:\"$REPO_NAME\") { issue(number:$child_number) { parent { number } } } }" \
    --jq '.data.repository.issue.parent.number // ""')"

  checked=$((checked + 1))

  if [ "$actual_parent_number" != "$expected_parent_number" ]; then
    printf 'Issue #%s expected parent #%s but found #%s\n' "$child_number" "$expected_parent_number" "${actual_parent_number:-none}" >> "$failures_file"
  fi
done < "$roadmap_file"

if [ -s "$failures_file" ]; then
  cat "$failures_file" >&2
  exit 1
fi

echo "Verified $checked native sub-issue parent links."
