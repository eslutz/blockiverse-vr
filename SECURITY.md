# Security Policy

## Reporting

Report security issues privately to the maintainer. Do not open public issues for vulnerabilities, leaked credentials, signing keys, or store account problems.

## Secrets

The repository must never contain:

- Android keystores
- Keystore passwords
- Unity credentials
- Meta app secrets
- API credentials
- `.env` files
- Private signing or provisioning files

If a secret is committed, rotate it immediately and treat the git history as compromised.

## Supported Versions

Until the first public release, only the current `main` branch is supported.
