using Jukebox.DataAccess.Contracts.DataContracts.Album;

namespace Jukebox.DataAccess.Interfaces;

/// <summary>
/// Provides data access operations for <see cref="Album"/> entities.
/// </summary>
public interface IAlbumRepositoryAccess
{
    /// <summary>
    /// Retrieves a single album by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the album to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="GetAlbumResult"/> indicating success or failure of the operation.</returns>
    Task<GetAlbumResult> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new album to the data store.
    /// </summary>
    /// <param name="request">The request containing album details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the add operation.</returns>
    Task<AddAlbumResult> AddAsync(AddAlbumRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing album in the data store.
    /// </summary>
    /// <param name="request">The request containing updated album details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="UpdateAlbumResult"/> containing the updated album details if successful.</returns>
    Task<UpdateAlbumResult> UpdateAsync(UpdateAlbumRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an album by its unique identifier.
    /// </summary>
    /// <param name="albumId">The ID of the album to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The result of the delete operation.</returns>
    Task<DeleteAlbumResult> DeleteAsync(int albumId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a list of albums.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<ListAlbumsResult> ListAsync(ListAlbumsRequest request, CancellationToken cancellationToken = default);
}