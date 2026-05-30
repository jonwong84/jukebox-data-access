using Jukebox.DataAccess.EntityFramework;
using Jukebox.DataAccess.EntityFramework.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Jukebox.DataAccess.Tests;

public abstract class RepositoryTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly JukeboxDbContext DbContext;

    protected RepositoryTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        // Enable foreign key enforcement in SQLite
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = "PRAGMA foreign_keys = ON;";
        cmd.ExecuteNonQuery();

        var options = new DbContextOptionsBuilder<JukeboxDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new JukeboxDbContext(options);
        DbContext.Database.EnsureCreated();
    }

    // -------------------------------------------------------------------------
    // Seed helpers
    // -------------------------------------------------------------------------

    protected Artist SeedArtist(string name = "Test Artist", string? bio = null, string? createdBy = null)
    {
        var artist = new Artist
        {
            Name = name,
            Bio = bio,
            CreatedBy = createdBy
        };
        DbContext.Artists.Add(artist);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();
        return artist;
    }

    protected Album SeedAlbum(
        List<int> artistIds,
        string title = "Test Album",
        DateTime? releaseDate = null,
        bool isCompilation = false,
        string? description = null,
        string? createdBy = null)
    {
        var album = new Album
        {
            Title = title,
            ReleaseDate = releaseDate,
            IsCompilation = isCompilation,
            CreatedBy = createdBy,
            AlbumArtists = artistIds.Select(id => new AlbumArtist { ArtistId = id }).ToList()
        };

        if (description != null)
        {
            album.Description = new AlbumDescription { Description = description };
        }

        DbContext.Albums.Add(album);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();
        return album;
    }

    protected Genre SeedGenre(string name = "Rock", string? parentGenreId = null, string? createdBy = null)
    {
        var genre = new Genre
        {
            Name = name,
            CreatedBy = createdBy
        };
        DbContext.Genres.Add(genre);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();
        return genre;
    }

    protected Song SeedSong(
        int artistId,
        string title = "Test Song",
        int? albumId = null,
        TimeSpan? duration = null,
        int? trackNumber = null,
        int? bpm = null,
        string? lyrics = null,
        List<int>? genreIds = null,
        string? createdBy = null)
    {
        var song = new Song
        {
            Title = title,
            ArtistId = artistId,
            AlbumId = albumId,
            Duration = duration ?? TimeSpan.FromSeconds(180),
            TrackNumber = trackNumber,
            Bpm = bpm,
            CreatedBy = createdBy,
            SongGenres = genreIds?.Select(id => new SongGenre { GenreId = id }).ToList()
                         ?? new List<SongGenre>()
        };

        if (lyrics != null)
        {
            song.Lyrics = new SongLyrics { Lyrics = lyrics };
        }

        DbContext.Songs.Add(song);
        DbContext.SaveChanges();
        DbContext.ChangeTracker.Clear();
        return song;
    }

    // -------------------------------------------------------------------------
    // Dispose
    // -------------------------------------------------------------------------

    public void Dispose()
    {
        DbContext.Dispose();
        _connection.Dispose();
    }
}
