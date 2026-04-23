using JukeboxDataAccess.Models;
using JukeboxDataAccess.Requests;

namespace JukeboxDataAccess.Interfaces;

/// <summary>
/// Provides data access operations for <see cref="Song"/> entities.
/// </summary>
public interface ISongRepository
{
    /// <summary>
    /// Retrieves a single song by its unique identifier.
    /// </summary>
    /// <param name="request">The request containing the song ID to look up.</param>
    /// <returns>The matching <see cref="Song"/>, or <c>null</c> if not found.</returns>
    Task<Song?> GetByIdAsync(GetSongRequest request);

    /// <summary>
    /// Searches for songs using the criteria provided in <paramref name="request"/>.
    /// </summary>
    /// <param name="request">The search criteria (title, artist, genre, etc.).</param>
    /// <returns>A collection of songs that match the search criteria.</returns>
    Task<IEnumerable<Song>> SearchAsync(SearchSongsRequest request);
}
