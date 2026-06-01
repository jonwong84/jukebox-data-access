# API Reference

Public interfaces and contract types exposed by the `Jukebox.DataAccess.Contracts` and `Jukebox.DataAccess` packages.

---

## Registration

All repositories are registered via a single call in the consuming service's `Program.cs`:

```csharp
builder.Services.AddDataAccess();
```

This makes `ISongRepositoryAccess`, `IArtistRepositoryAccess`, and `IAlbumRepositoryAccess` available for injection. The connection string is read from `JUKEBOX_DB_CONNECTION_STRING`.

---

## `IArtistRepositoryAccess`

Namespace: `Jukebox.DataAccess.Contracts`

```csharp
Task<ArtistResult?> GetByIdAsync(int id, CancellationToken ct = default);
Task<PagedResult<ArtistResult>> ListAsync(PagedRequest request, CancellationToken ct = default);
Task<ArtistResult> CreateAsync(CreateArtistRequest request, CancellationToken ct = default);
Task<ArtistResult?> UpdateAsync(int id, UpdateArtistRequest request, CancellationToken ct = default);
Task<bool> DeleteAsync(int id, CancellationToken ct = default);
```

### `CreateArtistRequest`

| Property | Type | Required | Description |
|---|---|---|---|
| `Name` | `string` | Yes | Artist name |
| `Bio` | `string?` | No | Artist biography |
| `CreatedBy` | `string?` | No | Identity of the caller (used for audit trail) |

### `UpdateArtistRequest`

| Property | Type | Required | Description |
|---|---|---|---|
| `Name` | `string?` | No | Updated name (omit to leave unchanged) |
| `Bio` | `string?` | No | Updated biography |
| `UpdatedBy` | `string?` | No | Identity of the caller |

### `ArtistResult`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | |
| `Name` | `string` | |
| `Bio` | `string?` | |
| `CreatedAt` | `DateTime` | UTC |
| `CreatedBy` | `string?` | |
| `UpdatedBy` | `string?` | |

---

## `IAlbumRepositoryAccess`

Namespace: `Jukebox.DataAccess.Contracts`

```csharp
Task<AlbumResult?> GetByIdAsync(int id, CancellationToken ct = default);
Task<PagedResult<AlbumResult>> ListAsync(PagedRequest request, CancellationToken ct = default);
Task<AlbumResult> CreateAsync(CreateAlbumRequest request, CancellationToken ct = default);
Task<AlbumResult?> UpdateAsync(int id, UpdateAlbumRequest request, CancellationToken ct = default);
Task<bool> DeleteAsync(int id, CancellationToken ct = default);
```

### `CreateAlbumRequest`

| Property | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | Yes | Album title |
| `ReleaseDate` | `DateTime?` | No | Release date (UTC) |
| `IsCompilation` | `bool` | No | Default: `false` |
| `Description` | `string?` | No | Album description text |
| `ArtistIds` | `IEnumerable<int>` | No | Associated artist IDs |
| `CreatedBy` | `string?` | No | Identity of the caller |

### `UpdateAlbumRequest`

| Property | Type | Required | Description |
|---|---|---|---|
| `Title` | `string?` | No | Updated title |
| `ReleaseDate` | `DateTime?` | No | Updated release date |
| `IsCompilation` | `bool?` | No | Updated compilation flag |
| `Description` | `string?` | No | Updated description |
| `ArtistIds` | `IEnumerable<int>?` | No | Replaces existing artist associations |
| `UpdatedBy` | `string?` | No | Identity of the caller |

### `AlbumResult`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | |
| `Title` | `string` | |
| `ReleaseDate` | `DateTime?` | |
| `IsCompilation` | `bool` | |
| `Description` | `string?` | |
| `CreatedAt` | `DateTime` | UTC |
| `CreatedBy` | `string?` | |
| `UpdatedBy` | `string?` | |
| `Artists` | `IReadOnlyList<ArtistRefResult>` | Lightweight artist references |

---

## `ISongRepositoryAccess`

Namespace: `Jukebox.DataAccess.Contracts`

```csharp
Task<SongResult?> GetByIdAsync(int id, CancellationToken ct = default);
Task<PagedResult<SongResult>> ListAsync(PagedRequest request, CancellationToken ct = default);
Task<SongResult> CreateAsync(CreateSongRequest request, CancellationToken ct = default);
Task<SongResult?> UpdateAsync(int id, UpdateSongRequest request, CancellationToken ct = default);
Task<bool> DeleteAsync(int id, CancellationToken ct = default);
```

### `CreateSongRequest`

| Property | Type | Required | Description |
|---|---|---|---|
| `Title` | `string` | Yes | Song title |
| `ArtistId` | `int` | Yes | ID of the performing artist |
| `AlbumId` | `int?` | No | ID of the album |
| `Duration` | `TimeSpan` | Yes | Track duration |
| `TrackNumber` | `int?` | No | Position on the album |
| `Bpm` | `int?` | No | Beats per minute |
| `GenreIds` | `IEnumerable<int>` | No | Associated genre IDs |
| `Lyrics` | `string?` | No | Lyrics text — creates a `SongLyrics` record |
| `CreatedBy` | `string?` | No | Identity of the caller |

### `UpdateSongRequest`

| Property | Type | Required | Description |
|---|---|---|---|
| `Title` | `string?` | No | Updated title |
| `AlbumId` | `int?` | No | Updated album ID |
| `Duration` | `TimeSpan?` | No | Updated duration |
| `TrackNumber` | `int?` | No | Updated track number |
| `Bpm` | `int?` | No | Updated BPM |
| `GenreIds` | `IEnumerable<int>?` | No | Replaces existing genre associations |
| `Lyrics` | `string?` | No | Updated lyrics |
| `UpdatedBy` | `string?` | No | Identity of the caller |

### `SongResult`

| Property | Type | Description |
|---|---|---|
| `Id` | `int` | |
| `Title` | `string` | |
| `Duration` | `TimeSpan` | |
| `TrackNumber` | `int?` | |
| `Bpm` | `int?` | |
| `Lyrics` | `string?` | |
| `CreatedAt` | `DateTime` | UTC |
| `CreatedBy` | `string?` | |
| `UpdatedBy` | `string?` | |
| `Artist` | `ArtistRefResult` | |
| `Album` | `AlbumRefResult?` | `null` if song has no album |
| `Genres` | `IReadOnlyList<GenreRefResult>` | |

---

## Shared Contract Types

### `PagedRequest`

Used by all three `ListAsync` methods.

| Property | Type | Default | Description |
|---|---|---|---|
| `Page` | `int` | `1` | Page number (1-based) |
| `PageSize` | `int` | `20` | Items per page |

### `PagedResult<T>`

Returned by all three `ListAsync` methods.

| Property | Type | Description |
|---|---|---|
| `Items` | `IReadOnlyList<T>` | Page of results |
| `TotalCount` | `int` | Total records matching the query |
| `Page` | `int` | Current page |
| `PageSize` | `int` | Items per page |

### Reference Types

Lightweight types used inside result objects to avoid circular references.

**`ArtistRefResult`**

| Property | Type |
|---|---|
| `Id` | `int` |
| `Name` | `string` |

**`AlbumRefResult`**

| Property | Type |
|---|---|
| `Id` | `int` |
| `Title` | `string` |

**`GenreRefResult`**

| Property | Type |
|---|---|
| `Id` | `int` |
| `Name` | `string` |

---

## Return Value Conventions

| Return type | Meaning |
|---|---|
| `T` | Operation succeeded; result is never null |
| `T?` | Returns `null` if the record was not found |
| `bool` (Delete) | Returns `true` if deleted, `false` if record was not found |

Callers are responsible for checking for `null` on `GetByIdAsync` and `UpdateAsync`, and for handling `false` on `DeleteAsync`.

Referential integrity violations (e.g. deleting an artist that has songs) will surface as `DbUpdateException` from EF Core. Callers should catch and handle this appropriately.
