using JukeboxDataAccess.DataModels;
using JukeboxDataAccess.Mappers;
using JukeboxDataAccess.Models;

namespace JukeboxDataAccess.Tests.Mappers;

public class SongMapperTests
{
    private readonly SongMapper _mapper = new();

    [Fact]
    public void Map_ShouldMapAllPropertiesCorrectly()
    {
        var dataModel = new SongDataModel
        {
            Id = 1,
            Title = "Bohemian Rhapsody",
            Artist = "Queen",
            Album = "A Night at the Opera",
            Genre = "Rock",
            DurationSeconds = 354,
            ReleaseYear = 1975
        };

        Song result = _mapper.Map(dataModel);

        Assert.Equal(dataModel.Id, result.Id);
        Assert.Equal(dataModel.Title, result.Title);
        Assert.Equal(dataModel.Artist, result.Artist);
        Assert.Equal(dataModel.Album, result.Album);
        Assert.Equal(dataModel.Genre, result.Genre);
        Assert.Equal(dataModel.DurationSeconds, result.DurationSeconds);
        Assert.Equal(dataModel.ReleaseYear, result.ReleaseYear);
    }

    [Fact]
    public void Map_NullSource_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _mapper.Map(null!));
    }
}
