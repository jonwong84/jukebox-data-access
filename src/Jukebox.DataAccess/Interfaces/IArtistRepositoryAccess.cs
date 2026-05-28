using Jukebox.DataAccess.Contracts.DataContracts.Artist;

namespace Jukebox.DataAccess.Interfaces;

/// <summary>
/// Provides data access operations for <see cref="Artist"/> entities.
/// </summary>
public interface IArtistRepositoryAccess
{
    /// <summary>
    /// Retrieves a single artist by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the artist to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="GetArtistResult"/> indicating success or failure of the operation.</returns>
    Task<GetArtistResult> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new artist to the data store.
    /// </summary>
    /// <param name="request">The request containing artist details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the add operation.</returns>
    Task<AddArtistResult> AddAsync(AddArtistRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing artist in the data store.
    /// </summary>
    /// <param name="request">The request containing updated artist details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="UpdateArtistResult"/> containing the updated artist details if successful.</returns>
    Task<UpdateArtistResult> UpdateAsync(UpdateArtistRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an artist by its unique identifier.
    /// </summary>
    /// <param name="artistId">The ID of the artist to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the delete operation.</returns>
    Task<DeleteArtistResult> DeleteAsync(int artistId, CancellationToken cancellationToken = default);
}