using Jukebox.DataAccess.Contracts.DataContracts.Song;
using Jukebox.DataAccess.EntityFramework.Models;
using Jukebox.DataAccess.Songs;
using Microsoft.Extensions.Logging.Abstractions;
using System.Linq;

namespace Jukebox.DataAccess.Tests.UnitTests.RepositoryTests;

public class SongRepositoryAccessTests : RepositoryTestBase
{
    private readonly SongRepositoryAccess _sut;

    public SongRepositoryAccessTests()
    {
        _sut = new SongRepositoryAccess(DbContext, NullLogger<SongRepositoryAccess>.Instance);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WhenSongExists()
    {
        // Arrange
        var artist = SeedArtist("Test Artist");
        var genre = SeedGenre("Rock");
        var song = SeedSong(artist.Id, "Test Song", bpm: 120, lyrics: "La la la", genreIds: new List<int> { genre.Id });

        // Act
        var result = await _sut.GetByIdAsync(song.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.SongDetails);
        Assert.Equal(song.Id, result.SongDetails.Id);
        Assert.Equal("Test Song", result.SongDetails.Title);
        Assert.Equal("Test Artist", result.SongDetails.Artist.Name);
        Assert.Equal(120, result.SongDetails.Bpm);
        Assert.Equal("La la la", result.SongDetails.Lyrics);
        Assert.Single(result.SongDetails.Genres);
        Assert.Equal("Rock", result.SongDetails.Genres.First().Name);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenSongDoesNotExist()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.SongDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsAlbum_WhenSongHasAlbum()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, "Test Album");
        var song = SeedSong(artist.Id, albumId: album.Id);

        // Act
        var result = await _sut.GetByIdAsync(song.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.SongDetails!.Album);
        Assert.Equal("Test Album", result.SongDetails.Album.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullAlbum_WhenSongHasNoAlbum()
    {
        // Arrange
        var artist = SeedArtist();
        var song = SeedSong(artist.Id);

        // Act
        var result = await _sut.GetByIdAsync(song.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.SongDetails!.Album);
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var artist = SeedArtist();
        var genre = SeedGenre();

        var request = new AddSongRequest
        {
            Title = "New Song",
            ArtistId = artist.Id,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int> { genre.Id },
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.SongId);

        var saved = await DbContext.Songs.FindAsync(result.SongId);
        Assert.NotNull(saved);
        Assert.Equal("New Song", saved.Title);
        Assert.Equal(artist.Id, saved.ArtistId);
        Assert.Equal("user-abc", saved.CreatedBy);
    }

    [Fact]
    public async Task AddAsync_SavesLyrics_WhenLyricsProvided()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new AddSongRequest
        {
            Title = "Lyrical Song",
            ArtistId = artist.Id,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int>(),
            Lyrics = "Some lyrics here",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(result.SongId!.Value);
        Assert.Equal("Some lyrics here", getResult.SongDetails!.Lyrics);
    }

    [Fact]
    public async Task AddAsync_SavesGenres_WhenGenresProvided()
    {
        // Arrange
        var artist = SeedArtist();
        var genre1 = SeedGenre("Rock");
        var genre2 = SeedGenre("Pop");

        var request = new AddSongRequest
        {
            Title = "Multi-Genre Song",
            ArtistId = artist.Id,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int> { genre1.Id, genre2.Id },
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(result.SongId!.Value);
        Assert.Equal(2, getResult.SongDetails!.Genres.Count);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        var request = new AddSongRequest
        {
            Title = "New Song",
            ArtistId = 999,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int>(),
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.SongId);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenAlbumDoesNotExist()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new AddSongRequest
        {
            Title = "New Song",
            ArtistId = artist.Id,
            AlbumId = 999,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int>(),
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.SongId);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenGenreDoesNotExist()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new AddSongRequest
        {
            Title = "New Song",
            ArtistId = artist.Id,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int> { 999 },
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.SongId);
        Assert.NotNull(result.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var artist = SeedArtist();
        var song = SeedSong(artist.Id, "Original Title");

        var request = new UpdateSongRequest
        {
            Id = song.Id,
            Title = "Updated Title",
            ArtistId = artist.Id,
            Duration = TimeSpan.FromSeconds(250),
            GenreIds = new List<int>(),
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.SongDetails);
        Assert.Equal("Updated Title", result.SongDetails.Title);
        var saved = await DbContext.Songs.FindAsync(result.SongDetails!.Id);
        Assert.Equal("user-abc", saved!.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGenres_WhenGenresChange()
    {
        // Arrange
        var artist = SeedArtist();
        var genre1 = SeedGenre("Rock");
        var genre2 = SeedGenre("Pop");
        var song = SeedSong(artist.Id, genreIds: new List<int> { genre1.Id });

        var request = new UpdateSongRequest
        {
            Id = song.Id,
            Title = song.Title,
            ArtistId = artist.Id,
            Duration = song.Duration,
            GenreIds = new List<int> { genre2.Id },
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.SongDetails!.Genres);
        Assert.Equal("Pop", result.SongDetails.Genres.First().Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenSongDoesNotExist()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new UpdateSongRequest
        {
            Id = 999,
            Title = "Updated Title",
            ArtistId = artist.Id,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int>(),
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.SongDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        var artist = SeedArtist();
        var song = SeedSong(artist.Id);

        var request = new UpdateSongRequest
        {
            Id = song.Id,
            Title = "Updated Title",
            ArtistId = 999,
            Duration = TimeSpan.FromSeconds(200),
            GenreIds = new List<int>(),
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_WhenSongExists()
    {
        // Arrange
        var artist = SeedArtist();
        var song = SeedSong(artist.Id);

        // Act
        var result = await _sut.DeleteAsync(song.Id);

        // Assert
        Assert.True(result.Success);
        var deleted = await DbContext.Songs.FindAsync(song.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_CascadesLyrics_WhenSongHasLyrics()
    {
        // Arrange
        var artist = SeedArtist();
        var song = SeedSong(artist.Id, lyrics: "Some lyrics");

        // Act
        var result = await _sut.DeleteAsync(song.Id);

        // Assert
        Assert.True(result.Success);
        var lyrics = DbContext.Set<SongLyrics>().FirstOrDefault(l => l.SongId == song.Id);
        Assert.Null(lyrics);
    }

    [Fact]
    public async Task DeleteAsync_CascadesSongGenres_WhenSongHasGenres()
    {
        // Arrange
        var artist = SeedArtist();
        var genre = SeedGenre();
        var song = SeedSong(artist.Id, genreIds: new List<int> { genre.Id });

        // Act
        var result = await _sut.DeleteAsync(song.Id);

        // Assert
        Assert.True(result.Success);
        var songGenres = DbContext.Set<SongGenre>().Where(sg => sg.SongId == song.Id).ToList();
        Assert.Empty(songGenres);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenSongDoesNotExist()
    {
        // Act
        var result = await _sut.DeleteAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // ListAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListAsync_ReturnsAllSongs_WhenNoFiltersApplied()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id, "Song A");
        SeedSong(artist.Id, "Song B");
        SeedSong(artist.Id, "Song C");

        var request = new ListSongsRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Songs.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByArtistId()
    {
        // Arrange
        var artist1 = SeedArtist("Artist 1");
        var artist2 = SeedArtist("Artist 2");
        SeedSong(artist1.Id, "Song A");
        SeedSong(artist1.Id, "Song B");
        SeedSong(artist2.Id, "Song C");

        var request = new ListSongsRequest { ArtistId = artist1.Id, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Songs, s => Assert.Equal("Artist 1", s.Artist));
    }

    [Fact]
    public async Task ListAsync_FiltersByAlbumId()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, "Test Album");
        SeedSong(artist.Id, "Song A", albumId: album.Id);
        SeedSong(artist.Id, "Song B", albumId: album.Id);
        SeedSong(artist.Id, "Song C");

        var request = new ListSongsRequest { AlbumId = album.Id, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FiltersByGenreId()
    {
        // Arrange
        var artist = SeedArtist();
        var genre = SeedGenre("Rock");
        SeedSong(artist.Id, "Song A", genreIds: new List<int> { genre.Id });
        SeedSong(artist.Id, "Song B", genreIds: new List<int> { genre.Id });
        SeedSong(artist.Id, "Song C");

        var request = new ListSongsRequest { GenreId = genre.Id, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FiltersByBpmRange()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id, "Slow Song", bpm: 80);
        SeedSong(artist.Id, "Mid Song", bpm: 120);
        SeedSong(artist.Id, "Fast Song", bpm: 160);

        var request = new ListSongsRequest { MinBpm = 100, MaxBpm = 140, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("Mid Song", result.Songs.ElementAt(0).Title);
    }

    [Fact]
    public async Task ListAsync_FiltersByTitleSearch()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id, "Bohemian Rhapsody");
        SeedSong(artist.Id, "Another One Bites the Dust");
        SeedSong(artist.Id, "Bohemian Like You");

        var request = new ListSongsRequest { TitleSearch = "Bohemian", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_ReturnsSongsOrderedByTitle()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id, "Zebra Song");
        SeedSong(artist.Id, "Apple Song");
        SeedSong(artist.Id, "Mango Song");

        var request = new ListSongsRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Apple Song", result.Songs.ElementAt(0).Title);
        Assert.Equal("Mango Song", result.Songs.ElementAt(1).Title);
        Assert.Equal("Zebra Song", result.Songs.ElementAt(2).Title);
    }

    [Fact]
    public async Task ListAsync_ReturnsCorrectPage()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id, "Song A");
        SeedSong(artist.Id, "Song B");
        SeedSong(artist.Id, "Song C");
        SeedSong(artist.Id, "Song D");
        SeedSong(artist.Id, "Song E");

        var request = new ListSongsRequest { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Songs.Count);
        Assert.Equal("Song C", result.Songs.ElementAt(0).Title);
        Assert.Equal("Song D", result.Songs.ElementAt(1).Title);
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyList_WhenNoSongsMatch()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id, "Test Song");

        var request = new ListSongsRequest { TitleSearch = "Nonexistent", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Songs);
    }
}
