# Jukebox Data Access

Data access layer for the Jukebox platform. Provides EF Core repository implementations, contract interfaces, and database migrations for the `Jukebox` SQL Server database.

Consumed as versioned NuGet packages by upstream services (e.g. `jukebox-data-manager`).

---

## Repository Structure

```
jukebox-data-access/
  src/
    Jukebox.DataAccess/                    # Repository implementations (public-facing)
    Jukebox.DataAccess.Contracts/          # Interfaces and contract types
    Jukebox.DataAccess.EntityFramework/    # JukeboxDbContext and EF Core entity models
    Jukebox.DataAccess.Migrations/         # EF Core migrations and DbContextFactory
  tests/
    Jukebox.DataAccess.Tests/              # Test project (76 tests)
  docs/                                    # Extended documentation (you are here)
  jukebox-data-access.sln
```

---

## Quick Start

**Prerequisites:** .NET 8.0 SDK, Docker (SQL Server).

```bash
# Start SQL Server
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Pass3word3!" \
  -p 1433:1433 --name jukebox-sql -d \
  mcr.microsoft.com/mssql/server:2022-latest

# Set connection string
export JUKEBOX_DB_CONNECTION_STRING="Server=localhost,1433;Database=Jukebox;User Id=sa;Password=Pass3word3!;TrustServerCertificate=True;"

# Apply migrations
dotnet ef database update \
  --project src/Jukebox.DataAccess.Migrations \
  --startup-project src/Jukebox.DataAccess.Migrations

# Run tests
dotnet test
```

See [docs/LOCAL_DEV.md](docs/LOCAL_DEV.md) for full setup including package consumption from another project.

---

## NuGet Packages

Three packages are published to [GitHub Packages](https://github.com/jonwong84/jukebox-data-access/packages):

| Package | Description |
|---|---|
| `Jukebox.DataAccess` | Repository implementations — reference this to query/mutate data |
| `Jukebox.DataAccess.Contracts` | Interfaces and contract types — reference this for DI and testing |
| `Jukebox.DataAccess.EntityFramework` | `JukeboxDbContext` and entity models — reference only if you need direct EF access |

To consume from another project, see [docs/LOCAL_DEV.md#consuming-the-packages](docs/LOCAL_DEV.md#consuming-the-packages).

---

## Documentation

| Document | Description |
|---|---|
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | Project structure, EF Core models, relationships, design decisions |
| [docs/LOCAL_DEV.md](docs/LOCAL_DEV.md) | Local setup, migrations, running tests, consuming the packages |
| [docs/API_REFERENCE.md](docs/API_REFERENCE.md) | Public interfaces and contract types for all three repositories |
| [docs/CICD.md](docs/CICD.md) | CircleCI pipeline, versioning, NuGet publishing |

---

## Technology Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 8.0 |
| ORM | EF Core 8 |
| Database | SQL Server |
| Test framework | xUnit + coverlet |
| CI/CD | CircleCI |
| Package registry | GitHub Packages (`https://nuget.pkg.github.com/jonwong84/index.json`) |
| Code quality | SonarCloud |

---

## Test Coverage

| Project | Tests |
|---|---|
| `Jukebox.DataAccess.Tests` | 100 |

```bash
dotnet test
```
