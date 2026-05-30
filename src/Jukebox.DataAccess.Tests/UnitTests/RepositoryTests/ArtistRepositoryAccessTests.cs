using Jukebox.DataAccess.Artists;
using Jukebox.DataAccess.Contracts.DataContracts.Artist;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jukebox.DataAccess.Tests.UnitTests.RepositoryTests;

public class ArtistRepositoryAccessTests : RepositoryTestBase
{
    private readonly ArtistRepositoryAccess _sut;

    public ArtistRepositoryAccessTests()
    {
        _sut = new ArtistRepositoryAccess(DbContext, NullLogger<ArtistRepositoryAccess>.Instance);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WhenArtistExists()
    {
        // Arrange
        var artist = SeedArtist("Test Artist", bio: "A great artist.");

        // Act
        var result = await _sut.GetByIdAsync(artist.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ArtistDetails);
        Assert.Equal(artist.Id, result.ArtistDetails.Id);
        Assert.Equal("Test Artist", result.ArtistDetails.Name);
        Assert.Equal("A great artist.", result.ArtistDetails.Bio);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.ArtistDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsAlbums_WhenArtistHasAlbums()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id }, "Album One");
        SeedAlbum(new List<int> { artist.Id }, "Album Two");

        // Act
        var result = await _sut.GetByIdAsync(artist.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.ArtistDetails!.Albums.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEmptyAlbums_WhenArtistHasNoAlbums()
    {
        // Arrange
        var artist = SeedArtist();

        // Act
        var result = await _sut.GetByIdAsync(artist.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.ArtistDetails!.Albums);
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var request = new AddArtistRequest
        {
            Name = "New Artist",
            Bio = "Some bio.",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ArtistId);

        var saved = await DbContext.Artists.FindAsync(result.ArtistId);
        Assert.NotNull(saved);
        Assert.Equal("New Artist", saved.Name);
        Assert.Equal("Some bio.", saved.Bio);
        Assert.Equal("user-abc", saved.CreatedBy);
    }

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenBioIsNull()
    {
        // Arrange
        var request = new AddArtistRequest
        {
            Name = "New Artist",
            Bio = null,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        var saved = await DbContext.Artists.FindAsync(result.ArtistId);
        Assert.Null(saved!.Bio);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var artist = SeedArtist("Original Name", bio: "Original bio.");

        var request = new UpdateArtistRequest
        {
            Id = artist.Id,
            Name = "Updated Name",
            Bio = "Updated bio.",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.ArtistDetails);
        Assert.Equal("Updated Name", result.ArtistDetails.Name);
        Assert.Equal("Updated bio.", result.ArtistDetails.Bio);

        var saved = await DbContext.Artists.FindAsync(artist.Id);
        Assert.Equal("user-abc", saved!.UpdatedBy);
    }

    [Fact]
    public async Task UpdateAsync_ClearsBio_WhenBioSetToNull()
    {
        // Arrange
        var artist = SeedArtist("Artist", bio: "Old bio.");

        var request = new UpdateArtistRequest
        {
            Id = artist.Id,
            Name = "Artist",
            Bio = null,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        var saved = await DbContext.Artists.FindAsync(artist.Id);
        Assert.Null(saved!.Bio);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenArtistDoesNotExist()
    {
        // Arrange
        var request = new UpdateArtistRequest
        {
            Id = 999,
            Name = "Updated Name",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.ArtistDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    // -------------------------------------------------------------------------
    // DeleteAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ReturnsSuccess_WhenArtistHasNoSongsOrAlbums()
    {
        // Arrange
        var artist = SeedArtist();

        // Act
        var result = await _sut.DeleteAsync(artist.Id);

        // Assert
        Assert.True(result.Success);
        var deleted = await DbContext.Artists.FindAsync(artist.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenArtistHasSongs()
    {
        // Arrange
        var artist = SeedArtist();
        SeedSong(artist.Id);

        // Act
        var result = await _sut.DeleteAsync(artist.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        var notDeleted = await DbContext.Artists.FindAsync(artist.Id);
        Assert.NotNull(notDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenArtistHasAlbumAssociations()
    {
        // Arrange
        var artist = SeedArtist();
        SeedAlbum(new List<int> { artist.Id });

        // Act
        var result = await _sut.DeleteAsync(artist.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        var notDeleted = await DbContext.Artists.FindAsync(artist.Id);
        Assert.NotNull(notDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenArtistDoesNotExist()
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
    public async Task ListAsync_ReturnsAllArtists_WhenNoFiltersApplied()
    {
        // Arrange
        SeedArtist("Artist A");
        SeedArtist("Artist B");
        SeedArtist("Artist C");

        var request = new ListArtistsRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Artists.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByNameSearch()
    {
        // Arrange
        SeedArtist("The Beatles");
        SeedArtist("The Rolling Stones");
        SeedArtist("Led Zeppelin");

        var request = new ListArtistsRequest { NameSearch = "The", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_ReturnsArtistsOrderedByName()
    {
        // Arrange
        SeedArtist("Zebra");
        SeedArtist("Apple");
        SeedArtist("Mango");

        var request = new ListArtistsRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Apple", result.Artists.ElementAt(0).Name);
        Assert.Equal("Mango", result.Artists.ElementAt(1).Name);
        Assert.Equal("Zebra", result.Artists.ElementAt(2).Name);
    }

    [Fact]
    public async Task ListAsync_ReturnsCorrectPage()
    {
        // Arrange
        SeedArtist("Artist A");
        SeedArtist("Artist B");
        SeedArtist("Artist C");
        SeedArtist("Artist D");
        SeedArtist("Artist E");

        var request = new ListArtistsRequest { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Artists.Count);
        Assert.Equal("Artist C", result.Artists.ElementAt(0).Name);
        Assert.Equal("Artist D", result.Artists.ElementAt(1).Name);
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyList_WhenNoArtistsMatch()
    {
        // Arrange
        SeedArtist("Test Artist");

        var request = new ListArtistsRequest { NameSearch = "Nonexistent", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Artists);
    }
}
