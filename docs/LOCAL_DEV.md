# Local Development

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 8.0 | |
| Docker Desktop | Any recent | For SQL Server |
| EF Core CLI tools | 8.0 | `dotnet tool install -g dotnet-ef` |

---

## 1. Start SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass3word3!" \
  -p 1433:1433 --name jukebox-sql -d \
  mcr.microsoft.com/mssql/server:2022-latest
```

Verify it's running:

```bash
docker ps --filter name=jukebox-sql
```

---

## 2. Set the Connection String

The migrations project and `JukeboxDbContextFactory` read the connection string from the `JUKEBOX_DB_CONNECTION_STRING` environment variable.

**Windows (PowerShell — current session):**
```powershell
$env:JUKEBOX_DB_CONNECTION_STRING = "Server=localhost,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;"
```

**Windows (Machine-level, persists across sessions — requires admin PowerShell):**
```powershell
[System.Environment]::SetEnvironmentVariable(
  "JUKEBOX_DB_CONNECTION_STRING",
  "Server=localhost,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;",
  "Machine"
)
```

**macOS / Linux:**
```bash
export JUKEBOX_DB_CONNECTION_STRING="Server=localhost,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;"
```

---

## 3. Apply Migrations

```bash
dotnet ef database update \
  --project src/Jukebox.DataAccess.Migrations \
  --startup-project src/Jukebox.DataAccess.Migrations
```

This applies all pending migrations to the `Jukebox` database (creating it if it doesn't exist). Currently there is one migration: `InitialCreate`.

### Adding a new migration

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Jukebox.DataAccess.Migrations \
  --startup-project src/Jukebox.DataAccess.Migrations \
  --output-dir Migrations
```

### Reverting a migration

```bash
dotnet ef database update <PreviousMigrationName> \
  --project src/Jukebox.DataAccess.Migrations \
  --startup-project src/Jukebox.DataAccess.Migrations
```

---

## 4. Running the Tests

Tests are fully self-contained and do not require a running database or connection string.

```bash
# All tests
dotnet test

# With detailed output
dotnet test --logger "console;verbosity=detailed"

# With coverage (OpenCover format, as used in CI)
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

Expected: **76 tests, all passing**.

---

## 5. Consuming the Packages

To consume `Jukebox.DataAccess` packages from another .NET project, you need:

1. A GitHub Personal Access Token (PAT) with `read:packages` scope
2. A `nuget.config` that points to the GitHub Packages feed

### Set the token

```bash
export GITHUB_TOKEN=ghp_your_token_here   # macOS / Linux
$env:GITHUB_TOKEN = "ghp_your_token_here"  # Windows PowerShell
```

### Add a `nuget.config`

Place this at the root of the consuming solution:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="github-jonwong84" value="https://nuget.pkg.github.com/jonwong84/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github-jonwong84>
      <add key="Username" value="jonwong84" />
      <add key="CleartextPassword" value="%GITHUB_TOKEN%" />
    </github-jonwong84>
  </packageSourceCredentials>
</configuration>
```

### Add package references

```xml
<!-- Jukebox.DataManager.csproj or similar -->
<ItemGroup>
  <PackageReference Include="Jukebox.DataAccess" Version="1.0.0" />
  <PackageReference Include="Jukebox.DataAccess.Contracts" Version="1.0.0" />
</ItemGroup>
```

Only reference `Jukebox.DataAccess.EntityFramework` if you need direct access to `JukeboxDbContext` or the entity models. Most consumers only need `Jukebox.DataAccess` and `Jukebox.DataAccess.Contracts`.

### Register in DI

In `Program.cs` of the consuming service:

```csharp
builder.Services.AddDataAccess();
```

This registers `JukeboxDbContext` and all three repository implementations (`ISongRepositoryAccess`, `IArtistRepositoryAccess`, `IAlbumRepositoryAccess`) with the DI container. The connection string is read from `JUKEBOX_DB_CONNECTION_STRING` at runtime.

---

## 6. Common Issues

**`dotnet ef` command not found**
Install the global tool: `dotnet tool install -g dotnet-ef`

**Migration fails with login error**
Check that `JUKEBOX_DB_CONNECTION_STRING` is set in the current shell session and that the SQL Server Docker container is running.

**Package restore fails in consuming project**
Ensure `GITHUB_TOKEN` is set and that the PAT has `read:packages` scope. Confirm `nuget.config` is at the solution root (not the project root).

**`TrustServerCertificate=True` warning**
This is expected for local SQL Server Docker instances which use self-signed certificates. Do not use this flag in production — provision a proper certificate instead.
