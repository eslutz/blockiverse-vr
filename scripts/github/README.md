# GitHub Bootstrap

Run the roadmap bootstrap after authenticating GitHub CLI:

```sh
gh auth login -h github.com
gh auth refresh -s project
scripts/github/bootstrap-roadmap.sh
```

Optional environment variables:

```sh
OWNER=your-github-user-or-org
REPO=your-github-user-or-org/blockiverse-vr
PROJECT_TITLE="Blockiverse VR Roadmap"
```

The script creates or configures:

- Public GitHub repository
- `main` branch remote
- Labels
- Milestones
- GitHub Project fields
- Epic, feature, and story issues from `roadmap.tsv`
- Issue parent/child references
- Best-effort `main` branch protection
