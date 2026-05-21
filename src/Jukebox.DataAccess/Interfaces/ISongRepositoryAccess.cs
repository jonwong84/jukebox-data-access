using Jukebox.DataAccess.Contracts.DataContracts.Song;
using Jukebox.DataAccess.EntityFramework.Models;

namespace Jukebox.DataAccess.Interfaces;

/// <summary>
/// Provides data access operations for <see cref="Song"/> entities.
/// </summary>
public interface ISongRepositoryAccess
{
    /// <summary>
    /// Retrieves a single song by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the song to look up.</param>
    /// <returns>The matching <see cref="SongSummary"/>, or <c>null</c> if not found.</returns>
    Task<SongSummary?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new song to the data store.
    /// </summary>
    /// <param name="request">The request containing song details.</param>
    /// <returns>The result of the add operation.</returns>
    Task<AddSongResult> AddAsync(AddSongRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing song in the data store.
    /// </summary>
    /// <param name="request">The request containing updated song details.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    Task<UpdateSongResult> UpdateAsync(UpdateSongRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a song by its unique identifier.
    /// </summary>
    /// <param name="songId">The ID of the song to delete.</param>
    /// <returns>True if the deletion was successful; otherwise, false.</returns>
    Task<bool> DeleteAsync(int songId, CancellationToken cancellationToken = default);
}
