# Architecture

## Overview

`jukebox-data-access` is a pure data access layer. It has no HTTP endpoints, no gRPC services, and no business logic. Its sole responsibilities are:

- Defining the EF Core entity models and `JukeboxDbContext`
- Implementing repository interfaces for `Song`, `Artist`, and `Album`
- Publishing those implementations as versioned NuGet packages for consumption by upstream services

```
Upstream service (e.g. jukebox-data-manager)
        │
        │  NuGet package references
        ▼
┌──────────────────────────────────────────────┐
│              Jukebox.DataAccess              │  Repository implementations
│                                              │  AddDataAccess() extension
└─────────────────────┬────────────────────────┘
                      │
        ┌─────────────┼──────────────┐
        ▼             ▼              ▼
┌─────────────┐ ┌──────────┐ ┌──────────────────────────┐
│  Contracts  │ │    EF    │ │        Migrations         │
│ (interfaces │ │  (models │ │ (InitialCreate, DbContext  │
│  + types)   │ │  + ctx)  │ │      Factory)             │
└─────────────┘ └──────────┘ └──────────────────────────┘
                      │
                      ▼
              SQL Server (Jukebox)
```

---

## Project Breakdown

### `Jukebox.DataAccess.Contracts`

Contains everything a consumer needs to program against — no EF Core dependency:

- `ISongRepositoryAccess`, `IArtistRepositoryAccess`, `IAlbumRepositoryAccess` — repository interfaces
- All request types (`CreateSongRequest`, `UpdateArtistRequest`, etc.)
- All result types (`SongResult`, `ArtistResult`, `AlbumResult`, `PagedResult<T>`, etc.)

Consumers that only need to mock or test against the interfaces should reference only this package.

### `Jukebox.DataAccess.EntityFramework`

Contains:

- `JukeboxDbContext` — the EF Core `DbContext`
- All entity model classes (`Song`, `Artist`, `Album`, `Genre`, `SongGenre`, `SongLyrics`, `AlbumArtist`, `AlbumDescription`)
- `OnModelCreating` configuration (relationships, delete behaviors, field constraints)

### `Jukebox.DataAccess.Migrations`

Contains:

- EF Core migrations (`InitialCreate` and any subsequent migrations)
- `JukeboxDbContextFactory` — implements `IDesignTimeDbContextFactory<JukeboxDbContext>` for `dotnet ef` tooling; reads connection string from `JUKEBOX_DB_CONNECTION_STRING` environment variable

This project is not published as a NuGet package. It exists solely to manage schema evolution.

### `Jukebox.DataAccess`

Contains the concrete repository implementations:

- `SongRepositoryAccess : ISongRepositoryAccess`
- `ArtistRepositoryAccess : IArtistRepositoryAccess`
- `AlbumRepositoryAccess : IAlbumRepositoryAccess`

Also contains the `AddDataAccess()` `IServiceCollection` extension method, which registers all three repositories and `JukeboxDbContext` with the DI container. Upstream services call this once in `Program.cs`.

---

## Entity Models

### Relationships

```
Artist ──────────< Song >────── Album
  │                │
  └──< AlbumArtist >┘         Song >── SongLyrics (1:1)
                               Song >──< SongGenre >── Genre
                              Album >── AlbumDescription (1:1)
                              Genre >── Genre (self-referential parent/sub-genres)
```

### Delete Behaviors

| Relationship | Behavior | Rationale |
|---|---|---|
| `Artist → Songs` | `Restrict` | Prevents accidental data loss; songs must be reassigned or deleted first |
| `Album → Songs` | `SetNull` | Removing an album orphans songs rather than deleting them |
| `Album → AlbumDescription` | `Cascade` | Description has no meaning without its album |
| `Song → SongLyrics` | `Cascade` | Lyrics have no meaning without their song |
| `SongGenre → Song` | `Cascade` | Join table rows are owned by the song |
| `Genre → ParentGenre` | `Restrict` | Prevents deleting a genre that has sub-genres |
| `AlbumArtist → Album` | `Cascade` | Join table rows are owned by the album |
| `AlbumArtist → Artist` | `Restrict` | Cannot remove an artist while they have album credits |

### Field Constraints

- All `CreatedBy` / `UpdatedBy` fields: `HasMaxLength(200)`
- `AlbumArtist` primary key: composite `{AlbumId, ArtistId}`

### Full Model Definitions

**`Song`**
```csharp
public int Id { get; set; }
public required string Title { get; set; }
public int ArtistId { get; set; }
public Artist Artist { get; set; } = null!;
public int? AlbumId { get; set; }
public Album? Album { get; set; }
public TimeSpan Duration { get; set; }
public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
public int? TrackNumber { get; set; }
public int? Bpm { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public SongLyrics? Lyrics { get; set; }
public string? CreatedBy { get; set; }
public string? UpdatedBy { get; set; }
```

**`Artist`**
```csharp
public int Id { get; set; }
public required string Name { get; set; }
public string? Bio { get; set; }
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public ICollection<Song> Songs { get; set; } = new List<Song>();
public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
public string? CreatedBy { get; set; }
public string? UpdatedBy { get; set; }
```

**`Album`**
```csharp
public int Id { get; set; }
public required string Title { get; set; }
public DateTime? ReleaseDate { get; set; }
public ICollection<Song> Songs { get; set; } = new List<Song>();
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public AlbumDescription? Description { get; set; }
public ICollection<AlbumArtist> AlbumArtists { get; set; } = new List<AlbumArtist>();
public bool IsCompilation { get; set; } = false;
public string? CreatedBy { get; set; }
public string? UpdatedBy { get; set; }
```

**`Genre`**
```csharp
public int Id { get; set; }
public required string Name { get; set; }
public string? Description { get; set; }
public ICollection<SongGenre> SongGenres { get; set; } = new List<SongGenre>();
public int? ParentGenreId { get; set; }
public Genre? ParentGenre { get; set; }
public ICollection<Genre> SubGenres { get; set; } = new List<Genre>();
public string? CreatedBy { get; set; }
public string? UpdatedBy { get; set; }
```

---

## Key Design Decisions

1. **Contracts package is dependency-free.** `Jukebox.DataAccess.Contracts` has no EF Core dependency. A consuming service that only needs to write against the interfaces (or mock them in tests) takes the smallest possible dependency.

2. **Three packages, one registration call.** Consumers reference `Jukebox.DataAccess` and call `AddDataAccess()` — that's it. The package handles DI registration of all repositories and `JukeboxDbContext` internally.

3. **Migrations project is not published.** Schema migration is an operational concern, not a library concern. The migrations project is run directly against the target database (by CI/CD or a developer) and is never shipped as a NuGet package.

4. **Connection string via environment variable.** `JUKEBOX_DB_CONNECTION_STRING` is the sole mechanism for database configuration. No connection strings in committed files.

5. **`CreatedBy` / `UpdatedBy` as strings, not foreign keys.** Audit trail fields accept any string identifier — a JWT `sub` claim, an API key name, a system identity. This keeps the data access layer decoupled from any specific auth scheme used by the consuming service.

6. **`Artist → Songs` uses `Restrict`, not `Cascade`.** Deleting an artist should never silently delete their songs. The consumer must explicitly handle songs before removing an artist.

7. **`Album → Songs` uses `SetNull`.** Albums are optional metadata on a song. Removing an album doesn't invalidate the song — it just becomes unaffiliated.
