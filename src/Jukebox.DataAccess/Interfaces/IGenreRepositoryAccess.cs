using Jukebox.DataAccess.Contracts.DataContracts.Genre;

namespace Jukebox.DataAccess.Contracts.Interfaces;

public interface IGenreRepositoryAccess
{
    Task<GetGenreResult> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<AddGenreResult> AddAsync(AddGenreRequest request, CancellationToken cancellationToken = default);
    Task<UpdateGenreResult> UpdateAsync(UpdateGenreRequest request, CancellationToken cancellationToken = default);
    Task<DeleteGenreResult> DeleteAsync(int genreId, CancellationToken cancellationToken = default);
    Task<ListGenresResult> ListAsync(ListGenresRequest request, CancellationToken cancellationToken = default);
}