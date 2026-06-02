# CI/CD Pipeline

The `jukebox-data-access` repository uses CircleCI. The pipeline builds, tests, scans, and publishes three NuGet packages to GitHub Packages.

---

## Trigger Branches

| Branch pattern | What runs |
|---|---|
| `main` | Full pipeline — build, test, publish packages |
| `feature/*` | Full pipeline — build, test, SonarCloud scan, publish packages |
| All other branches | No pipeline |

---

## Jobs

```
build-and-test
      │
      ├─ resolve-version  (parallel)
      ├─ sonar-scan       (parallel, feature/* only)
      │
      ▼
publish-contracts
      │
      ▼
publish-entityframework
      │
      ▼
publish-dataaccess
```

### `build-and-test`

Builds the solution and runs all tests.

Steps:
1. `dotnet restore` — authenticates to GitHub Packages via `GITHUB_TOKEN`
2. `dotnet build`
3. `dotnet test` with `coverlet.msbuild` coverage in OpenCover format
4. Persists coverage report as a workspace artifact for `sonar-scan`

Expected: **76 tests, all passing**.

### `resolve-version`

Determines the NuGet package version for this build. Runs in parallel with `sonar-scan` after `build-and-test` completes.

Version strategy:
- Reads from a version file or tag in the repository
- On `feature/*`: produces a pre-release version (e.g. `1.0.0-beta.<sha>`)
- On `main`: produces a release version (e.g. `1.0.0`)

### `sonar-scan`

Runs SonarCloud static analysis. Only triggered on `feature/*` branches.

- Attaches workspace to read the coverage report from `build-and-test`
- SonarCloud project key: `jonwong84_jukebox-data-access`
- Authenticated via `SONAR_TOKEN` CircleCI environment variable

### `publish-contracts`

Packs and pushes `Jukebox.DataAccess.Contracts` to GitHub Packages.

- Requires `resolve-version` (and `sonar-scan` on `feature/*`)
- Uses the version resolved by `resolve-version`
- Target feed: `https://nuget.pkg.github.com/jonwong84/index.json`
- Authenticated via `GITHUB_TOKEN`

### `publish-entityframework`

Packs and pushes `Jukebox.DataAccess.EntityFramework`. Runs after `publish-contracts` to ensure the contracts package is available before the EF package (which depends on it) is published.

### `publish-dataaccess`

Packs and pushes `Jukebox.DataAccess`. Runs last as it depends on both `Contracts` and `EntityFramework`.

---

## Published Packages

All three packages are published to:

```
https://nuget.pkg.github.com/jonwong84/index.json
```

| Package | Depends on |
|---|---|
| `Jukebox.DataAccess.Contracts` | (none) |
| `Jukebox.DataAccess.EntityFramework` | `Jukebox.DataAccess.Contracts` |
| `Jukebox.DataAccess` | `Jukebox.DataAccess.Contracts`, `Jukebox.DataAccess.EntityFramework` |

The sequential publish order (`contracts` → `entityframework` → `dataaccess`) ensures each package's dependencies are already available in the feed before it is published.

---

## Environment Variables (CircleCI)

Set these in your CircleCI project settings:

| Variable | Used by | Description |
|---|---|---|
| `GITHUB_TOKEN` | All jobs | NuGet restore from GitHub Packages; package push authentication |
| `SONAR_TOKEN` | `sonar-scan` | SonarCloud authentication |

---

## Code Quality

SonarCloud analysis runs on every `feature/*` branch. Dashboard:

```
https://sonarcloud.io/project/overview?id=jonwong84_jukebox-data-access
```

Coverage is collected via `coverlet.msbuild` in OpenCover format and uploaded as part of the SonarCloud scan.

---

## Consuming a Newly Published Package

After a pipeline completes, the new package version is immediately available. In the consuming project, update the version reference:

```xml
<PackageReference Include="Jukebox.DataAccess" Version="1.0.1" />
```

Then restore:

```bash
dotnet restore
```

Ensure `GITHUB_TOKEN` is set in the consuming environment — see [LOCAL_DEV.md#consuming-the-packages](LOCAL_DEV.md#consuming-the-packages).
