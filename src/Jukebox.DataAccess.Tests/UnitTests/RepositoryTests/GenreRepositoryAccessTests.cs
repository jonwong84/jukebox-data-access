using Jukebox.DataAccess.Contracts.DataContracts.Genre;
using Jukebox.DataAccess.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace Jukebox.DataAccess.Tests.UnitTests.RepositoryTests;

public class GenreRepositoryAccessTests : RepositoryTestBase
{
    private readonly GenreRepositoryAccess _sut;

    public GenreRepositoryAccessTests()
    {
        _sut = new GenreRepositoryAccess(DbContext, NullLogger<GenreRepositoryAccess>.Instance);
    }

    // -------------------------------------------------------------------------
    // GetByIdAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ReturnsSuccess_WhenGenreExists()
    {
        // Arrange
        var genre = SeedGenre("Rock", description: "Guitar-driven music.");

        // Act
        var result = await _sut.GetByIdAsync(genre.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.GenreDetails);
        Assert.Equal(genre.Id, result.GenreDetails.Id);
        Assert.Equal("Rock", result.GenreDetails.Name);
        Assert.Equal("Guitar-driven music.", result.GenreDetails.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsFailure_WhenGenreDoesNotExist()
    {
        // Act
        var result = await _sut.GetByIdAsync(999);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.GenreDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEmptyDescription_WhenDescriptionIsNull()
    {
        // Arrange
        var genre = SeedGenre("Rock");

        // Act
        var result = await _sut.GetByIdAsync(genre.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(string.Empty, result.GenreDetails!.Description);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsParentGenre_WhenParentExists()
    {
        // Arrange
        var parent = SeedGenre("Music");
        var child = SeedGenre("Rock", parentGenreId: parent.Id);

        // Act
        var result = await _sut.GetByIdAsync(child.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.GenreDetails!.ParentGenre);
        Assert.Equal(parent.Id, result.GenreDetails.ParentGenre.Id);
        Assert.Equal("Music", result.GenreDetails.ParentGenre.Name);
        Assert.Equal(parent.Id, result.GenreDetails.ParentGenreId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullParentGenre_WhenGenreHasNoParent()
    {
        // Arrange
        var genre = SeedGenre("Rock");

        // Act
        var result = await _sut.GetByIdAsync(genre.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.GenreDetails!.ParentGenre);
        Assert.Null(result.GenreDetails.ParentGenreId);
    }

    [Fact]
    public async Task GetByIdAsync_LoadsSubGenres_WhenSubGenresExist()
    {
        // Arrange
        var parent = SeedGenre("Rock");
        SeedGenre("Classic Rock", parentGenreId: parent.Id);
        SeedGenre("Indie Rock", parentGenreId: parent.Id);

        // Act
        var result = await _sut.GetByIdAsync(parent.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.GenreDetails!.SubGenres.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEmptySubGenres_WhenGenreHasNoSubGenres()
    {
        // Arrange
        var genre = SeedGenre("Rock");

        // Act
        var result = await _sut.GetByIdAsync(genre.Id);

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.GenreDetails!.SubGenres);
    }

    // -------------------------------------------------------------------------
    // AddAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var request = new AddGenreRequest
        {
            Name = "Jazz",
            Description = "Improvisation-based music.",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.GenreId > 0);

        var saved = await DbContext.Genres.FindAsync(result.GenreId);
        Assert.NotNull(saved);
        Assert.Equal("Jazz", saved.Name);
        Assert.Equal("Improvisation-based music.", saved.Description);
        Assert.Equal("user-abc", saved.CreatedBy);
        Assert.Null(saved.ParentGenreId);
    }

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenParentGenreIdIsProvided()
    {
        // Arrange
        var parent = SeedGenre("Music");

        var request = new AddGenreRequest
        {
            Name = "Jazz",
            ParentGenreId = parent.Id,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);

        var saved = await DbContext.Genres.FindAsync(result.GenreId);
        Assert.Equal(parent.Id, saved!.ParentGenreId);
    }

    [Fact]
    public async Task AddAsync_ReturnsSuccess_WhenDescriptionIsNull()
    {
        // Arrange
        var request = new AddGenreRequest
        {
            Name = "Jazz",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.True(result.Success);
        var saved = await DbContext.Genres.FindAsync(result.GenreId);
        Assert.Null(saved!.Description);
    }

    [Fact]
    public async Task AddAsync_ReturnsFailure_WhenParentGenreDoesNotExist()
    {
        // Arrange
        var request = new AddGenreRequest
        {
            Name = "Jazz",
            ParentGenreId = 999,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.AddAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Equal(0, result.GenreId);
    }

    // -------------------------------------------------------------------------
    // UpdateAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ReturnsSuccess_WhenRequestIsValid()
    {
        // Arrange
        var genre = SeedGenre("Rock", description: "Original description.");

        var request = new UpdateGenreRequest
        {
            Id = genre.Id,
            Name = "Updated Rock",
            Description = "Updated description.",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.GenreDetails);
        Assert.Equal("Updated Rock", result.GenreDetails.Name);
        Assert.Equal("Updated description.", result.GenreDetails.Description);

        var saved = await DbContext.Genres.FindAsync(genre.Id);
        Assert.Equal("user-abc", saved!.UpdatedBy);
        Assert.NotNull(saved.UpdatedAt);
    }

    [Fact]
    public async Task UpdateAsync_ClearsDescription_WhenDescriptionSetToNull()
    {
        // Arrange
        var genre = SeedGenre("Rock", description: "Old description.");

        var request = new UpdateGenreRequest
        {
            Id = genre.Id,
            Name = "Rock",
            Description = null,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        var saved = await DbContext.Genres.FindAsync(genre.Id);
        Assert.Null(saved!.Description);
    }

    [Fact]
    public async Task UpdateAsync_SetsParentGenre_WhenParentGenreIdIsProvided()
    {
        // Arrange
        var parent = SeedGenre("Music");
        var genre = SeedGenre("Rock");

        var request = new UpdateGenreRequest
        {
            Id = genre.Id,
            Name = "Rock",
            ParentGenreId = parent.Id,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.GenreDetails!.ParentGenre);
        Assert.Equal(parent.Id, result.GenreDetails.ParentGenre.Id);
    }

    [Fact]
    public async Task UpdateAsync_ClearsParentGenre_WhenParentGenreIdSetToNull()
    {
        // Arrange
        var parent = SeedGenre("Music");
        var genre = SeedGenre("Rock", parentGenreId: parent.Id);

        var request = new UpdateGenreRequest
        {
            Id = genre.Id,
            Name = "Rock",
            ParentGenreId = null,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.GenreDetails!.ParentGenre);
        Assert.Null(result.GenreDetails.ParentGenreId);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenGenreDoesNotExist()
    {
        // Arrange
        var request = new UpdateGenreRequest
        {
            Id = 999,
            Name = "Updated Name",
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.GenreDetails);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenParentGenreDoesNotExist()
    {
        // Arrange
        var genre = SeedGenre("Rock");

        var request = new UpdateGenreRequest
        {
            Id = genre.Id,
            Name = "Rock",
            ParentGenreId = 999,
            UserId = "user-abc"
        };

        // Act
        var result = await _sut.UpdateAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFailure_WhenGenreSetAsItsOwnParent()
    {
        // Arrange
        var genre = SeedGenre("Rock");

        var request = new UpdateGenreRequest
        {
            Id = genre.Id,
            Name = "Rock",
            ParentGenreId = genre.Id,
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
    public async Task DeleteAsync_ReturnsSuccess_WhenGenreHasNoSubGenresOrSongs()
    {
        // Arrange
        var genre = SeedGenre("Rock");

        // Act
        var result = await _sut.DeleteAsync(genre.Id);

        // Assert
        Assert.True(result.Success);
        var deleted = await DbContext.Genres.FindAsync(genre.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenGenreHasSubGenres()
    {
        // Arrange
        var parent = SeedGenre("Rock");
        SeedGenre("Classic Rock", parentGenreId: parent.Id);

        // Act
        var result = await _sut.DeleteAsync(parent.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        var notDeleted = await DbContext.Genres.FindAsync(parent.Id);
        Assert.NotNull(notDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenGenreHasSongs()
    {
        // Arrange
        var genre = SeedGenre("Rock");
        var artist = SeedArtist();
        SeedSong(artist.Id, genreIds: new List<int> { genre.Id });

        // Act
        var result = await _sut.DeleteAsync(genre.Id);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        var notDeleted = await DbContext.Genres.FindAsync(genre.Id);
        Assert.NotNull(notDeleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFailure_WhenGenreDoesNotExist()
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
    public async Task ListAsync_ReturnsAllGenres_WhenNoFiltersApplied()
    {
        // Arrange
        SeedGenre("Rock");
        SeedGenre("Jazz");
        SeedGenre("Classical");

        var request = new ListGenresRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(3, result.Genres.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByNameSearch()
    {
        // Arrange
        SeedGenre("Classic Rock");
        SeedGenre("Classical");
        SeedGenre("Jazz");

        var request = new ListGenresRequest { NameSearch = "Classic", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task ListAsync_FiltersByParentGenreId()
    {
        // Arrange
        var parent = SeedGenre("Rock");
        SeedGenre("Classic Rock", parentGenreId: parent.Id);
        SeedGenre("Indie Rock", parentGenreId: parent.Id);
        SeedGenre("Jazz");

        var request = new ListGenresRequest { ParentGenreId = parent.Id, PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Genres, g => Assert.Equal(parent.Id, g.ParentGenreId));
    }

    [Fact]
    public async Task ListAsync_ReturnsGenresOrderedByName()
    {
        // Arrange
        SeedGenre("Rock");
        SeedGenre("Jazz");
        SeedGenre("Classical");

        var request = new ListGenresRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Classical", result.Genres.ElementAt(0).Name);
        Assert.Equal("Jazz", result.Genres.ElementAt(1).Name);
        Assert.Equal("Rock", result.Genres.ElementAt(2).Name);
    }

    [Fact]
    public async Task ListAsync_ReturnsCorrectPage()
    {
        // Arrange
        SeedGenre("Blues");
        SeedGenre("Classical");
        SeedGenre("Jazz");
        SeedGenre("Pop");
        SeedGenre("Rock");

        var request = new ListGenresRequest { PageNumber = 2, PageSize = 2 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Genres.Count);
        Assert.Equal("Jazz", result.Genres.ElementAt(0).Name);
        Assert.Equal("Pop", result.Genres.ElementAt(1).Name);
    }

    [Fact]
    public async Task ListAsync_ReturnsEmptyList_WhenNoGenresMatch()
    {
        // Arrange
        SeedGenre("Rock");

        var request = new ListGenresRequest { NameSearch = "Nonexistent", PageNumber = 1, PageSize = 10 };

        // Act
        var result = await _sut.ListAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Genres);
    }
}