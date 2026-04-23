using System.Data;
using JukeboxDataAccess.DataModels;
using JukeboxDataAccess.Interfaces;
using JukeboxDataAccess.Mappers;
using JukeboxDataAccess.Models;
using JukeboxDataAccess.Repositories;
using JukeboxDataAccess.Requests;
using Microsoft.Data.Sqlite;
using Moq;

namespace JukeboxDataAccess.Tests.Repositories;

/// <summary>
/// Tests for <see cref="SongRepository"/> using an in-memory SQLite database to exercise real SQL queries.
/// </summary>
public class SongRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SongMapper _mapper = new();

    public SongRepositoryTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        CreateSchema();
        SeedData();
    }

    public void Dispose() => _connection.Dispose();

    private void CreateSchema()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE Songs (
                Id              INTEGER PRIMARY KEY,
                Title           TEXT    NOT NULL,
                Artist          TEXT    NOT NULL,
                Album           TEXT    NOT NULL DEFAULT '',
                Genre           TEXT    NOT NULL DEFAULT '',
                DurationSeconds INTEGER NOT NULL DEFAULT 0,
                ReleaseYear     INTEGER NOT NULL DEFAULT 0
            )
            """;
        cmd.ExecuteNonQuery();
    }

    private void SeedData()
    {
        using var cmd = _connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Songs (Id, Title, Artist, Album, Genre, DurationSeconds, ReleaseYear) VALUES
                (1, 'Imagine',            'John Lennon', 'Imagine',              'Rock',  187, 1971),
                (2, 'Bohemian Rhapsody',  'Queen',       'A Night at the Opera', 'Rock',  354, 1975),
                (3, 'Hotel California',   'Eagles',      'Hotel California',     'Rock',  391, 1977),
                (4, 'Billie Jean',        'Michael Jackson', 'Thriller',         'Pop',   294, 1982),
                (5, 'Like a Rolling Stone','Bob Dylan',  'Highway 61 Revisited', 'Folk',  369, 1965)
            """;
        cmd.ExecuteNonQuery();
    }

    // ── Constructor guard-clause tests ──────────────────────────────────────

    [Fact]
    public void Constructor_NullConnection_ShouldThrowArgumentNullException()
    {
        var mapperMock = new Mock<IMapper<SongDataModel, Song>>();
        Assert.Throws<ArgumentNullException>(() =>
            new SongRepository(null!, mapperMock.Object));
    }

    [Fact]
    public void Constructor_NullMapper_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new SongRepository(_connection, null!));
    }

    // ── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_NullRequest_ShouldThrowArgumentNullException()
    {
        var repo = new SongRepository(_connection, _mapper);
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetByIdAsync(null!));
    }

    [Fact]
    public async Task GetByIdAsync_WhenSongExists_ReturnsMappedSong()
    {
        var repo = new SongRepository(_connection, _mapper);

        var result = await repo.GetByIdAsync(new GetSongRequest { Id = 1 });

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Imagine", result.Title);
        Assert.Equal("John Lennon", result.Artist);
        Assert.Equal("Imagine", result.Album);
        Assert.Equal("Rock", result.Genre);
        Assert.Equal(187, result.DurationSeconds);
        Assert.Equal(1971, result.ReleaseYear);
    }

    [Fact]
    public async Task GetByIdAsync_WhenSongDoesNotExist_ReturnsNull()
    {
        var repo = new SongRepository(_connection, _mapper);

        var result = await repo.GetByIdAsync(new GetSongRequest { Id = 999 });

        Assert.Null(result);
    }

    // ── SearchAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_NullRequest_ShouldThrowArgumentNullException()
    {
        var repo = new SongRepository(_connection, _mapper);
        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.SearchAsync(null!));
    }

    [Fact]
    public async Task SearchAsync_WithNoFilters_ReturnsAllSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest())).ToList();

        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task SearchAsync_FilterByTitle_ReturnsMatchingSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest { Title = "imagine" })).ToList();

        Assert.Single(results);
        Assert.Equal("Imagine", results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_FilterByArtist_ReturnsMatchingSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest { Artist = "Queen" })).ToList();

        Assert.Single(results);
        Assert.Equal("Bohemian Rhapsody", results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_FilterByGenre_ReturnsMatchingSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest { Genre = "Rock" })).ToList();

        Assert.Equal(3, results.Count);
        Assert.All(results, s => Assert.Equal("Rock", s.Genre));
    }

    [Fact]
    public async Task SearchAsync_FilterByAlbum_ReturnsMatchingSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest { Album = "Thriller" })).ToList();

        Assert.Single(results);
        Assert.Equal("Billie Jean", results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_FilterByReleaseYear_ReturnsMatchingSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest { ReleaseYear = 1975 })).ToList();

        Assert.Single(results);
        Assert.Equal("Bohemian Rhapsody", results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_FilterByMultipleCriteria_ReturnsMatchingSongs()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest
        {
            Artist = "Eagles",
            Genre = "Rock"
        })).ToList();

        Assert.Single(results);
        Assert.Equal("Hotel California", results[0].Title);
    }

    [Fact]
    public async Task SearchAsync_NoMatchingResults_ReturnsEmptyCollection()
    {
        var repo = new SongRepository(_connection, _mapper);

        var results = (await repo.SearchAsync(new SearchSongsRequest { Title = "NonExistentSong" })).ToList();

        Assert.Empty(results);
    }
}
