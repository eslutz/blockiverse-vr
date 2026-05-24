#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
ROADMAP_FILE="$ROOT_DIR/scripts/github/roadmap.tsv"

cd "$ROOT_DIR"

if ! command -v gh >/dev/null 2>&1; then
  echo "Missing required tool: gh" >&2
  exit 1
fi

if ! command -v jq >/dev/null 2>&1; then
  echo "Missing required tool: jq" >&2
  exit 1
fi

if ! gh auth status >/dev/null 2>&1; then
  echo "GitHub CLI is not authenticated with a valid token." >&2
  exit 1
fi

OWNER="${OWNER:-$(gh api user --jq '.login')}"
REPO="${REPO:-$OWNER/blockiverse-vr}"

tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT

issues_file="$tmp_dir/issues.tsv"
roadmap_file="$tmp_dir/roadmap.tsv"

gh api --paginate "repos/$REPO/issues?state=all&per_page=100" \
  --jq '.[] | select(.pull_request | not) | [.title, .number, .node_id] | @tsv' > "$issues_file"

tail -n +2 "$ROADMAP_FILE" > "$roadmap_file"

issue_by_key() {
  local key="$1"
  local title
  title="$(awk -F '|' -v key="$key" '$1 == key { print $3; exit }' "$roadmap_file")"
  if [ -z "$title" ]; then
    return 1
  fi

  awk -F '\t' -v title="$title" '$1 == title { print $2 "\t" $3; exit }' "$issues_file"
}

link_subissue() {
  local parent_key="$1"
  local child_key="$2"
  local parent_info child_info parent_number child_number parent_id child_id

  parent_info="$(issue_by_key "$parent_key")"
  child_info="$(issue_by_key "$child_key")"

  if [ -z "$parent_info" ] || [ -z "$child_info" ]; then
    echo "Missing issue for parent $parent_key or child $child_key" >&2
    return 1
  fi

  parent_number="$(printf '%s' "$parent_info" | cut -f1)"
  parent_id="$(printf '%s' "$parent_info" | cut -f2)"
  child_number="$(printf '%s' "$child_info" | cut -f1)"
  child_id="$(printf '%s' "$child_info" | cut -f2)"

  local response
  if ! response="$(gh api graphql \
    -f query="mutation { addSubIssue(input:{issueId:\"$parent_id\",subIssueId:\"$child_id\",replaceParent:true}) { issue { number } subIssue { number } } }" 2>&1)"; then
    if printf '%s' "$response" | grep -q "duplicate sub-issues"; then
      echo "Already linked #$child_number under #$parent_number"
      return 0
    fi

    echo "Failed linking #$child_number under #$parent_number" >&2
    printf '%s\n' "$response" >&2
    return 1
  fi

  echo "Linked #$child_number under #$parent_number"
}

echo "Linking features to epics and stories to features in $REPO"

while IFS='|' read -r child_key _title_type _title parent_key _phase _milestone _area _priority _risk _target _effort; do
  if [ -z "$child_key" ] || [ -z "$parent_key" ]; then
    continue
  fi

  link_subissue "$parent_key" "$child_key"
done < "$roadmap_file"

echo "Sub-issue links complete."
