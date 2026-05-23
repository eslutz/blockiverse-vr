# Contributing

## Branches

Use short-lived branches from `main`:

- `feature/*`
- `fix/*`
- `chore/*`
- `spike/*`
- `hotfix/*`

Do not create a long-lived `develop` branch or long-lived release branches.

## Pull Requests

Pull requests should include:

- Summary of player-facing and technical changes
- Test evidence
- Manual validation steps when VR, performance, save/load, networking, signing, or store behavior changes
- Linked issue or roadmap item

Incomplete systems should stay behind feature flags, disabled scenes, or clearly isolated test scenes so `main` remains playable.

## Assets

Only commit assets that are original or redistributable. Record AI-generated asset prompts, generation date/tool, and post-processing steps in `docs/art-direction/prompt-log.md`.

## Secrets

Never commit secrets, keystores, signing credentials, `.env` files, local Unity generated folders, or private platform credentials.
