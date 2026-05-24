# Testing

Testing is split into:

- Repository foundation checks for governance files, Unity Git settings, branch policy, CI workflow triggers, and forbidden tracked files
- Main and release APK workflow checks that publish CI artifacts, with explicit placeholder artifacts until the Unity project exists
- EditMode tests for pure C# logic
- PlayMode tests for Unity-connected systems
- Multiplayer Play Mode tests for local multi-client behavior
- Manual Quest 3 and Quest 3S smoke tests
- OVR Metrics performance captures
- Store-readiness validation before submission

Performance reports belong in `docs/testing/performance/`.

Run the repository checks locally with:

```sh
scripts/ci/test-repository-foundation.sh
scripts/ci/repository-foundation.sh
scripts/ci/forbidden-files.sh
```
