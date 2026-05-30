using Jukebox.DataAccess.Albums;
using Jukebox.DataAccess.Contracts.DataContracts.Album;
using Microsoft.Extensions.Logging.Abstractions;
using Jukebox.DataAccess.EntityFramework.Models;

namespace Jukebox.DataAccess.Tests;

public class AlbumRepositoryAccessTests : RepositoryTestBase
{
    private readonly AlbumRepositoryAccess _sut;

    public AlbumRepositoryAccessTests()
    {
        _sut = new AlbumRepositoryAccess(DbContext, NullLogger<AlbumRepositoryAccess>.Instance);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WhenAlbumExists()
    {
        // Arrange
        var artist = SeedArtist("Test Artist");
        var album = SeedAlbum(new List<int> { artist.Id }, "Test Album", description: "A great album.");

        // Act
        var result = await _sut.GetByIdAsync(album.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AlbumDetails);
        Assert.Equal(album.Id, result.AlbumDetails.Id);
        Assert.Equal("Test Album", result.AlbumDetails.Title);
        Assert.Equal("A great album.", result.AlbumDetails.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenAlbumDoesNotExist()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.AlbumDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsArtists_WhenAlbumHasArtists()
    {
        // Arrange
        var artist1 = SeedArtist("Artist One");
        var artist2 = SeedArtist("Artist Two");
        var album = SeedAlbum(new List<int> { artist1.Id, artist2.Id }, "Collaboration Album");

        // Act
        var result = await _sut.GetByIdAsync(album.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.AlbumDetails!.Artists.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullDescription_WhenAlbumHasNoDescription()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, description: null);

        // Act
        var result = await _sut.GetByIdAsync(album.Id);

        // Assert
        Assert.True(result.Success);
        Assert.True(string.IsNullOrEmpty(result.AlbumDetails!.Description));
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectReleaseDate_WhenSet()
    {
        // Arrange
        var artist = SeedArtist();
        var releaseDate = new DateTime(1991, 9, 24);
        var album = SeedAlbum(new List<int> { artist.Id }, releaseDate: releaseDate);

        // Act
        var result = await _sut.GetByIdAsync(album.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(releaseDate, result.AlbumDetails!.ReleaseDate);
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new AddAlbumRequest
        {
            Title = "New Album",
            ArtistIds = new List<int> { artist.Id },
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AlbumId);

        var saved = await DbContext.Albums.FindAsync(result.AlbumId);
        Assert.NotNull(saved);
        Assert.Equal("New Album", saved.Title);
        Assert.Equal("user-abc", saved.CreatedBy);
    }

    [Fact]
    public async Task AddAsync_SavesDescription_WhenDescriptionProvided()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new AddAlbumRequest
        {
            Title = "New Album",
            ArtistIds = new List<int> { artist.Id },
            Description = "Album description.",
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(result.AlbumId!.Value);
        Assert.Equal("Album description.", getResult.AlbumDetails!.Description);
    }

    [Fact]
    public async Task AddAsync_SavesMultipleArtists_WhenMultipleArtistIdsProvided()
    {
        // Arrange
        var artist1 = SeedArtist("Artist One");
        var artist2 = SeedArtist("Artist Two");

        var request = new AddAlbumRequest
        {
            Title = "Collaboration Album",
            ArtistIds = new List<int> { artist1.Id, artist2.Id },
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(result.AlbumId!.Value);
        Assert.Equal(2, getResult.AlbumDetails!.Artists.Count);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        var request = new AddAlbumRequest
        {
            Title = "New Album",
            ArtistIds = new List<int> { 999 },
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.AlbumId);
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
        var album = SeedAlbum(new List<int> { artist.Id }, "Original Title");

        var request = new UpdateAlbumRequest
        {
            Id = album.Id,
            Title = "Updated Title",
            ArtistIds = new List<int> { artist.Id },
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.AlbumDetails);
        Assert.Equal("Updated Title", result.AlbumDetails.Title);

        var saved = await DbContext.Albums.FindAsync(album.Id);
        Assert.Equal("user-abc", saved!.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesArtists_WhenArtistsChange()
    {
        // Arrange
        var artist1 = SeedArtist("Artist One");
        var artist2 = SeedArtist("Artist Two");
        var album = SeedAlbum(new List<int> { artist1.Id }, "Test Album");

        var request = new UpdateAlbumRequest
        {
            Id = album.Id,
            Title = "Test Album",
            ArtistIds = new List<int> { artist2.Id },
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Single(result.AlbumDetails!.Artists);
        Assert.Equal("Artist Two", result.AlbumDetails.Artists.First().Name);
    }

    [Fact]
    public async Task UpdateAsync_AddsDescription_WhenDescriptionWasNull()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, description: null);

        var request = new UpdateAlbumRequest
        {
            Id = album.Id,
            Title = album.Title,
            ArtistIds = new List<int> { artist.Id },
            Description = "New description.",
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(album.Id);
        Assert.Equal("New description.", getResult.AlbumDetails!.Description);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesDescription_WhenDescriptionExists()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, description: "Old description.");

        var request = new UpdateAlbumRequest
        {
            Id = album.Id,
            Title = album.Title,
            ArtistIds = new List<int> { artist.Id },
            Description = "Updated description.",
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(album.Id);
        Assert.Equal("Updated description.", getResult.AlbumDetails!.Description);
    }

    [Fact]
    public async Task UpdateAsync_RemovesDescription_WhenDescriptionSetToNull()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, description: "Old description.");

        var request = new UpdateAlbumRequest
        {
            Id = album.Id,
            Title = album.Title,
            ArtistIds = new List<int> { artist.Id },
            Description = null,
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        var getResult = await _sut.GetByIdAsync(album.Id);
        Assert.True(string.IsNullOrEmpty(getResult.AlbumDetails!.Description));
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenAlbumDoesNotExist()
    {
        // Arrange
        var artist = SeedArtist();

        var request = new UpdateAlbumRequest
        {
            Id = 999,
            Title = "Updated Title",
            ArtistIds = new List<int> { artist.Id },
            IsCompilation = false,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.AlbumDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id });

        var request = new UpdateAlbumRequest
        {
            Id = album.Id,
            Title = "Updated Title",
            ArtistIds = new List<int> { 999 },
            IsCompilation = false,
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
    public async Task DeleteAsync_ReturnsSuccess_WhenAlbumHasNoSongs()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id });

        // Act
        var result = await _sut.DeleteAsync(album.Id);

        // Assert
        Assert.True(result.Success);
        var deleted = await DbContext.Albums.FindAsync(album.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_CascadesDescription_WhenAlbumHasDescription()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id }, description: "Some description.");

        // Act
        var result = await _sut.DeleteAsync(album.Id);

        // Assert
        Assert.True(result.Success);
        var description = DbContext.Set<AlbumDescription>().FirstOrDefault(d => d.AlbumId == album.Id);
        Assert.Null(description);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenAlbumHasSongs()
    {
        // Arrange
        var artist = SeedArtist();
        var album = SeedAlbum(new List<int> { artist.Id });
        SeedSong(artist.Id, albumId: album.Id);

        // Act
        var result = await _sut.DeleteAsync(album.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        var notDeleted = await DbContext.Albums.FindAsync(album.Id);
        Assert.NotNull(notDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenAlbumDoesNotExist()
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
    public async Task ListAsync_ReturnsAllAlbums_WhenNoFiltersApplied()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id }, "Album A");
        SeedAlbum(new List<int> { artist.Id }, "Album B");
        SeedAlbum(new List<int> { artist.Id }, "Album C");

        var request = new ListAlbumsRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Albums.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByArtistId()
    {
        // Arrange
        var artist1 = SeedArtist("Artist One");
        var artist2 = SeedArtist("Artist Two");
        SeedAlbum(new List<int> { artist1.Id }, "Album A");
        SeedAlbum(new List<int> { artist1.Id }, "Album B");
        SeedAlbum(new List<int> { artist2.Id }, "Album C");

        var request = new ListAlbumsRequest { ArtistId = artist1.Id, PageNumber = 1, PageSize = 10 };

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
        var album1 = SeedAlbum(new List<int> { artist.Id }, "Album A");
        var album2 = SeedAlbum(new List<int> { artist.Id }, "Album B");
        SeedAlbum(new List<int> { artist.Id }, "Album C");

        // Add songs with the genre to album1 and album2
        SeedSong(artist.Id, albumId: album1.Id, genreIds: new List<int> { genre.Id });
        SeedSong(artist.Id, albumId: album2.Id, genreIds: new List<int> { genre.Id });

        var request = new ListAlbumsRequest { GenreId = genre.Id, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FiltersByTitleSearch()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id }, "Greatest Hits");
        SeedAlbum(new List<int> { artist.Id }, "Greatest Ballads");
        SeedAlbum(new List<int> { artist.Id }, "Live at Wembley");

        var request = new ListAlbumsRequest { TitleSearch = "Greatest", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_ReturnsAlbumsOrderedByTitle()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id }, "Zebra Album");
        SeedAlbum(new List<int> { artist.Id }, "Apple Album");
        SeedAlbum(new List<int> { artist.Id }, "Mango Album");

        var request = new ListAlbumsRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Apple Album", result.Albums.ElementAt(0).Title);
        Assert.Equal("Mango Album", result.Albums.ElementAt(1).Title);
        Assert.Equal("Zebra Album", result.Albums.ElementAt(2).Title);
    }

    [Fact]
    public async Task ListAsync_ReturnsCorrectPage()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id }, "Album A");
        SeedAlbum(new List<int> { artist.Id }, "Album B");
        SeedAlbum(new List<int> { artist.Id }, "Album C");
        SeedAlbum(new List<int> { artist.Id }, "Album D");
        SeedAlbum(new List<int> { artist.Id }, "Album E");

        var request = new ListAlbumsRequest { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Albums.Count);
        Assert.Equal("Album C", result.Albums.ElementAt(0).Title);
        Assert.Equal("Album D", result.Albums.ElementAt(1).Title);
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyList_WhenNoAlbumsMatch()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id }, "Test Album");

        var request = new ListAlbumsRequest { TitleSearch = "Nonexistent", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Albums);
    }
}
