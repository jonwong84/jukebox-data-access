using JukeboxDataAccess.DataModels;
using JukeboxDataAccess.Interfaces;
using JukeboxDataAccess.Models;

namespace JukeboxDataAccess.Mappers;

/// <summary>
/// Maps a <see cref="SongDataModel"/> returned from the database to a <see cref="Song"/> domain model.
/// </summary>
public class SongMapper : IMapper<SongDataModel, Song>
{
    /// <inheritdoc />
    public Song Map(SongDataModel source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Song
        {
            Id = source.Id,
            Title = source.Title,
            Artist = source.Artist,
            Album = source.Album,
            Genre = source.Genre,
            DurationSeconds = source.DurationSeconds,
            ReleaseYear = source.ReleaseYear
        };
    }
}
